#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using via;
using via.attribute;
using via.dev.command;

namespace app.enemy.domain.events
{
    /// <summary>
    /// ドメインイベントをハンドラに配信するディスパッチャ
    /// 登録時に IDisposable トークンを返し、usingで自動解除
    /// ハンドラ単位でロックを分割し、多スレッド競合を抑制
    /// </summary>
    public sealed class DomainEventDispatcher : IDisposable
    {
        #region Disposable Token
        private sealed class AnonymousDisposable : IDisposable
        {
            private Action? _dispose;
            private readonly object _gate = new();

            public AnonymousDisposable(Action dispose) => _dispose = dispose;

            public void Dispose()
            {
                Action? d;
                lock (_gate)
                {
                    d = _dispose;
                    _dispose = null;
                }
                d?.Invoke();
            }
        }

        private static IDisposable CreateToken(Action dispose) => new AnonymousDisposable(dispose);
        #endregion
		#region Handler Wrapper
		/// <summary>
		/// イベント型ごとに保持するハンドラ集合。リスト単位でロックする
		/// </summary>
		private sealed class HandlerList
		{
		    private readonly object _gate = new();
		    private readonly List<Delegate> _handlers = new();
	
		    public bool IsEmpty
		    {
		        get { lock (_gate) return _handlers.Count == 0; }
		    }
		
		    public IDisposable Add(Delegate del)
		    {
		        if (del == null) throw new ArgumentNullException(nameof(del));
		        lock (_gate) _handlers.Add(del);
		        return CreateToken(() => Remove(del));
		    }
		
		    public void Remove(Delegate del)
		    {
		        lock (_gate) _handlers.Remove(del);
		    }
		
		    public Delegate[] Snapshot()
		    {
		        lock (_gate) return _handlers.Count == 0 ? Array.Empty<Delegate>() : _handlers.ToArray();
		    }
		}
		#endregion
		
		private readonly ConcurrentDictionary<Type, HandlerList> _map = new();
		
		#region Register / Unregister
		/// <summary>
		/// 特定イベント型用ハンドラを登録し、解除用 IDisposable を返す
		/// </summary>
		public IDisposable Register<T>(Action<T> handler) where T : IDomainEvent
		{
		    var list = _map.GetOrAdd(typeof(T), _ => new HandlerList());
		    return list.Add(handler);
		}
		
		/// <summary>
		/// 全イベント共通ハンドラ
		/// </summary>
		public IDisposable Register(Action<IDomainEvent> handler) => Register<IDomainEvent>(handler);
		
		public void Unregister<T>(Action<T> handler) where T : IDomainEvent
		{
		    if (handler == null) return;
		    if (!_map.TryGetValue(typeof(T), out var list)) return;
		
		    list.Remove(handler);
		    if (list.IsEmpty)
		        _map.TryRemove(typeof(T), out _);
		}
		
		public void Unregister(Action<IDomainEvent> handler) => Unregister<IDomainEvent>(handler);
		#endregion
		
		#region Dispatch
		public void Dispatch<T>(T ev) where T : IDomainEvent
		{
		    if (ev is null) throw new ArgumentNullException(nameof(ev));
#if VIA_DEVELOP
		    via.debug.infoLine($"[Dispatcher] Dispatch {ev.GetType().Name}");
#endif
		
		    Deliver(ev, typeof(T));
		    
		    if (typeof(T) != typeof(IDomainEvent))
		        Deliver(ev, typeof(IDomainEvent));
		}
		
		/// <summary>
		/// 型に応じるリストをとりだし、スナップショットを呼び出す
		/// </summary>
		private void Deliver<T>(T ev, Type key) where T : IDomainEvent
		{
		    if (!_map.TryGetValue(key, out var list)) return;
		
		    foreach (var del in list.Snapshot())
		    {
		        try
		        {
		            switch (del)
		            {
		                case Action<T> typed:
		                    typed(ev);
		                    break;
		                    
		                case Action<IDomainEvent> generic:
		                    generic(ev);
		                    break;

		                default:
		                    del.DynamicInvoke(ev);
		                    break;
		            }
		        }
		        catch (Exception ex)
		        {
#if VIA_DEVELOP
		            via.debug.errorLine($"[Dispatcher] handler threw {ex.Message} @ {del.Method}");
#endif
		        }
		    }
		}
		#endregion
		
		#region IDisposable
		public void Clear() => _map.Clear();
		public void Dispose() => Clear();
		#endregion
	}
}