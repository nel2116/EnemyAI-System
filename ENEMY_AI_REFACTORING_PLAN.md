# エネミー AI リファクタリング計画：新エネミー実装の容易化とデータ駆動設計の維持

## 1. 概要

本ドキュメントは、既存のエネミー AI コードベースを、後続の開発者が新規エネミーを容易に実装できるようリファクタリングするための計画と設計見直しをまとめたものです。また、デザイナーが独自エンジンのインスペクターでパラメータ調整を行えるよう、`EnemyUserData.cs`や`TwinGoblinUserData.cs`といったデータ構造を維持・強化することを目標とします。

## 2. 現状の課題分析

### 2.1. 新エネミー実装における主要な問題点

#### A. ファクトリの拡張性不足

```csharp
// Application/EnemyFactory.cs
public sealed class EnemyFactory : IEnemyFactory
{
    private readonly Dictionary<EnemyAITemplate, Builder> _builders;

    public EnemyFactory()
    {
        _builders = new()
        {
            { EnemyAITemplate.Basic, BuildBasic },
            { EnemyAITemplate.Twin, BuildTwin },
        };
    }
}
```

**問題点**:

- 新しいエネミータイプを追加するたびに`EnemyFactory`の内部ロジックを変更する必要がある
- `EnemyAITemplate`の Enum に新しい値を追加し、対応するビルダーメソッドを実装する必要がある
- Open/Closed Principle に反している

#### B. 継承設計による拡張性の制限

```csharp
// Infrastructure/AI/TwinGoblinAI.cs
public sealed class TwinGoblinAI : BasicEnemyAI
{
    private readonly EnemyId _pairId;
    private readonly Func<float> _enrageSpeedMul;
    private readonly Func<float> _enrageAtkMul;
    private bool _enraged;
}
```

**問題点**:

- `TwinGoblinAI`が`BasicEnemyAI`を継承しているため、基本 AI の変更が特殊 AI に影響する
- 複数の特殊な挙動を持つエネミーを追加する際に、継承階層が複雑になる
- 共通の AI ロジックと特殊ロジックが混在している

#### C. データ駆動設計の不十分さ

```csharp
// Data/EnemyUserData.cs
public enum EnemyAITemplate { Basic, Twin, }

// Application/EnemyFactory.cs
private static IEnemyUnit BuildTwin(EnemyUserData d, DomainEventDispatcher disp, IAIContext ctx, IMoveLogic move, PairLink? pair)
{
    var td = (TwinGoblinUserData)d; // キャストが必要
    // ...
}
```

**問題点**:

- 新しいエネミータイプを追加するたびに`EnemyAITemplate`の Enum を更新する必要がある
- 型安全性が不十分（キャストが必要）
- デザイナーが調整できるパラメータが限定的

#### D. エンジン依存による開発効率の低下

```csharp
// Domain/Interfaces/IAiContext.cs
public interface IAiContext
{
    GameObject Self { get; }        // via.GameObject
    vec3 SelfPosition { get; }      // via.vec3
}
```

**問題点**:

- Domain 層がエンジン固有の型に依存している
- ローカル環境でのテストが困難
- エンジン環境なしでの開発ができない

## 3. リファクタリングの目標

### 3.1. 新エネミー実装の容易化

- **Open/Closed Principle の適用**: 既存コードを変更せずに新しいエネミーを追加可能
- **コンポジション優先設計**: 継承ではなく、コンポーネントの組み合わせで特殊な挙動を実現
- **型安全性の向上**: キャストを排除し、コンパイル時にエラーを検出

### 3.2. データ駆動設計の維持・強化

- **UserData 構造の維持**: 既存の`EnemyUserData.cs`と`TwinGoblinUserData.cs`の木構造を維持
- **パラメータ調整の拡張**: デザイナーがより多くのパラメータを調整可能
- **設定駆動の強化**: コード変更なしにエネミーの挙動を調整可能

