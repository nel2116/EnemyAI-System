//======================================================================================================================
/// <summary>
/// DomainEventBase
/// </summary>
/// <author>CGC_10_田中 ミノル</author>
//======================================================================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app.enemy.domain.events
{
    /// <summary>
    /// <see cref="IDomainEvent"/>の基底クラス。
    /// 継承して必要なプロパティを追加してください。
    /// </summary>
    public abstract record class DomainEventBase : IDomainEvent
    {
        public DateTime OccurredAtUtc
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}
