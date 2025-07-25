using System;
using app.enemy.domain;
using app.enemy.domain.events;
using app.enemy.domain.interfaces;

namespace app.enemy.ai.behaviors
{
    public interface IEnemyBehavior
    {
        void Initialize(IEnemyUnit enemy);
        void Update(float deltaTime);
        void Dispose();
    }

    public interface IPairBehavior : IEnemyBehavior
    {
        event Action<EnemyId> OnPairMemberDied;
        bool IsPaired { get; }
    }

    public sealed class PairBehavior : IPairBehavior
    {
        private readonly DomainEventDispatcher _dispatcher;
        private EnemyId _pairId;
        private IDisposable? _token;
        private bool _initialized;
        private IEnemyUnit? _enemy;

        public event Action<EnemyId>? OnPairMemberDied;

        public bool IsPaired => _initialized;

        public PairBehavior(DomainEventDispatcher dispatcher, EnemyId pairId)
        {
            _dispatcher = dispatcher;
            _pairId = pairId;
        }

        public void Initialize(IEnemyUnit enemy)
        {
            if (_initialized) return;
            _enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
            _initialized = true;
            _token = _dispatcher.Register<TwinMateDeedEvent>(OnTwinMateDead);
        }

        private void OnTwinMateDead(TwinMateDeedEvent e)
        {
            if (e.PairId != _pairId) return;
            if (_enemy != null && e.Id == _enemy.Id) return;
            OnPairMemberDied?.Invoke(e.Id);
        }

        public void Update(float deltaTime)
        {
            // behavior is event-driven, no per-frame logic
        }

        public void Dispose()
        {
            _token?.Dispose();
        }
    }
}