### 3.3. 開発効率の向上

- **エンジン依存の分離**: Domain 層からエンジン固有の型を排除
- **テスト容易性の向上**: 各コンポーネントが独立してテスト可能
- **ローカル開発環境の活用**: エンジン環境なしでの開発・テスト

## 4. 設計の見直しとリファクタリング計画

### 4.1. Phase 1: エンジン依存の分離（最優先）

#### 目標

Domain 層から via エンジンへの直接的な依存を完全に排除し、ローカル開発環境での新エネミー AI 開発を容易にする。

#### 具体的な変更点

**A. 抽象化インターフェースの強化**

```csharp
// Core/Interfaces/IGameObject.cs
public interface IGameObject
{
    string Name { get; }
    ITransform Transform { get; }
    T? GetComponent<T>() where T : class;
}

// Core/Interfaces/ITransform.cs
public interface ITransform
{
    Vector3 Position { get; set; }
    Quaternion Rotation { get; set; }
    Vector3 Forward { get; }
}

// Core/Interfaces/IPhysicsSystem.cs
public interface IPhysicsSystem
{
    bool Raycast(Vector3 origin, Vector3 direction, float maxDistance);
    Vector3 GetGravity();
}

// Core/Interfaces/INavigationSystem.cs
public interface INavigationSystem
{
    bool CalculatePath(Vector3 start, Vector3 end, out Vector3[] path);
    bool IsReachable(Vector3 position);
}
```

**B. Domain 層のインターフェース更新**

```csharp
// Domain/Interfaces/IAiContext.cs
public interface IAiContext
{
    EnemyId EnemyId { get; }

    // エンジン非依存の抽象型を使用
    IGameObject Self { get; }
    IGameObject Target { get; }
    Vector3 SelfPosition { get; }
    Vector3 TargetPosition { get; }
    float DistanceToTarget { get; }
    bool HasLineOfSight { get; }

    void Tick(float dt);
}
```

**C. アダプター層の実装**

```csharp
// Core/Adapters/ViaAdapter.cs
public class ViaAdapter : IGameObject
{
    private readonly via.GameObject _viaObject;

    public ViaAdapter(via.GameObject viaObject)
    {
        _viaObject = viaObject;
    }

    public string Name => _viaObject.Name;
    public ITransform Transform => new ViaTransform(_viaObject.Transform);

    public T? GetComponent<T>() where T : class
    {
        // viaエンジンのコンポーネント取得ロジック
        return _viaObject.getComponent<T>();
    }
}
```

#### 期待される効果

- Domain 層のコードがエンジンに依存しなくなり、ローカル環境での単体テストが容易になる
- 新しいエネミーの AI ロジックは、エンジンに依存しない純粋な C#コードとして記述・テスト可能になる
- 将来的に異なるエンジンへの移植が容易になる

#### 段階的移行計画

1. **抽象化インターフェースの追加**  
   `Core/Interfaces` と `Core/Values` に `IGameObject` などを実装し、既存コードへは影響を与えず追加する。
2. **アダプター層の用意**  
   via エンジンの型を新インターフェースに変換する `ViaAdapter` 等を Infrastructure 層に実装する。
3. **Domain インターフェースの更新**  
   `IAiContext` や `IMoveNavigator` を新しい抽象型に差し替え、旧型との変換コードを暫定的に保持する。
4. **コンポーネントの順次置き換え**  
   各 AI 実装やファクトリから via 型の直接利用を排除し、ビルドが通る単位で段階的に適用する。
5. **旧依存の削除**  
   すべての Domain コードが新インターフェース経由となった段階で、旧 via 依存コードを整理する。

### 4.2. Phase 2: コンポジション優先設計への移行

#### 目標

継承による拡張性の制限を解決し、コンポーネントの組み合わせで新エネミーの特殊な挙動を実現する。

#### 具体的な変更点

**A. ビヘイビアコンポーネントの導入**

