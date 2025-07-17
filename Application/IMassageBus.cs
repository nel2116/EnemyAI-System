//====================================================================
// <summary>
// IMessageBus
// </summary>
// <author>CGC_10_田中 ミノル</author>
//====================================================================
using System;
using System.Collections.Generic;
using app.enemy.domain.events;
using via;
using via.attribute;

namespace app.enemy.app
{
    /// <summary>
    /// Presenter -> 外界(Analytics/ログ etc.)へイベントを流すハブ
    /// 実装は後続フェーズ
    /// </summary>
    public interface IMessageBus
    {
        void Publish(IDomainEvent ev);
        void Flush(float timeBudgetMs = 1f); // 1フレームで処理する最大時間（ms）
    }
}
