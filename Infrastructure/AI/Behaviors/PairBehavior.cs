using System;
using System.Threading;
using app.enemy.domain;
using app.enemy.domain.events;
using app.enemy.domain.interfaces;

namespace app.enemy.ai.behaviors
{
    public interface IEnemyBehavior
    {
        bool IsInitialized { get; }
        void Initialize(IEnemyUnit enemy);
        void Update(float deltaTime);
        void Dispose();
    }

    public interface IPairBehavior : IEnemyBehavior
    {
        event Action<EnemyId> OnPairMemberDied;
    }

    public sealed class PairBehavior : IPairBehavior
    {
        private readonly DomainEventDispatcher _dispatcher;
        private readonly EnemyId _pairId;
        private readonly object _lock = new();
        private IDisposable? _token;
        private bool _initialized;
        private bool _disposed;
        private IEnemyUnit? _enemy;

        public event Action<EnemyId>? OnPairMemberDied;

        public bool IsInitialized
        {
            get
            {
                lock (_lock)
                {
                    return _initialized;
                }
            }
        }

        public PairBehavior(DomainEventDispatcher dispatcher, EnemyId pairId)
        {
            _dispatcher = dispatcher;
            _pairId = pairId;
        }

        public void Initialize(IEnemyUnit enemy)
        {
            ArgumentNullException.ThrowIfNull(enemy);
            lock (_lock)
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(PairBehavior));

                if (_initialized) return;

                _enemy = enemy;
                _token = _dispatcher.Register<TwinMateDeedEvent>(OnTwinMateDead);
                _initialized = true;
            }
        }

        private void OnTwinMateDead(TwinMateDeedEvent e)
        {
            EnemyId id;
            lock (_lock)
            {
                if (!_initialized || _disposed) return;
                if (e.PairId != _pairId) return;
                if (_enemy == null || e.Id == _enemy.Id) return;
                id = e.Id;
            }
            OnPairMemberDied?.Invoke(id);
        }

        public void Update(float deltaTime)
        {
            lock (_lock)
            {
                if (!_initialized || _disposed) return;
            }

            // No operation. This behavior reacts to domain events and
            // requires no per-frame logic.
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed) return;
                _token?.Dispose();
                _token = null;
                _enemy = null;
                _disposed = true;
            }
        }
    }
}
