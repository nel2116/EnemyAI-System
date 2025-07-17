//======================================================================================================================
/// <summary>
/// AiState
/// </summary>
/// <author>CGC_10_田中 ミノル</author>
//======================================================================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app.enemy.domain.enums
{
    /// <summary>
    /// 敵がとりえる状態
    /// </summary>
    public enum AiState
    {
        Idle,       // 立ち止まり
        Patrol,     // 巡回
        Chase,      // 追跡
        Attack,     // 攻撃発動
        Cooldown,   // 攻撃後クールダウン
        Return,     // ターゲット見失い・ホームへ帰投
        Dead,       // HP0（移動・攻撃停止）
    }
}
