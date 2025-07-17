//======================================================================
// <summary>
//   IEnemyAi
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
    /// 敵AIステートマシンの共通I/F
    /// </summary>
    public interface IEnemyAi
    {
        void Tick(float dt);
    }
}
