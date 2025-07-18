using System;
using System.Threading;
using app.enemy.domain.interfaces;

namespace app.enemy.ai.behaviors
{
    public interface IEnrageBehavior : IEnemyBehavior
    {
        event Action OnEnrageTriggered;
        float SpeedMultiplier { get; }
        float AttackMultiplier { get; }
        bool IsEnraged { get; }
        void TriggerEnrage();
    }

    public sealed class EnrageBehavior : IEnrageBehavior
    {
        private readonly float _speedMul;
        private readonly float _atkMul;
        private bool _enraged;
        private readonly object _lock = new();
        private bool _initialized;
        private bool _disposed;

        public event Action? OnEnrageTriggered;

        public float SpeedMultiplier => _speedMul;
        public float AttackMultiplier => _atkMul;
        public bool IsEnraged
        {
            get
            {
                lock (_lock)
                {
                    return _enraged;
                }
            }
        }
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

        public EnrageBehavior(float speedMultiplier, float attackMultiplier)
        {
            _speedMul = speedMultiplier;
            _atkMul = attackMultiplier;
        }

        public void Initialize(IEnemyUnit enemy)
        {
            ArgumentNullException.ThrowIfNull(enemy);
            lock (_lock)
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(EnrageBehavior));

                if (_initialized) return;

                _initialized = true;
            }
        }

        public void Update(float deltaTime)
        {
            lock (_lock)
            {
                if (!_initialized || _disposed) return;
            }

            // No operation. This behavior is event-driven and does not
            // require per-frame updates.
        }

        public void TriggerEnrage()
        {
            lock (_lock)
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(EnrageBehavior));

                if (!_initialized)
                    throw new InvalidOperationException("EnrageBehavior must be initialized before triggering enrage.");

                if (_enraged) return;
                _enraged = true;
                OnEnrageTriggered?.Invoke();
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed) return;
                _disposed = true;
            }
        }
    }
}
