# Codex 実装ガイドライン

## 抽象化インターフェース設計

### 1. IGameObject インターフェース

```csharp
public interface IGameObject
{
    string Name { get; }
    ITransform Transform { get; }
    T? GetComponent<T>() where T : class;
    bool IsActive { get; }
}
```

### 2. ITransform インターフェース

```csharp
public interface ITransform
{
    Vector3 Position { get; set; }
    Quaternion Rotation { get; set; }
    Vector3 Forward { get; }
    Vector3 Right { get; }
    Vector3 Up { get; }
    Vector3 Scale { get; set; }
}
```

### 3. Vector3 構造体

```csharp
public readonly struct Vector3
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 Zero => new(0, 0, 0);
    public static Vector3 One => new(1, 1, 1);

    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator *(Vector3 a, float b) => new(a.X * b, a.Y * b, a.Z * b);
}
```

### 4. Quaternion 構造体

```csharp
public readonly struct Quaternion
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }
    public float W { get; }

    public Quaternion(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public static Quaternion Identity => new(0, 0, 0, 1);
}
```

## ビヘイビアコンポーネント設計

### 1. IEnemyBehavior インターフェース

```csharp
public interface IEnemyBehavior
{
    void Initialize(IEnemyUnit enemy);
    void Update(float deltaTime);
    void Dispose();
}
```

### 2. IPairBehavior インターフェース

```csharp
public interface IPairBehavior : IEnemyBehavior
{
    event Action<EnemyId> OnPairMemberDied;
    void SetPairId(EnemyId pairId);
    bool IsPaired { get; }
}
```

### 3. IEnrageBehavior インターフェース

```csharp
public interface IEnrageBehavior : IEnemyBehavior
{
    event Action OnEnrageTriggered;
    float SpeedMultiplier { get; }
    float AttackMultiplier { get; }
    bool IsEnraged { get; }
}
```

## アダプター層設計

### 1. ViaAdapter クラス

```csharp
public class ViaAdapter : IGameObject
{
    private readonly via.GameObject _viaObject;

    public ViaAdapter(via.GameObject viaObject)
    {
        _viaObject = viaObject;
    }

    public string Name => _viaObject.Name;
    public ITransform Transform => new ViaTransform(_viaObject.Transform);
    public bool IsActive => _viaObject.IsActive;

    public T? GetComponent<T>() where T : class
    {
        return _viaObject.getComponent<T>();
    }
}
```

### 2. ViaTransform クラス

```csharp
public class ViaTransform : ITransform
{
    private readonly via.Transform _viaTransform;

    public ViaTransform(via.Transform viaTransform)
    {
        _viaTransform = viaTransform;
    }

    public Vector3 Position
    {
        get => _viaTransform.Position.ToVector3();
        set => _viaTransform.Position = value.ToViaVec3();
    }

    public Quaternion Rotation
    {
        get => _viaTransform.Rotation.ToQuaternion();
        set => _viaTransform.Rotation = value.ToViaQuaternion();
    }

    public Vector3 Forward => _viaTransform.Forward.ToVector3();
    public Vector3 Right => _viaTransform.Right.ToVector3();
    public Vector3 Up => _viaTransform.Up.ToVector3();
    public Vector3 Scale
    {
        get => _viaTransform.Scale.ToVector3();
        set => _viaTransform.Scale = value.ToViaVec3();
    }
}
```

## 拡張メソッド

### 1. 型変換拡張メソッド

```csharp
public static class ViaExtensions
{
    public static Vector3 ToVector3(this via.vec3 viaVec)
    {
        return new Vector3(viaVec.x, viaVec.y, viaVec.z);
    }

    public static via.vec3 ToViaVec3(this Vector3 vector)
    {
        return new via.vec3(vector.X, vector.Y, vector.Z);
    }

    public static Quaternion ToQuaternion(this via.quaternion viaQuat)
    {
        return new Quaternion(viaQuat.x, viaQuat.y, viaQuat.z, viaQuat.w);
    }

    public static via.quaternion ToViaQuaternion(this Quaternion quaternion)
    {
        return new via.quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
    }

    public static IGameObject ToIGameObject(this via.GameObject viaObject)
    {
        return new ViaAdapter(viaObject);
    }
}
```

## ファクトリパターン設計

### 1. IEnemyBuilder インターフェース

```csharp
public interface IEnemyBuilder
{
    IEnemyBuilder WithData(EnemyUserData data);
    IEnemyBuilder WithDispatcher(DomainEventDispatcher dispatcher);
    IEnemyBuilder WithContext(IAIContext context);
    IEnemyBuilder WithNavigator(IMoveNavigator navigator);
    IEnemyBuilder WithPairLink(PairLink? pairLink);
    IEnemyUnit Build();
}
```

### 2. EnemyBuilder クラス

```csharp
public class EnemyBuilder : IEnemyBuilder
{
    private EnemyUserData? _data;
    private DomainEventDispatcher? _dispatcher;
    private IAIContext? _context;
    private IMoveNavigator? _navigator;
    private PairLink? _pairLink;

    public IEnemyBuilder WithData(EnemyUserData data)
    {
        _data = data;
        return this;
    }

    public IEnemyBuilder WithDispatcher(DomainEventDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        return this;
    }

    public IEnemyBuilder WithContext(IAIContext context)
    {
        _context = context;
        return this;
    }

    public IEnemyBuilder WithNavigator(IMoveNavigator navigator)
    {
        _navigator = navigator;
        return this;
    }

    public IEnemyBuilder WithPairLink(PairLink? pairLink)
    {
        _pairLink = pairLink;
        return this;
    }

    public IEnemyUnit Build()
    {
        if (_data == null || _dispatcher == null || _context == null || _navigator == null)
            throw new InvalidOperationException("Required dependencies not set");

        return _data.Template switch
        {
            EnemyAITemplate.Basic => BuildBasicEnemy(),
            EnemyAITemplate.Twin => BuildTwinGoblin(),
            _ => BuildBasicEnemy()
        };
    }

    private IEnemyUnit BuildBasicEnemy()
    {
        // 基本エネミーの構築ロジック
    }

    private IEnemyUnit BuildTwinGoblin()
    {
        // TwinGoblinの構築ロジック
    }
}
```

## テスト用モック実装

### 1. MockGameObject クラス

```csharp
public class MockGameObject : IGameObject
{
    public string Name { get; set; } = "MockObject";
    public ITransform Transform { get; set; } = new MockTransform();
    public bool IsActive { get; set; } = true;

    public T? GetComponent<T>() where T : class
    {
        return null;
    }
}
```

### 2. MockTransform クラス

```csharp
public class MockTransform : ITransform
{
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Vector3 Forward { get; set; } = new Vector3(0, 0, 1);
    public Vector3 Right { get; set; } = new Vector3(1, 0, 0);
    public Vector3 Up { get; set; } = new Vector3(0, 1, 0);
    public Vector3 Scale { get; set; } = Vector3.One;
}
```

## 実装時の注意点

### 1. パフォーマンス考慮

- 型変換のオーバーヘッドを最小化
- キャッシュを活用して重複計算を避ける
- 不要なオブジェクト生成を避ける

### 2. 型安全性

- キャストを避け、型安全な設計を採用
- コンパイル時にエラーを検出できる設計
- null 参照を避ける適切な null チェック

### 3. 可読性向上

- 明確な命名規則
- 適切なコメント
- 単一責任の原則に従った設計

### 4. テスト容易性

- 依存性注入を活用
- モック可能なインターフェース設計
- 純粋関数の活用
