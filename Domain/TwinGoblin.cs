/// <summary>
/// TwinGoblin
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

using System;
using System.Collections.Generic;
using app.enemy.domain.events;
using app.enemy.domain.interfaces;
using via;
using via.attribute;

namespace app.enemy.domain
{
	public class TwinGoblin : Enemy
	{
    	private readonly EnemyId _pairId;

    	public TwinGoblin(EnemyId id, int maxHp, float enrageThresholdRatio, DomainEventDispatcher dispatcher, Guid pairId)
        	: base(id, maxHp, enrageThresholdRatio, dispatcher)
	    {
    	    _pairId = new EnemyId(pairId);
	    }

    	protected override void PublishDeathEvent()
	    {
    	    _dispatcher.Dispatch(new TwinMateDiedEvent(Id, _pairId));
	    }
	}
}