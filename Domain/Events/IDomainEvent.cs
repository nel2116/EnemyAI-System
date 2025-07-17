//======================================================================================================================
/// <summary>
/// IDomainEvent
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
    /// ドメイン層で発火するイベントの共通インターフェイス。<br/>
    /// 実装クラスは不変(immutable)にすることを推奨します。
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// イベント発生日時(utc)
        /// </summary>
        DateTime OccurredAtUtc { get; }
    }
}
