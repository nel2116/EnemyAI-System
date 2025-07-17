//======================================================================
// <summary>
//   IAiContext
// </summary>
// <author>CGC_10_田中 ミノル</author>
//======================================================================
using System;
using System.Collections.Generic;
using app.enemy.core.interfaces;
using app.enemy.core.values;

namespace app.enemy.domain.interfaces
{
    /// <summary>
    /// AIが毎フレーム参照する「外部環境情報」を抽象化したインターフェイス
    /// </summary>
    public interface IAiContext
    {
    	EnemyId EnemyId { get; }
    	
        // --- 基本的な空間情報 ---

        /// <summary>
        /// 自分の参照
        /// </summary>
        IGameObject Self { get; }

        /// <summary>
        /// ターゲットの参照
        /// </summary>
        IGameObject Target { get; }

        /// <summary>
        /// 現在自分がいるワールド座標
        /// </summary>
        Vector3 SelfPosition { get; }

        /// <summary>
        /// ターゲットのワールド座標
        /// </summary>
        Vector3 TargetPosition { get; }

        /// <summary>
        /// ターゲットまでの直線距離
        /// </summary>
        float DistanceToTarget { get; }

        /// <summary>
        /// 敵からターゲットまでの視線がさえぎられていないか
        /// </summary>
        bool HasLineOfSight { get; }

        /// <summary>
        /// 高低差判定や複数ターゲット切り替えなど、
        /// AIが追加で参照したい場合に拡張メソッドで保管
        /// </summary>
        /// <typeparam name="T">コンテキスト型</typeparam>
        /// <returns>任意の追加情報</returns>
        T? GetExtra<T>() where T : class;

        void Tick(float dt);
    }
}
