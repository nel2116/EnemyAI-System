//======================================================================
// <summary>
//   IEnemyUnit
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
    /// 1体の敵を更新・操作する最小インターフェース
    /// </summary>
    public interface IEnemyUnit : IDisposable
    {
        EnemyStatus Status { get; }
        float CurrentSpeed { get; }

        void Tick(float dt);
        void ApplyDamage(int amount);
    }
}
