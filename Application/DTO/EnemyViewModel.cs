//============================================================
/// <summary>
/// EnemyViewModel
/// </summary>
/// <author>CGC_10_田中 ミノル</author>
//============================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app.enemy.app.dto
{
    /// <summary>
    /// Viewに渡す描画専用データ
    /// </summary>
    public readonly record struct EnemyViewModel(float HpRatio, bool IsEnraged, float Speed);
}
