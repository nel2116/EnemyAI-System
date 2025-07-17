//======================================================================
// <summary>
//   IMoveNavigator
// </summary>
// <author>CGC_10_田中 ミノル</author>
//======================================================================
using System;
using System.Collections.Generic;
using app.enemy.core.interfaces;

namespace app.enemy.domain.interfaces
{
    /// <summary>
    /// ナビゲーション先を与えられる移動ロジック
    /// </summary>
    public interface IMoveNavigator : IMoveLogic
    {
        void SetTarget(IGameObject target);
        void Stop();
        bool IsArrived { get; }
    }
}
