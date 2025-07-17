/// <summary>
/// SimpleEnemyView
/// </summary>
/// <author>CGC_10_田中 ミノル</author>
using System;
using System.Collections.Generic;
using app.enemy.app.dto;
using app.enemy.presentation;
using via;
using via.attribute;

namespace app.enemy.vias
{
    /// <summary>
    /// 簡易実装
    /// </summary>
    public sealed class SimpleEnemyView : via.Behavior, IEnemyView
    {
        // TODO: エンジン依存メンバ
        // Animator Animator;
        // HpBarShim HpBar;
        // GameObject RageFx;

        public void Render(in EnemyViewModel vm)
        {
            // TODO: なんか描画処理？
            // 例: RageFx.SetActive();
        }
    }
}
