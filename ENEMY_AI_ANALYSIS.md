# エネミー AI コード分析結果

## 概要

独自エンジン（via）に依存したエネミー AI システムの詳細分析結果です。クリーンアーキテクチャ風の設計を採用していますが、エンジン依存や継承設計など複数の問題点が存在します。

## プロジェクト構造

```
LocalCode/
├── Application/          # ユースケース層
│   ├── EnemyFactory.cs   # エネミー生成ファクトリ
│   ├── EnemyPresenterCore.cs # プレゼンターのコアロジック
│   ├── IEnemyFactory.cs  # ファクトリインターフェース
│   ├── IMassageBus.cs    # メッセージバスインターフェース
│   └── DTO/
│       └── EnemyViewModel.cs # ビューモデル
├── Domain/               # ドメイン層
│   ├── Enemy.cs          # エネミーエンティティ
│   ├── TwinGoblin.cs     # 特殊エネミー
│   ├── Services/
│   │   └── EnemyDomainServices.cs # ドメインサービス
│   ├── Interfaces/       # ドメインインターフェース
│   ├── Events/           # ドメインイベント
│   └── Enum/
│       └── AiState.cs    # AI状態列挙
├── Infrastructure/       # インフラ層
│   ├── AI/
│   │   ├── BasicEnemyAI.cs # 基本AI実装
│   │   └── TwinGoblinAI.cs # 特殊AI実装
│   ├── CustomAiContext.cs # AIコンテキスト実装
│   ├── NavigationAgent.cs # 経路移動エージェント
│   ├── NavigationMoveLogic.cs # 移動ロジック
│   ├── SimpleCombatLogic.cs # 戦闘ロジック
│   ├── WorldAdapter.cs   # ワールド情報取得
│   └── QueueMessageBus.cs # メッセージバス実装
├── Data/                 # データ層
│   ├── EnemyUserData.cs  # エネミー共通データ
│   └── TwinGoblinUserData.cs # 特殊エネミーデータ
├── Presentation/         # プレゼンテーション層
│   └── IEnemyView.cs     # Viewインターフェース
├── Via/                  # エンジン接着層
│   ├── EnemyPresenter.cs # エンジンとコアの橋渡し
│   └── SimpleEnemyView.cs # View実装
└── Util/
    └── AttackStats.cs    # 攻撃ステータス
```

## 設計の特徴・強み

### 1. アーキテクチャ設計

- **クリーンアーキテクチャ風のレイヤ分離**: Domain、Application、Infrastructure、Presentation、Via 層の明確な分離
- **依存性注入**: コンストラクタインジェクションによる依存関係の管理
- **イベント駆動設計**: ドメインイベントによる疎結合な通信

### 2. AI 設計

- **ステートパターン**: 各 AI 状態が独立したクラスで実装
- **状態遷移の明示化**: 各状態で次の状態を決定するロジック
- **設定駆動**: データクラスによる AI パラメータの外部化

### 3. パフォーマンス最適化

- **キャッシュ機能**: AI コンテキストでの位置・距離情報のキャッシュ
- **更新頻度制御**: レイキャスト等の重い処理の更新間隔制御
- **描画最適化**: ViewModel 変更検知による不要な描画の抑制

### 4. エラー処理

- **リトライ機能**: ナビゲーション失敗時の自動リトライ
- **指数バックオフ**: 失敗時の待機時間を指数関数的に増加

## 主要な問題点

### 1. エンジン依存の問題

#### 問題の詳細

- **全層で via エンジン型を使用**: `GameObject`, `vec3`, `via.Behavior`等
- **抽象化の不十分**: エンジン非依存のインターフェースが不完全
- **テスト困難**: エンジン環境なしでの単体テストが困難

#### 影響範囲

```csharp
// Domain層
public interface IAiContext
{
    GameObject Self { get; }        // via.GameObject
    vec3 SelfPosition { get; }      // via.vec3
}

// Application層
PatrolPoints: d.Patrol?.Points ?? Array.Empty<GameObject>() // via.GameObject

// Infrastructure層
public class NavigationAgent : via.Behavior
{
    private vec3 _frameMove = vec3.Zero;
    private GameObject? _destObj;
}
```

#### リファクタリング提案

```csharp
// 抽象化インターフェース
public interface IGameObject
{
    string Name { get; }
    ITransform Transform { get; }
}

public interface ITransform
{
    Vector3 Position { get; set; }
    Vector3 Rotation { get; set; }
}

public interface IAiContext
{
    IGameObject Self { get; }
    Vector3 SelfPosition { get; }
}
```

### 2. 継承設計の問題

#### 問題の詳細

- **継承による拡張**: `TwinGoblinAI : BasicEnemyAI`, `TwinGoblinUserData : EnemyUserData`
- **状態管理の複雑化**: 継承による状態の重複管理
- **拡張性の制限**: 複数の特殊エネミーに対応困難

