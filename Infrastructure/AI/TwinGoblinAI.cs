/// <summary>
/// TwinGoblinAI
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

using System;
using app.enemy.ai.behaviors;
using app.enemy.data;
using app.enemy.domain;
using app.enemy.domain.events;
using app.enemy.domain.interfaces;
using app.enemy.infrastructure;

namespace app.enemy.ai
{
    public readonly record struct PairLink(Guid Id);

    public sealed class TwinGoblinAI : IEnemyAi, IDisposable
    {
        private readonly BasicEnemyAI _baseAI;
        private readonly IMoveLogic _move;
        private readonly ICombatLogic _combat;
        private readonly IPairBehavior _pairBehavior;
        private readonly IEnrageBehavior _enrageBehavior;
        private bool _initialized;

        public TwinGoblinAI(
            IAIContext ctx,
            IMoveLogic move,
            ICombatLogic combat,
            DomainEventDispatcher dispatcher,
            BasicEnemyAI.Config cfg,
            TwinGoblinUserData src,
            Guid pair)
        {
            _move = move;
            _combat = combat;
            _baseAI = new BasicEnemyAI(ctx, move, combat, dispatcher, cfg);

            _pairBehavior = new PairBehavior(dispatcher, new EnemyId(pair));

            _enrageBehavior = new EnrageBehavior(src.EnrageSpeedMul, src.EnrageAttackMul);

            _pairBehavior.OnPairMemberDied += _ => OnPairMemberDied();
            _enrageBehavior.OnEnrageTriggered += OnEnrageTriggered;
        }

        public void Initialize(IEnemyUnit unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));
            if (_initialized) return;
            _pairBehavior.Initialize(unit);
            _enrageBehavior.Initialize(unit);
            _initialized = true;
        }

        public void Tick(float dt)
        {
            if (!_initialized) throw new InvalidOperationException("TwinGoblinAI is not initialized");
            _baseAI.Tick(dt);
            _pairBehavior.Update(dt);
            _enrageBehavior.Update(dt);
        }

        private void OnPairMemberDied()
        {
            _enrageBehavior.TriggerEnrage();
        }

        private void OnEnrageTriggered()
        {
            SetSpeedMultiplier(_enrageBehavior.SpeedMultiplier);
            SetAttackMultiplier(_enrageBehavior.AttackMultiplier);
        }

        private void SetSpeedMultiplier(float m)
        {
            _move.SetSpeedMultiplier(m);
        }

        private void SetAttackMultiplier(float m)
        {
            _combat.SetAttackMultiplier(m);
        }

        public void Dispose()
        {
            (_baseAI as IDisposable)?.Dispose();
            _pairBehavior.Dispose();
            _enrageBehavior.Dispose();
        }
    }
}
