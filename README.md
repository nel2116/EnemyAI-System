# エネミー AI 開発環境

独自エンジン（via）に依存していたエネミー AI コードを、ローカル環境で開発・テストできるようにしたプロジェクトです。

## 概要

このプロジェクトは、以下の特徴を持つエネミー AI 開発環境を提供します：

- **エンジン非依存**: via エンジンに依存せずに開発可能
- **コンソールアプリケーション**: 高速なテストとデバッグ
- **モック実装**: 物理演算、ナビゲーション、GameObject のモック
- **段階的移行**: 既存コードを最小限の変更で適応可能

## プロジェクト構造

```
LocalCode/
├── Core/
│   ├── Interfaces/          # 抽象化インターフェース
│   ├── Mocks/              # コンソール用モック実装
│   └── Adapters/           # viaエンジン適応層
├── Application/            # 既存のアプリケーション層
├── Domain/                 # 既存のドメイン層
├── Infrastructure/         # 既存のインフラ層
├── Data/                   # 既存のデータ層
├── Presentation/           # 既存のプレゼンテーション層
├── Via/                    # 既存のviaエンジン依存コード
├── LocalCode.Tests/        # テストプロジェクト
├── Program.cs              # メインプログラム
└── LocalCode.csproj        # プロジェクト設定
```

## セットアップ

### 前提条件

- .NET 8.0 SDK
- Visual Studio 2022 または VS Code

### ビルドと実行

1. **プロジェクトの復元**

   ```bash
   dotnet restore
   ```

2. **ビルド**

   ```bash
   dotnet build
   ```

3. **実行**

   ```bash
   dotnet run
   ```

4. **テスト実行**
   ```bash
   dotnet test
   ```

## 使用方法

### 基本的なエネミー AI のテスト

```csharp
// エネミーとプレイヤーの作成
var enemy = new ConsoleGameObject("Enemy", new Vector3(0, 0, 0));
var player = new ConsoleGameObject("Player", new Vector3(10, 0, 10));

// 物理システムのテスト
var physicsSystem = new ConsolePhysicsSystem();
var hit = physicsSystem.Raycast(enemy.Transform.Position, player.Transform.Position, "Stage");

// ナビゲーションシステムのテスト
var navSystem = new ConsoleNavigationSystem(enemy.Transform.Position);
navSystem.SetDestination(player.Transform.Position);
var movement = navSystem.GetMovementVector();
```

### 既存コードとの統合

既存の via エンジン依存コードを新しいインターフェースに適応するには：

```csharp
// via.GameObjectをIGameObjectに変換
var viaObject = new via.GameObject();
var iGameObject = viaObject.ToIGameObject();

// via.vec3をVector3に変換
var viaVec = new via.vec3(1, 2, 3);
var vector = viaVec.ToVector3();
```

## 主要コンポーネント

### 抽象化インターフェース

- **IGameObject**: ゲームオブジェクトの抽象化
- **IPhysicsSystem**: 物理演算システムの抽象化
- **INavigationSystem**: ナビゲーションシステムの抽象化

### モック実装

- **ConsoleGameObject**: コンソール用 GameObject モック
- **ConsolePhysicsSystem**: コンソール用物理演算モック
- **ConsoleNavigationSystem**: コンソール用ナビゲーションモック

### 適応層

- **ViaAdapter**: via エンジン型を新しいインターフェースに変換

## 開発フロー

1. **ローカル環境での開発**

   - コンソールアプリケーションで AI ロジックを開発
   - モック実装を使用してテスト

2. **via エンジンでの統合**

   - 適応層を使用して via エンジンと統合
   - 実際のゲーム環境でテスト

3. **継続的な改善**
   - 両環境でテストを実行
   - パフォーマンスと機能を改善

## テスト

プロジェクトには以下のテストが含まれています：

- **基本機能テスト**: モック実装の動作確認
- **AI 状態遷移テスト**: 距離に基づく状態遷移の検証
- **物理演算テスト**: レイキャスト機能の検証
- **ナビゲーションテスト**: 移動システムの検証

テストを実行するには：

```bash
dotnet test --verbosity normal
```

## 今後の拡張予定

- [ ] Unity 環境での動作確認
- [ ] より高度な AI 機能の実装
- [ ] パフォーマンス最適化
- [ ] ビジュアルデバッグ機能
- [ ] 複数エネミー間の連携機能

## トラブルシューティング

### よくある問題

1. **ビルドエラー**

   - .NET 8.0 SDK がインストールされているか確認
   - `dotnet restore`を実行して依存関係を復元

2. **テストエラー**

   - テストプロジェクトが正しく参照されているか確認
   - モック実装が正しく動作しているか確認

3. **via エンジン統合エラー**
   - 適応層が正しく実装されているか確認
   - 型変換が正しく行われているか確認

## ライセンス

このプロジェクトは開発・学習目的で作成されています。