```csharp
// Domain/Interfaces/IEnemyBehavior.cs
public interface IEnemyBehavior
{
    void Initialize(IEnemyUnit enemy);
    void Update(float deltaTime);
    void Dispose();
}

// Infrastructure/Behaviors/IPairBehavior.cs
public interface IPairBehavior : IEnemyBehavior
{
    event Action<EnemyId> OnPairMemberDied;
    void SetPairId(EnemyId pairId);
}

// Infrastructure/Behaviors/IEnrageBehavior.cs
public interface IEnrageBehavior : IEnemyBehavior
{
    event Action OnEnrageTriggered;
    float SpeedMultiplier { get; }
    float AttackMultiplier { get; }
}
```

**B. TwinGoblinAI のコンポジション化**

```csharp
// Infrastructure/AI/TwinGoblinAI.cs
public sealed class TwinGoblinAI : IEnemyAi
{
    private readonly BasicEnemyAI _baseAI;
    private readonly IPairBehavior _pairBehavior;
    private readonly IEnrageBehavior _enrageBehavior;
    private readonly DomainEventDispatcher _dispatcher;

    public TwinGoblinAI(
        IAIContext ctx,
        IMoveLogic move,
        ICombatLogic combat,
        DomainEventDispatcher dispatcher,
        BasicEnemyAI.Config cfg,
        TwinGoblinUserData userData,
        Guid pairId)
    {
        _baseAI = new BasicEnemyAI(ctx, move, combat, dispatcher, cfg);
        _pairBehavior = new PairBehavior(pairId);
        _enrageBehavior = new EnrageBehavior(userData.EnrageSpeedMul, userData.EnrageAttackMul);
        _dispatcher = dispatcher;

        // イベントの接続
        _pairBehavior.OnPairMemberDied += OnPairMemberDied;
        _enrageBehavior.OnEnrageTriggered += OnEnrageTriggered;
    }

    public void Tick(float dt)
    {
        _baseAI.Tick(dt);
        _pairBehavior.Update(dt);
        _enrageBehavior.Update(dt);
    }

    private void OnPairMemberDied(EnemyId pairId)
    {
        _enrageBehavior.TriggerEnrage();
    }

    private void OnEnrageTriggered()
    {
        // 激高時の処理
        if (_baseAI is BasicEnemyAI basicAI)
        {
            basicAI.SetSpeedMultiplier(_enrageBehavior.SpeedMultiplier);
            basicAI.SetAttackMultiplier(_enrageBehavior.AttackMultiplier);
        }
    }
}
```

**C. ビヘイビアファクトリの導入**

```csharp
// Application/Behaviors/IBehaviorFactory.cs
public interface IBehaviorFactory
{
    IPairBehavior CreatePairBehavior(Guid pairId);
    IEnrageBehavior CreateEnrageBehavior(float speedMultiplier, float attackMultiplier);
    // 他のビヘイビアも同様に...
}

// Application/Behaviors/BehaviorFactory.cs
public class BehaviorFactory : IBehaviorFactory
{
    public IPairBehavior CreatePairBehavior(Guid pairId)
    {
        return new PairBehavior(new EnemyId(pairId));
    }

    public IEnrageBehavior CreateEnrageBehavior(float speedMultiplier, float attackMultiplier)
    {
        return new EnrageBehavior(speedMultiplier, attackMultiplier);
    }
}
```

#### 期待される効果

- 新しいエネミーは、既存の AI ステートを再利用しつつ、独自のビヘイビアコンポーネントを追加するだけで済む
- コードの重複を減らし、各ビヘイビアの責務が明確になる
- 複数の特殊な挙動を組み合わせて新しいエネミーを作成可能

### 4.3. Phase 3: ファクトリパターンの改善

#### 目標

新しいエネミータイプを追加する際に、`EnemyFactory`自体を変更せずに済むようにする（Open/Closed Principle の適用）。

