# Codex Agent: Enemy AI System Refactoring

## プロジェクト概要

このプロジェクトは、独自エンジン（via）に依存したエネミー AI システムを、クリーンアーキテクチャとコンポジション優先設計に基づいてリファクタリングするものです。主な目標は、新エネミーの実装を容易にし、エンジン依存を分離し、テスト容易性を向上させることです。

### プロジェクト構造

```
LocalCode/
├── Application/          # ユースケース層
├── Domain/              # ドメイン層（ビジネスロジック）
├── Infrastructure/      # インフラ層（AI実装、ナビゲーション）
├── Data/               # データ層（UserData）
├── Presentation/       # プレゼンテーション層
├── Via/               # エンジン接着層
└── Util/              # ユーティリティ
```

## 主要な課題とリファクタリング目標

### 1. エンジン依存の分離

- **問題**: Domain 層が via エンジン固有の型（GameObject, vec3）に依存
- **目標**: 抽象化インターフェースの導入によるエンジン非依存化
- **効果**: ローカル環境でのテスト・開発が可能

### 2. 継承設計からコンポジション設計への移行

- **問題**: TwinGoblinAI が BasicEnemyAI を継承し、拡張性が制限
- **目標**: ビヘイビアコンポーネントによるコンポジション優先設計
- **効果**: 新エネミーの追加が容易

### 3. ファクトリの拡張性改善

- **問題**: 新エネミー追加時に Enum とファクトリの変更が必要
- **目標**: ビルダーパターンと設定駆動設計の導入
- **効果**: Open/Closed Principle の遵守

### 4. テスト戦略の実装

- **問題**: エンジン依存により単体テストが困難
- **目標**: モック実装による包括的なテスト戦略
- **効果**: 高いテストカバレッジと品質保証

## リファクタリングフェーズ

### Phase 1: エンジン依存の分離（最優先）

1. **抽象化インターフェースの設計**
   - IGameObject, ITransform, Vector3, Quaternion
   - IPhysicsSystem, INavigationSystem
2. **アダプター層の実装**
   - ViaAdapter, ViaTransform
   - 型変換拡張メソッド
3. **Domain 層の更新**
   - IAiContext のエンジン非依存化

### Phase 2: コンポジション優先設計への移行

1. **ビヘイビアコンポーネントの設計**
   - IPairBehavior, IEnrageBehavior
   - IEnemyBehavior 基本インターフェース
2. **TwinGoblinAI のリファクタリング**
   - 継承の削除
   - コンポーネントの組み合わせ

### Phase 3: ファクトリの拡張性改善

1. **ビルダーパターンの導入**
   - 設定駆動設計の強化
2. **型安全性の向上**
   - キャストの排除

### Phase 4: テスト戦略の実装

1. **テストアーキテクチャの設計**
2. **モック実装の作成**

## 設計原則とガイドライン

### 1. クリーンアーキテクチャの遵守

- **依存関係の方向**: Domain → Application → Infrastructure
- **インターフェース分離**: 各層の責務を明確に分離
- **依存性注入**: コンストラクタインジェクションの活用

### 2. SOLID 原則の適用

- **Single Responsibility**: 各クラスは単一の責務
- **Open/Closed**: 拡張に開き、修正に閉じる
- **Liskov Substitution**: 継承よりもコンポジション
- **Interface Segregation**: 小さく、特化したインターフェース
- **Dependency Inversion**: 抽象に依存、具象に依存しない

### 3. パフォーマンス考慮事項

- **キャッシュ戦略**: 重い処理の結果をキャッシュ
- **更新頻度制御**: 不要な処理の削減
- **メモリ効率**: オブジェクトプールの活用

## 実装ガイドライン

### 1. 抽象化インターフェース設計

```csharp
// 基本インターフェース
public interface IGameObject
{
    string Name { get; }
    ITransform Transform { get; }
    T? GetComponent<T>() where T : class;
    bool IsActive { get; }
}

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

### 2. ビヘイビアコンポーネント設計

```csharp
public interface IEnemyBehavior
{
    void Initialize(IEnemyUnit enemy);
    void Update(float deltaTime);
    void Dispose();
}

