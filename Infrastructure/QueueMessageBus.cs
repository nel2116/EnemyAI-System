/// <summary>
/// QueueMessageBus
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using app.enemy.app;
using app.enemy.domain.events;
using via;
using via.attribute;

namespace app.enemy.infrastructure.messaging
{
    public sealed class QueueMessageBus : IMessageBus
    {
        private readonly Queue<IDomainEvent> _queue = new();

        public void Publish(IDomainEvent ev)
        {
            if (ev is null) throw new ArgumentNullException(nameof(ev));
            // via.debug.infoLine($"[Bus] Enqueue {ev.GetType().Name}");
            _queue.Enqueue(ev);
        }

        public void Flush(float timeBudgetMs = 1f)
        {
            if (_queue.Count == 0) return;
            // via.debug.infoLine("[Event] Flush");

            var sw = Stopwatch.StartNew();
            while (_queue.Count > 0 && sw.ElapsedMilliseconds < timeBudgetMs)
            {
                var ev = _queue.Dequeue();
                try
                {
                    // via.debug.infoLine($"[Event] {ev.GetType().Name} @ {ev.OccurredAtUtc:O}");
                    // Analytics
                }
                catch (Exception ex)
                {
                    via.debug.errorLine($"{ex.Message} : MessageBus handler error");
                }
            }
        }
    }
}