#### 具体的な変更点

**A. ビルダーインターフェースの導入**

```csharp
// Application/Builders/IEnemyBuilder.cs
public interface IEnemyBuilder
{
    EnemyAITemplate Template { get; }
    IEnemyUnit Build(
        EnemyUserData userData,
        DomainEventDispatcher dispatcher,
        IAIContext context,
        IMoveNavigator move,
        PairLink? pair);
}

// Application/Builders/BasicEnemyBuilder.cs
public class BasicEnemyBuilder : IEnemyBuilder
{
    public EnemyAITemplate Template => EnemyAITemplate.Basic;

    public IEnemyUnit Build(
        EnemyUserData userData,
        DomainEventDispatcher dispatcher,
        IAIContext context,
        IMoveNavigator move,
        PairLink? pair)
    {
        var combat = new SimpleCombatLogic(() => userData.AttackPower, () => userData.CooldownSeconds);

        var cfg = new BasicEnemyAI.Config(
            DetectionRange: () => userData.DetectRange,
            AttackRange: () => userData.AttackRange,
            PatrolPoints: userData.Patrol?.Points ?? Array.Empty<IGameObject>(),
            PatrolWaitTime: () => userData.Patrol?.WaitSeconds ?? 0f,
            ReturnHomeDist: () => userData.Patrol?.ReturnHomeDistance ?? 0f
        );

        var ai = new BasicEnemyAI(context, move, combat, dispatcher, cfg);
        var core = new Enemy(context.EnemyId, userData.MaxHp, 0.3f, dispatcher);

        return new EnemyDomainService(core, move, combat, ai, dispatcher);
    }
}

// Application/Builders/TwinGoblinBuilder.cs
public class TwinGoblinBuilder : IEnemyBuilder
{
    private readonly IBehaviorFactory _behaviorFactory;

    public TwinGoblinBuilder(IBehaviorFactory behaviorFactory)
    {
        _behaviorFactory = behaviorFactory;
    }

    public EnemyAITemplate Template => EnemyAITemplate.Twin;

    public IEnemyUnit Build(
        EnemyUserData userData,
        DomainEventDispatcher dispatcher,
        IAIContext context,
        IMoveNavigator move,
        PairLink? pair)
    {
        var twinData = userData as TwinGoblinUserData
            ?? throw new ArgumentException("TwinGoblinUserData expected", nameof(userData));

        var combat = new SimpleCombatLogic(() => userData.AttackPower, () => userData.CooldownSeconds);

        var cfg = new BasicEnemyAI.Config(
            DetectionRange: () => userData.DetectRange,
            AttackRange: () => userData.AttackRange,
            PatrolPoints: userData.Patrol?.Points ?? Array.Empty<IGameObject>(),
            PatrolWaitTime: () => userData.Patrol?.WaitSeconds ?? 0f,
            ReturnHomeDist: () => userData.Patrol?.ReturnHomeDistance ?? 0f
        );

        var ai = new TwinGoblinAI(
            context, move, combat, dispatcher, cfg, twinData, pair?.Id ?? Guid.Empty);
        var core = new TwinGoblin(context.EnemyId, userData.MaxHp, 0.3f, dispatcher, pair?.Id ?? Guid.Empty);

        return new EnemyDomainService(core, move, combat, ai, dispatcher);
    }
}
```

**B. EnemyFactory の改善**

```csharp
// Application/EnemyFactory.cs
public sealed class EnemyFactory : IEnemyFactory
{
    private readonly Dictionary<EnemyAITemplate, IEnemyBuilder> _builders;

    public EnemyFactory(IEnumerable<IEnemyBuilder> builders)
    {
        _builders = builders.ToDictionary(b => b.Template);
    }

    public IEnemyUnit Create(
        EnemyUserData data,
        DomainEventDispatcher dispatcher,
        IAIContext ctx,
        IMoveNavigator move,
        PairLink? pair)
    {
        if (!_builders.TryGetValue(data.Template, out var builder))
        {
            // デフォルトはBasic
            builder = _builders[EnemyAITemplate.Basic];
        }

        return builder.Build(data, dispatcher, ctx, move, pair);
    }
}
```