public interface IPairBehavior : IEnemyBehavior
{
    event Action<EnemyId> OnPairMemberDied;
    void SetPairId(EnemyId pairId);
    bool IsPaired { get; }
}
```

### 3. アダプター層設計

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
    // ...
}
```

## テスト戦略

### 1. 単体テスト

- **Domain 層**: ビジネスロジックのテスト
- **Application 層**: ユースケースのテスト
- **Infrastructure 層**: AI 実装のテスト

### 2. 統合テスト

- **エンドツーエンド**: 完全なエネミー AI フロー
- **コンポーネント間**: 層間の連携テスト

### 3. モック戦略

- **エンジン依存**: IGameObject, IPhysicsSystem のモック
- **外部依存**: ナビゲーション、戦闘システムのモック

## 開発ワークフロー

### 1. 新機能開発

1. **要件分析**: 新エネミーの要件を明確化
2. **インターフェース設計**: 必要な抽象化インターフェースを定義
3. **実装**: コンポーネントベースで実装
4. **テスト**: 単体・統合テストの作成
5. **統合**: 既存システムとの統合

### 2. リファクタリング

1. **影響範囲分析**: 変更による影響を分析
2. **段階的移行**: 小さな変更を段階的に適用
3. **テスト実行**: 各段階でテストを実行
4. **検証**: 機能の動作確認

### 3. 品質保証

1. **コードレビュー**: 設計原則の遵守確認
2. **パフォーマンステスト**: 性能要件の確認
3. **統合テスト**: 全体の動作確認

## 関連ドキュメント

### プロジェクト概要

- [README.md](./README.md) - プロジェクトの基本情報とセットアップ
- [ENEMY_AI_ANALYSIS.md](./ENEMY_AI_ANALYSIS.md) - 現状の詳細分析

### リファクタリング計画

- [ENEMY_AI_REFACTORING_PLAN.md](./ENEMY_AI_REFACTORING_PLAN.md) - 包括的なリファクタリング計画
- [codex_refactoring_context.md](./codex_refactoring_context.md) - リファクタリングのコンテキスト

### 実装ガイド

- [codex_implementation_guide.md](./codex_implementation_guide.md) - 実装の詳細ガイドライン
- [codex_workflow.md](./codex_workflow.md) - 開発ワークフロー

### プロンプト例

- [codex_refactoring_prompts.md](./codex_refactoring_prompts.md) - Codex への依頼例
- [codex_ready_files.md](./codex_ready_files.md) - 準備済みファイル一覧

## 技術スタック

### 開発環境

- **言語**: C# (.NET 8.0)
- **エンジン**: via（独自エンジン）
- **アーキテクチャ**: クリーンアーキテクチャ
- **テスト**: xUnit

### 主要パターン

- **Repository Pattern**: データアクセス
- **Factory Pattern**: オブジェクト生成
- **Observer Pattern**: イベント処理
- **State Pattern**: AI 状態管理
- **Strategy Pattern**: ビヘイビア切り替え

## 品質基準

### 1. コード品質

- **可読性**: 明確な命名とコメント
- **保守性**: モジュール化と低結合
- **拡張性**: 新機能追加の容易さ
- **テスト容易性**: 単体テストの書きやすさ

### 2. パフォーマンス

- **応答性**: AI 更新の高速化
- **メモリ効率**: 不要なオブジェクト生成の削減
- **スケーラビリティ**: 多数のエネミー対応

### 3. 安定性

- **エラー処理**: 適切な例外処理
- **リカバリー**: 失敗時の復旧機能
- **ログ出力**: デバッグ情報の記録

## 今後の拡張予定

### 短期目標

- [ ] エンジン依存の完全分離
- [ ] コンポジション設計への移行
- [ ] 包括的なテスト戦略の実装

### 中期目標

- [ ] Unity 環境での動作確認
- [ ] より高度な AI 機能の実装
- [ ] パフォーマンス最適化

### 長期目標

- [ ] ビジュアルデバッグ機能
- [ ] 複数エネミー間の連携機能
- [ ] AI 学習機能の導入

---

このドキュメントは、Enemy AI System のリファクタリングプロジェクトにおける Codex の活用指針として作成されました。各フェーズでの具体的な実装方法や、設計原則の詳細については、関連ドキュメントを参照してください。
