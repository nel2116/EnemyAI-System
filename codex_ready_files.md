# Codex リファクタリング準備完了ファイル

## 作成済みファイル一覧

### 1. コンテキストファイル

- **`codex_refactoring_context.md`**: プロジェクトの詳細な分析とリファクタリング目標
- **`codex_refactoring_prompts.md`**: 具体的なプロンプト例
- **`codex_implementation_guide.md`**: 実装ガイドラインと設計パターン
- **`codex_workflow.md`**: 段階的なリファクタリングワークフロー

### 2. プロジェクト分析ファイル

- **`ENEMY_AI_ANALYSIS.md`**: 現在のコードベースの詳細分析
- **`ENEMY_AI_REFACTORING_PLAN.md`**: 既存のリファクタリング計画

## Codex への送信準備

### Phase 1: エンジン依存の分離

**送信するファイル:**

1. `codex_refactoring_context.md` - プロジェクト概要
2. `Domain/Interfaces/IAiContext.cs` - 現在のインターフェース
3. `codex_implementation_guide.md` - 実装ガイドライン

**プロンプト例:**

```
以下のC#プロジェクトのDomain層からエンジン依存を分離するリファクタリングを提案してください。

プロジェクト概要:
[codex_refactoring_context.mdの内容]

現在の問題:
- Domain層のIAiContextインターフェースがviaエンジン固有の型（GameObject, vec3）に依存
- ローカル環境でのテストが困難

目標:
- Domain層からviaエンジン型を完全に排除
- 抽象化インターフェースの強化
- アダプター層の実装

現在のコード:
[IAiContext.csの内容]

実装ガイドライン:
[codex_implementation_guide.mdの抽象化インターフェース設計部分]

提案してください:
1. 新しい抽象化インターフェース（IGameObject, ITransform, Vector3等）
2. アダプター層の実装
3. 段階的移行計画
```

### Phase 2: コンポジション優先設計への移行

**送信するファイル:**

1. `codex_refactoring_context.md` - プロジェクト概要
2. `Infrastructure/AI/BasicEnemyAI.cs` - 基本 AI 実装
3. `Infrastructure/AI/TwinGoblinAI.cs` - 特殊 AI 実装
4. `codex_implementation_guide.md` - ビヘイビアコンポーネント設計

**プロンプト例:**

```
以下のC#プロジェクトの継承設計をコンポジション優先設計に変更するリファクタリングを提案してください。

プロジェクト概要:
[codex_refactoring_context.mdの内容]

現在の問題:
- TwinGoblinAIがBasicEnemyAIを継承している
- 複数の特殊エネミーに対応困難
- 状態管理の複雑化

目標:
- ビヘイビアコンポーネントの導入
- 継承からコンポジションへの変更
- 型安全性の向上

現在のコード:
[BasicEnemyAI.csとTwinGoblinAI.csの内容]

実装ガイドライン:
[codex_implementation_guide.mdのビヘイビアコンポーネント設計部分]

提案してください:
1. ビヘイビアコンポーネントの設計（IPairBehavior, IEnrageBehavior等）
2. コンポジション優先のTwinGoblinAI実装
3. 新エネミー追加時の拡張方法
```

### Phase 3: ファクトリの拡張性改善

**送信するファイル:**

1. `codex_refactoring_context.md` - プロジェクト概要
2. `Application/EnemyFactory.cs` - 現在のファクトリ
3. `Data/EnemyUserData.cs` - データクラス
4. `codex_implementation_guide.md` - ファクトリパターン設計

**プロンプト例:**

```
以下のC#プロジェクトのファクトリパターンを拡張性の高い設計に変更するリファクタリングを提案してください。

プロジェクト概要:
[codex_refactoring_context.mdの内容]

現在の問題:
- 新しいエネミータイプ追加時にEnumとビルダーを変更
- Open/Closed Principleに反している
- 型安全性が不十分（キャストが必要）

目標:
- ビルダーパターンの導入
- 設定駆動設計の強化
- 新エネミー追加の容易化

現在のコード:
[EnemyFactory.csとEnemyUserData.csの内容]

実装ガイドライン:
[codex_implementation_guide.mdのファクトリパターン設計部分]

提案してください:
1. ビルダーパターンの実装
2. 設定駆動のファクトリ設計
3. 新エネミー追加時の手順
```

## ファイル送信のベストプラクティス

### 1. 段階的な送信

- 一度に全てのファイルを送信せず、段階的に送信
- 各 Phase ごとに必要なファイルのみを送信
- 前の Phase の結果を次の Phase に反映

### 2. コンテキストの明確化

- プロジェクト概要を最初に送信
- 現在の問題点を具体的に説明
- 目標と制約を明確に示す

### 3. コードの整理

- 関連するファイルをグループ化
- 不要なコードは除外
- 重要な部分にコメントを追加

### 4. フィードバックの活用

- Codex の提案を検討
- 必要に応じて追加の質問
- 実装の詳細を要求

## 期待される成果

### Phase 1 完了後

- エンジン非依存の抽象化インターフェース
- アダプター層の実装
- 段階的移行計画

### Phase 2 完了後

- ビヘイビアコンポーネントの設計
- コンポジション優先の AI 実装
- 新エネミー追加の拡張方法

### Phase 3 完了後

- ビルダーパターンの実装
- 設定駆動のファクトリ設計
- 型安全な新エネミー追加手順

### 全体完了後

- エンジン非依存の Domain 層
- 新エネミー実装の容易化
- テスト容易性の向上
- 型安全性の向上

## 次のステップ

1. **Phase 1 の開始**: エンジン依存の分離から開始
2. **段階的な実装**: 各 Phase を順次実行
3. **テストの実行**: 各段階でテストを実行
4. **ドキュメント更新**: リファクタリング結果を反映

これで、Codex にリファクタリングを依頼する準備が整いました。各 Phase ごとに適切なファイルとプロンプトを使用して、効果的なリファクタリングを進めることができます。