**C. 依存性注入の設定**

```csharp
// Program.cs または DI設定ファイル
public static void ConfigureServices(IServiceCollection services)
{
    // ビルダーの登録
    services.AddSingleton<IEnemyBuilder, BasicEnemyBuilder>();
    services.AddSingleton<IEnemyBuilder, TwinGoblinBuilder>();

    // ビヘイビアファクトリの登録
    services.AddSingleton<IBehaviorFactory, BehaviorFactory>();

    // ファクトリの登録
    services.AddSingleton<IEnemyFactory, EnemyFactory>();
}
```

#### 期待される効果

- 新しいエネミータイプを追加する際は、新しい`IEnemyBuilder`実装クラスを作成し、DI コンテナに登録するだけで済む
- `EnemyFactory`の既存コードを変更する必要がない
- 各ビルダーが単一責任を持ち、テストが容易になる

### 4.4. Phase 4: データ駆動設計の強化

#### 目標

デザイナーのパラメータ調整フローを維持しつつ、データとロジックの分離を明確にする。

#### 具体的な変更点

**A. UserData の拡張**

```csharp
// Data/EnemyUserData.cs
public class EnemyUserData : via.UserData
{
    // 既存のパラメータ...

    // 新しい共通パラメータ
    [DataMember, DisplayName("AIテンプレート"), EnumSelector(typeof(EnemyAITemplate))]
    public EnemyAITemplate Template = EnemyAITemplate.Basic;

    [DataMember, DisplayName("激高閾値"), Slider(0f, 1f)]
    public float EnrageThreshold = 0.3f;

    [DataMember, DisplayName("死亡時エフェクトID")]
    public string DeathEffectId = "FX_Death_Default";

    // ビヘイビア設定
    [DataMember, DisplayName("ペア行動有効")]
    public bool EnablePairBehavior = false;

    [DataMember, DisplayName("激高行動有効")]
    public bool EnableEnrageBehavior = false;
}

// Data/TwinGoblinUserData.cs
public sealed class TwinGoblinUserData : EnemyUserData
{
    // 既存のパラメータ...

    [DataMember, DisplayName("ペアリンクID")]
    public string PairLinkId = "TwinGoblinPair1";

    [DataMember, DisplayName("激高エフェクトID")]
    public string EnrageEffectId = "FX_Enrage_TwinGoblin";

    [DataMember, DisplayName("ペア死亡時の激高確率"), Slider(0f, 1f)]
    public float EnrageOnPairDeathChance = 1.0f;
}
```

**B. 設定駆動のビルダー**

