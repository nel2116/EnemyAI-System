//====================================================================================================================
/// <summary>
/// IEnemyView
/// </summary>
/// <author>CGC_10_田中 ミノル</author>
//====================================================================================================================
using System;
using System.Collections.Generic;
using app.enemy.app.dto;
using via;
using via.attribute;

namespace app.enemy.presentation
{
    /// <summary>
    /// Presenter から描画情報を受け取って UI を更新。
    /// </summary>
    public interface IEnemyView
    {
        void Render(in EnemyViewModel vm);
    }
}