#### 影響範囲

```csharp
// TwinGoblinAI.cs
public sealed class TwinGoblinAI : BasicEnemyAI
{
    private bool _enraged;

    void OnMateDied(TwinMateDeedEvent e)
    {
        if (_enraged || e.PairId != _pairId || e.Id == _ctx.EnemyId) return;
        _enraged = true;
        SetSpeedMultiplier(_enrageSpeedMul());
        SetAttackMultiplier(_enrageAtkMul());
    }
}
```

#### リファクタリング提案

```csharp
// コンポジション優先設計
public class TwinGoblinAI : IEnemyAi
{
    private readonly BasicEnemyAI _baseAI;
    private readonly IPairBehavior _pairBehavior;
    private readonly IEnrageBehavior _enrageBehavior;

    public void Tick(float dt)
    {
        _baseAI.Tick(dt);
        _pairBehavior.Update(dt);
        _enrageBehavior.Update(dt);
    }
}
```

### 3. 責務の混在

#### 問題の詳細

- **ファクトリの責務過多**: オブジェクト生成・設定・組み立てを 1 つのメソッドで実行
- **PresenterCore の複雑性**: 初期化処理で複数の責務を実行
- **移動ロジックの混在**: 移動と回転の処理が混在

#### 影響範囲

```csharp
// EnemyFactory.cs
private static IEnemyUnit BuildBasic(EnemyUserData d, DomainEventDispatcher disp, IAIContext ctx, IMoveLogic move, PairLink? pair)
{
    var combat = new SimpleCombatLogic(() => d.AttackPower, () => d.CooldownSeconds);
    var cfg = new BasicEnemyAI.Config(...);
    var ai = new BasicEnemyAI(ctx, move, combat, disp, cfg);
    var core = new Enemy(ctx.EnemyId, d.MaxHp, 0.3f, disp);
    return new EnemyDomainService(core, move, combat, ai, disp);
}

// NavigationMoveLogic.cs
public void Tick(float dt)
{
    // 移動計算
    var frameMove = _agent.FrameMove;
    float speedBase = _speed() * _speedMul;

    // 回転計算
    Quaternion currentRot = tf.Rotation;
    Quaternion arcRot = quaternion.makeRotationArc(currentFwd, targetDir);

    // 位置更新
    var delta = frameMove * speed * dt;
    tf.Position += delta;

    // 回転更新
    tf.Rotation = quaternion.slerp(currentRot, desiredRot, easeT);
}
```

#### リファクタリング提案

```csharp
// ビルダーパターン
public class EnemyBuilder
{
    private EnemyUserData _data;
    private DomainEventDispatcher _dispatcher;
    private IAIContext _context;
    private IMoveNavigator _move;

    public EnemyBuilder WithData(EnemyUserData data) { _data = data; return this; }
    public EnemyBuilder WithDispatcher(DomainEventDispatcher dispatcher) { _dispatcher = dispatcher; return this; }

    public IEnemyUnit Build()
    {
        var combat = CreateCombatLogic();
        var ai = CreateAI(combat);
        var core = CreateCore();
        return new EnemyDomainService(core, _move, combat, ai, _dispatcher);
    }
}

// 責務の分離
public class MovementController
{
    private readonly IPositionController _positionController;
    private readonly IRotationController _rotationController;

    public void Update(float dt)
    {
        var movement = _positionController.CalculateMovement(dt);
        var rotation = _rotationController.CalculateRotation(dt);

        _positionController.ApplyMovement(movement);
        _rotationController.ApplyRotation(rotation);
    }
}
```

### 4. 型安全性の問題

#### 問題の詳細

- **実行時キャスト**: `var td = (TwinGoblinUserData)d;`
- **null 参照の可能性**: `objs[i] = PatrolPoints[i].Target;`
- **型安全でない拡張**: `T? GetExtra<T>() where T : class;`

#### 影響範囲

```csharp
// EnemyFactory.cs
var td = (TwinGoblinUserData)d; // キャストが必要

// EnemyUserData.cs
private PatrolSettings GetPatrolSettings()
{
    GameObject[] objs = new GameObject[PatrolPoints.Length];
    for (int i = 0; i < PatrolPoints.Length; ++i)
    {
        objs[i] = PatrolPoints[i].Target; // null参照の可能性
    }
    return new PatrolSettings(objs, PatrolWaitSeconds, ReturnHomeDistance);
}

// IAiContext.cs
T? GetExtra<T>() where T : class; // 型安全でない拡張メカニズム
```

#### リファクタリング提案