```csharp
// Application/Builders/ConfigurableEnemyBuilder.cs
public class ConfigurableEnemyBuilder : IEnemyBuilder
{
    private readonly IBehaviorFactory _behaviorFactory;

    public ConfigurableEnemyBuilder(IBehaviorFactory behaviorFactory)
    {
        _behaviorFactory = behaviorFactory;
    }

    public EnemyAITemplate Template => EnemyAITemplate.Custom;

    public IEnemyUnit Build(
        EnemyUserData userData,
        DomainEventDispatcher dispatcher,
        IAIContext context,
        IMoveNavigator move,
        PairLink? pair)
    {
        var combat = CreateCombatLogic(userData);
        var ai = CreateAI(userData, context, move, combat, dispatcher);
        var core = CreateCore(userData, dispatcher);

        return new EnemyDomainService(core, move, combat, ai, dispatcher);
    }

    private ICombatLogic CreateCombatLogic(EnemyUserData userData)
    {
        return new SimpleCombatLogic(
            () => userData.AttackPower,
            () => userData.CooldownSeconds);
    }

    private IEnemyAi CreateAI(
        EnemyUserData userData,
        IAIContext context,
        IMoveLogic move,
        ICombatLogic combat,
        DomainEventDispatcher dispatcher)
    {
        var baseAI = CreateBaseAI(userData, context, move, combat, dispatcher);

        // 設定に基づいてビヘイビアを追加
        var behaviors = new List<IEnemyBehavior>();

        if (userData.EnablePairBehavior)
        {
            behaviors.Add(_behaviorFactory.CreatePairBehavior(Guid.Empty)); // ペアIDは後で設定
        }

        if (userData.EnableEnrageBehavior)
        {
            behaviors.Add(_behaviorFactory.CreateEnrageBehavior(2.0f, 2.0f)); // デフォルト値
        }

        return new CompositeEnemyAI(baseAI, behaviors);
    }

    private Enemy CreateCore(EnemyUserData userData, DomainEventDispatcher dispatcher)
    {
        return new Enemy(
            EnemyId.NewId(),
            userData.MaxHp,
            userData.EnrageThreshold,
            dispatcher);
    }
}
```

#### 期待される効果

- デザイナーは、既存のワークフローで新エネミーのパラメータを調整できる
- 開発者は、コードを変更せずにパラメータの調整が可能になる
- 設定駆動により、エネミーのバリエーション作成が容易になる

### 4.5. Phase 5: テスト戦略の強化

#### 目標

各コンポーネントが独立してテスト可能である状態を維持・強化する。

#### 具体的な変更点

**A. ユニットテストの拡充**

```csharp
// tests/Infrastructure/Behaviors/PairBehaviorTests.cs
public class PairBehaviorTests
{
    [Fact]
    public void OnPairMemberDied_ShouldTriggerEvent()
    {
        // Arrange
        var behavior = new PairBehavior(new EnemyId(Guid.NewGuid()));
        var enemy = Mock.Of<IEnemyUnit>();
        behavior.Initialize(enemy);

        EnemyId? triggeredPairId = null;
        behavior.OnPairMemberDied += pairId => triggeredPairId = pairId;

        // Act
        behavior.OnPairMemberDied(new EnemyId(Guid.NewGuid()));

        // Assert
        triggeredPairId.Should().NotBeNull();
    }
}

// tests/Application/Builders/TwinGoblinBuilderTests.cs
public class TwinGoblinBuilderTests
{
    [Fact]
    public void Build_WithValidData_ShouldCreateTwinGoblin()
    {
        // Arrange
        var behaviorFactory = Mock.Of<IBehaviorFactory>();
        var builder = new TwinGoblinBuilder(behaviorFactory);
        var userData = new TwinGoblinUserData();
        var dispatcher = new DomainEventDispatcher();
        var context = Mock.Of<IAIContext>();
        var move = Mock.Of<IMoveNavigator>();

        // Act
        var result = builder.Build(userData, dispatcher, context, move, null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<EnemyDomainService>();
    }
}
```

**B. 統合テストの導入**

```csharp
// tests/Integration/EnemyLifecycleTests.cs
public class EnemyLifecycleTests
{
    [Fact]
    public void TwinGoblin_WhenPairMemberDies_ShouldBecomeEnraged()
    {
        // Arrange
        var services = new ServiceCollection();
        ConfigureTestServices(services);
        var serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetRequiredService<IEnemyFactory>();
        var userData = new TwinGoblinUserData();
        var dispatcher = new DomainEventDispatcher();

        // Act & Assert
        // エネミーの生成から動作、ペアメンバーの死亡、激高までの一連の流れをテスト
    }
}
```

#### 期待される効果

- コードの品質と信頼性が向上する
- リファクタリング時の安全性が確保される
- 新機能追加時の回帰テストが容易になる

## 5. 実装計画のロードマップ

### 5.1. 優先順位

