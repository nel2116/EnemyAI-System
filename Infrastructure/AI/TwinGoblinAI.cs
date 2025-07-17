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

            _pairBehavior.OnPairMemberDied += OnPairMemberDied;
            _enrageBehavior.OnEnrageTriggered += OnEnrageTriggered;
        }

        public void Tick(float dt)
        {
            _baseAI.Tick(dt);
            _pairBehavior.Update(dt);
            _enrageBehavior.Update(dt);
        }

        private void OnPairMemberDied(EnemyId id)
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
            if (_move is NavigationMoveLogic nav)
                nav.SetSpeedMultiplier(m);
        }

        private void SetAttackMultiplier(float m)
        {
            if (_combat is SimpleCombatLogic sc)
                sc.SetAttackMultiplier(m);
        }

        public void Dispose()
        {
            (_baseAI as IDisposable)?.Dispose();
            _pairBehavior.Dispose();
            _enrageBehavior.Dispose();
        }
    }
}