```csharp
// ジェネリックファクトリ
public interface IEnemyFactory<TData> where TData : EnemyUserData
{
    IEnemyUnit Create(TData data, DomainEventDispatcher dispatcher, IAIContext ctx, IMoveNavigator move, PairLink? pair);
}

// null安全性の向上
public class PatrolSettings
{
    public IReadOnlyList<IPatrolPoint> Points { get; }

    public PatrolSettings(IEnumerable<IPatrolPoint> points, float waitSeconds, float returnDistance)
    {
        Points = points?.ToList() ?? new List<IPatrolPoint>();
        WaitSeconds = Math.Max(0, waitSeconds);
        ReturnHomeDistance = Math.Max(0, returnDistance);
    }
}
```

### 5. 未実装部分

#### 問題の詳細

- **View 実装の不完全**: `SimpleEnemyView`の描画処理が未実装
- **TODO コメント**: 複数箇所に未実装部分が存在
- **コメントアウトされたコード**: 実装途中のコードが残存

#### 影響範囲

```csharp
// SimpleEnemyView.cs
public void Render(in EnemyViewModel vm)
{
    // TODO: なんか描画処理？
    // 例: RageFx.SetActive();
}

// TwinGoblin.cs
protected override void PublishDeathEvent()
{
    //_dispatcher.Dispatch(new TwinMateDeedEvent(Id, _pairId));
    base.PublishDeathEvent();
}

// TwinGoblinAI.cs
// TODO: 少女を取得し _enragedTarget に格納
// TODO: ターゲットの変更
```

## リファクタリング優先順位

### Phase 1: エンジン依存の分離（最優先）

1. **抽象化インターフェースの作成**
   - `IGameObject`, `ITransform`, `IPhysicsSystem`, `INavigationSystem`
2. **モック実装の提供**
   - コンソールアプリケーション用のモック
   - テスト用のモック
3. **適応層の実装**
   - via エンジン型と抽象化インターフェースの変換

### Phase 2: コンポジション優先設計への移行

1. **継承の置き換え**
   - `TwinGoblinAI`のコンポジション化
   - `TwinGoblinUserData`のコンポジション化
2. **ビヘイビアパターンの導入**
   - `IPairBehavior`, `IEnrageBehavior`等の実装

### Phase 3: 責務の明確な分離

1. **ビルダーパターンの導入**
   - `EnemyBuilder`による複雑なオブジェクト生成の簡素化
2. **責務の分離**
   - 移動と回転の分離
   - 初期化処理の分離

### Phase 4: 型安全性の向上

1. **ジェネリックファクトリの導入**
2. **null 安全性の向上**
3. **型安全な拡張メカニズムの実装**

### Phase 5: 未実装部分の完成

1. **View 実装の完成**
2. **TODO 部分の実装**
3. **コメントアウトされたコードの整理**

## テスト戦略

### 単体テスト

- **Domain 層**: エンティティ、ドメインサービスのテスト
- **Application 層**: ファクトリ、プレゼンターのテスト
- **Infrastructure 層**: AI、移動、戦闘ロジックのテスト

### 統合テスト

- **エンドツーエンドテスト**: エネミー生成から動作まで
- **パフォーマンステスト**: 複数エネミーの同時動作
- **状態遷移テスト**: AI 状態の遷移パターン

### モック戦略

- **エンジン依存のモック**: `IGameObject`, `IPhysicsSystem`等
- **外部依存のモック**: `IWorld`, `INavigationSystem`等
- **イベントのモック**: `IDomainEvent`等

## パフォーマンス考慮事項

### 現在の最適化

- **キャッシュ機能**: AI コンテキストでの位置情報キャッシュ
- **更新頻度制御**: 重い処理の更新間隔制御
- **描画最適化**: ViewModel 変更検知

### 今後の最適化案

- **オブジェクトプール**: エネミーの再利用
- **空間分割**: 近傍エネミーの効率的な検索
- **LOD**: 距離に応じた AI 精度の調整

## 拡張性の考慮事項

### 新しいエネミータイプの追加

- **データ駆動**: 設定ファイルによる新エネミーの追加
- **プラグイン方式**: 動的ロードによる機能拡張
- **スクリプト機能**: カスタム AI ロジックの実装

### 新しい AI 機能の追加

- **ステートの追加**: 新しい AI 状態の実装
- **ビヘイビアの追加**: 新しい行動パターンの実装
- **条件の追加**: 新しい状態遷移条件の実装

## まとめ

現在のエネミー AI システムは、クリーンアーキテクチャ風の設計とステートパターンによる AI 実装など、多くの良い設計パターンを採用しています。しかし、via エンジンへの強依存や継承設計による拡張性の制限など、複数の問題点が存在します。

段階的なリファクタリングにより、エンジン非依存でテスト容易、拡張性の高いシステムに改善することが可能です。特に、エンジン依存の分離を最優先とし、その後コンポジション優先設計への移行を進めることを推奨します。
