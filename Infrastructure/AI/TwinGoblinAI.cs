/// <summary>
/// TwinGoblinAI
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

using System;
using System.Threading;
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
        private readonly DomainEventDispatcher _dispatcher;
        private readonly IAIContext _ctx;
        private readonly IPairBehavior _pairBehavior;
        private readonly IEnrageBehavior _enrageBehavior;
        private readonly object _lock = new();
        private volatile bool _initialized;

        public TwinGoblinAI(
            IAIContext ctx,
            IMoveLogic move,
            ICombatLogic combat,
            DomainEventDispatcher dispatcher,
            BasicEnemyAI.Config cfg,
            TwinGoblinUserData src,
            Guid pair)
        {
            _ctx = ctx;
            _move = move;
            _combat = combat;
            _dispatcher = dispatcher;
            _baseAI = new BasicEnemyAI(ctx, move, combat, dispatcher, cfg);

            _pairBehavior = new PairBehavior(dispatcher, new EnemyId(pair));

            _enrageBehavior = new EnrageBehavior(src.EnrageSpeedMul, src.EnrageAttackMul);

            _pairBehavior.OnPairMemberDied += _ => OnPairMemberDied();
            _enrageBehavior.OnEnrageTriggered += OnEnrageTriggered;
        }

        public void Initialize(IEnemyUnit unit)
        {
            ArgumentNullException.ThrowIfNull(unit);
            lock (_lock)
            {
                if (_initialized) return;
                _baseAI.Initialize(unit);
                _pairBehavior.Initialize(unit);
                _enrageBehavior.Initialize(unit);
                _initialized = true;
            }
        }

        public void Tick(float dt)
        {
            if (!_initialized)
                throw new InvalidOperationException("TwinGoblinAI must be initialized before calling Tick. Call Initialize() first.");

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
            _dispatcher.Dispatch(new TwinEnragedEvent(_ctx.EnemyId));
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
            _baseAI.Dispose();
            _pairBehavior.Dispose();
            _enrageBehavior.Dispose();
        }
    }
}