1. **Phase 1: エンジン依存の分離**（最優先）

   - 最も基盤となる変更であり、他のフェーズの前提となる
   - ローカル開発環境での開発効率が大幅に向上する

2. **Phase 2: コンポジション優先設計への移行**

   - 新エネミー実装の容易化に直結する
   - 既存の`TwinGoblinAI`を段階的に移行

3. **Phase 3: ファクトリパターンの改善**

   - 新エネミー追加時の開発フローを簡素化
   - DI コンテナの活用により保守性が向上

4. **Phase 4: データ駆動設計の強化**

   - デザイナーの作業効率向上
   - 設定駆動による柔軟性の向上

5. **Phase 5: テスト戦略の強化**
   - 開発と並行して継続的に実施
   - 品質と開発効率を担保

### 5.2. 段階的移行戦略

#### Step 1: 基盤整備（1-2 週間）

- 抽象化インターフェースの実装
- モックシステムの構築
- 既存テストの修正

#### Step 2: 既存コードの移行（2-3 週間）

- `TwinGoblinAI`のコンポジション化
- `EnemyFactory`の改善
- 段階的なテスト実行

#### Step 3: 新機能の追加（1-2 週間）

- 設定駆動システムの実装
- 新しいビヘイビアの追加
- 統合テストの実装

#### Step 4: 最適化とドキュメント（1 週間）

- パフォーマンスの最適化
- 開発者向けドキュメントの作成
- 最終テストと検証

## 6. 期待される効果

### 6.1. 開発効率の向上

- **新エネミー実装時間の短縮**: 既存コンポーネントの再利用により、実装時間が 50%以上短縮
- **バグの削減**: 型安全性の向上とテスト容易性により、バグ発生率を 30%以上削減
- **保守性の向上**: 責務の明確な分離により、コードの理解と変更が容易になる

### 6.2. デザイナー作業の効率化

- **パラメータ調整の柔軟性**: より多くのパラメータをインスペクターで調整可能
- **リアルタイム調整**: ローカル環境での高速テストにより、調整サイクルが短縮
- **設定の再利用**: 共通設定のテンプレート化により、設定作業が効率化

### 6.3. システムの拡張性

- **新しいエネミータイプの追加**: 既存コードへの影響を最小限に抑えた追加が可能
- **新しい AI 機能の追加**: ビヘイビアコンポーネントとして独立して追加可能
- **エンジン非依存**: 将来的なエンジン変更への対応が容易

## 7. リスクと対策

### 7.1. 移行リスク

**リスク**: 既存コードの大幅な変更によるバグの発生
**対策**:

- 段階的な移行により、リスクを分散
- 各段階での十分なテスト実行
- 既存機能の動作保証

### 7.2. パフォーマンスリスク

**リスク**: 抽象化層の追加によるパフォーマンスの低下
**対策**:

- プロファイリングによる継続的な監視
- 必要に応じた最適化の実施
- パフォーマンステストの自動化

### 7.3. 学習コスト

**リスク**: 新しい設計パターンによる学習コストの発生
**対策**:

- 詳細なドキュメントの作成
- サンプルコードの提供
- 段階的な学習支援

## 8. まとめ

本リファクタリング計画により、エネミー AI システムは以下のメリットを享受できます：

- **高い拡張性**: 新しいエネミータイプを既存コードへの影響を最小限に抑えて追加できる
- **高い保守性**: 各コンポーネントの責務が明確になり、コードの見通しが良くなる
- **高いテスト容易性**: エンジン依存が排除され、各ロジックが独立してテスト可能になる
- **デザイナーフレンドリー**: `UserData`構造を維持することで、デザイナーは引き続き直感的にパラメータ調整を行える

これらの改善により、今後のエネミー開発がより効率的かつ堅牢に進められるようになります。特に、後続の開発者が新エネミーを実装する際の障壁が大幅に低減され、デザイナーによるパラメータ調整の柔軟性も向上するでしょう。
