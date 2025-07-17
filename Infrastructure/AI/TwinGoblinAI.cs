/// <summary>
/// TwinGoblinAI
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

using System;
using System.Collections.Generic;
using app.enemy.data;
using app.enemy.domain;
using app.enemy.domain.enums;
using app.enemy.domain.events;
using app.enemy.domain.interfaces;
using via;
using via.attribute;

namespace app.enemy.ai
{
    public readonly record struct PairLink(Guid Id);

    public sealed class TwinGoblinAI : BasicEnemyAI
    {
        private readonly EnemyId _pairId;
        private readonly Func<float> _enrageSpeedMul;
        private readonly Func<float> _enrageAtkMul;
        private readonly GameObject _enragedTarget;
        private readonly DomainEventDispatcher _disp;
        private readonly IAIContext _ctx;
        private bool _enraged;

        public TwinGoblinAI(IAIContext ctx, IMoveLogic move, ICombatLogic combat, DomainEventDispatcher dispatcher, BasicEnemyAI.Config cfg, TwinGoblinUserData src, Guid pair)
            : base(ctx, move, combat, dispatcher, cfg)
        {
            _enrageSpeedMul = () => src.EnrageSpeedMul;
            _enrageAtkMul = () => src.EnrageAttackMul;
            _ctx = ctx;
            _disp = dispatcher;
            _enragedTarget = src.EnragedTarget.Target;
            _pairId = new EnemyId(pair);

            // TODO: 少女を取得し _enragedTarget に格納
            _disp.Register<TwinMateDeedEvent>(OnMateDied);
            _disp.Register<TwinMateDeedEvent>(OnSelfDied);
        }

        void OnSelfDied(TwinMateDeedEvent e)
        {
            if (e.Id.Value != _ctx.EnemyId.Value) return;
            if (Current == AiState.Dead) return;
            SwitchState(AiState.Dead);
        }

        void OnMateDied(TwinMateDeedEvent e)
        {
            if (_enraged || e.PairId != _pairId || e.Id == _ctx.EnemyId) return;

            _enraged = true;
            SetSpeedMultiplier(_enrageSpeedMul());
            SetAttackMultiplier(_enrageAtkMul());

            // TODO: ターゲットの変更

            _disp.Dispatch(new TwinEnragedEvent(_ctx.EnemyId));
        }
    }
}
