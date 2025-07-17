//======================================================================
// <summary>
//   EnemyInterfaces
// </summary>
// <author>CGC_10_田中 ミノル</author>
//======================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app.enemy.domain.interfaces
{
    /// <summary>
    /// フレームごとに移動ベクトル／速度を計算するロジック
    /// </summary>
    public interface IMoveLogic
    {
        /// <summary>
        /// 呼び出し感覚 ≒ フレーム時間
        /// </summary>
        /// <param name="dt"></param>
        void Tick(float dt);

        /// <summary>
        /// 現在速度(m/s) デバッグ/アニメ制御用
        /// </summary>
        float CurrentSpeed { get; }
    }

    /// <summary>
    /// 攻撃クールダウンなど戦闘関連のタイミング管理
    /// </summary>
    public interface ICombatLogic
    {
        /// <summary>
        /// フレーム更新
        /// </summary>
        void Tick(float dt);

        /// <summary>
        /// 攻撃可能状態かどうか
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// 攻撃を発動しクールダウンを開始。
        /// </summary>
        void Use();

        /// <summary>
        /// 次に使用できるまでの残り時間(秒)
        /// </summary>
        float CooldownRemaining { get; }

        event Action? OnAttack;
    }
}
