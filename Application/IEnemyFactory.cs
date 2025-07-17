//====================================================================
// <summary>
// IEnemyFactory
// </summary>
// <author>CGC_10_田中 ミノル</author>
//====================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;
using app.enemy.app.dto;
using app.enemy.domain;
using app.enemy.domain.events;
using app.enemy.domain.interfaces;

namespace app.enemy.app
{
    /// <summary>
    /// PresenterCore から呼ばれる Enemy 生成ファクトリ
    /// </summary>
    public interface IEnemyFactory
    {
        IEnemyUnit Create(
            EnemyUserData userData,
            DomainEventDispatcher dispatcher,
            IAIContext ctx,
            IMoveNavigator move,
            PairLink? pair
        );
    }
}
