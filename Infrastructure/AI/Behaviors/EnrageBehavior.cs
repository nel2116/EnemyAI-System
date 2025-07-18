using System;
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
        private IEnemyUnit? _enemy;

        public event Action? OnEnrageTriggered;

        public float SpeedMultiplier => _speedMul;
        public float AttackMultiplier => _atkMul;
        public bool IsEnraged => _enraged;

        public EnrageBehavior(float speedMultiplier, float attackMultiplier)
        {
            _speedMul = speedMultiplier;
            _atkMul = attackMultiplier;
        }

        public void Initialize(IEnemyUnit enemy)
        {
            ArgumentNullException.ThrowIfNull(enemy);
            _enemy = enemy;
        }

        public void Update(float deltaTime)
        {
            // This behavior is event-driven and does not require per-frame updates.
        }

        public void TriggerEnrage()
        {
            if (_enraged) return;
            _enraged = true;
            OnEnrageTriggered?.Invoke();
        }

        public void Dispose()
        {
            // No cleanup is needed as this class does not allocate unmanaged resources.
        }
    }
}
