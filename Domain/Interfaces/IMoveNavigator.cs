//======================================================================
// <summary>
//   IMoveNavigator
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
    /// ナビゲーション先を与えられる移動ロジック
    /// </summary>
    public interface IMoveNavigator : IMoveLogic
    {
        void SetTarget(GameObject target);
        void Stop();
        bool IsArrived { get; }
    }
}
