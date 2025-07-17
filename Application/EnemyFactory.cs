// ============================================================
// <summary>
// EnemyFactory
// </summary>
// <author>CGC_10_田中 ミノル</author>
// ============================================================

using System;
using System.Collections.Generic;
using app.enemy.ai;
using app.enemy.app.dto;
using app.enemy.data;
using app.enemy.domain;
using app.enemy.domain.events;
using app.enemy.domain.interfaces;
using app.enemy.infrastructure;
using app.enemy.shared;
using via;
using via.attribute;

namespace app.enemy.app
{
    public sealed class EnemyFactory : IEnemyFactory
    {
        // テンプレートごとのビルダー
        private readonly Dictionary<EnemyAITemplate, Builder> _builders;

        // 型エイリアス
        private delegate IEnemyUnit Builder(EnemyUserData d, DomainEventDispatcher disp, IAIContext ctx, IMoveLogic move, PairLink? pair);

        public EnemyFactory()
        {
            _builders = new()
            {
                { EnemyAITemplate.Basic, BuildBasic },
                { EnemyAITemplate.Twin, BuildTwin },
            };
        }

        public IEnemyUnit Create(EnemyUserData data, DomainEventDispatcher dispatcher, IAIContext ctx, IMoveNavigator move, PairLink? pair)
        {
            // テンプレート未登録なら Basic を規定にする
            if (!_builders.TryGetValue(data.Template, out var builder)) builder = BuildBasic;
            return builder(data, dispatcher, ctx, move, pair);
        }

        private static IEnemyUnit BuildBasic(EnemyUserData d, DomainEventDispatcher disp, IAIContext ctx, IMoveLogic move, PairLink? pair)
        {
            var combat = new SimpleCombatLogic(() => d.AttackPower, () => d.CooldownSeconds);

            var cfg = new BasicEnemyAI.Config(
                DetectionRange: () => d.DetectRange,
                AttackRange: () => d.AttackRange,
                PatrolPoints: d.Patrol?.Points ?? Array.Empty<GameObject>(),
                PatrolWaitTime: () => d.Patrol?.WaitSeconds ?? 0f,
                ReturnHomeDist: () => d.Patrol?.ReturnHomeDistance ?? 0f
            );

            var ai = new BasicEnemyAI(ctx, move, combat, disp, cfg);
            var core = new Enemy(ctx.EnemyId, d.MaxHp, 0.3f, disp);

            return new EnemyDomainService(core, move, combat, ai, disp);
        }

        private static IEnemyUnit BuildTwin(EnemyUserData d, DomainEventDispatcher disp, IAIContext ctx, IMoveLogic move, PairLink? pair)
        {
            var td = (TwinGoblinUserData)d;

            var combat = new SimpleCombatLogic(() => d.AttackPower, () => d.CooldownSeconds);

            var cfg = new BasicEnemyAI.Config(
                DetectionRange: () => d.DetectRange,
                AttackRange: () => d.AttackRange,
                PatrolPoints: d.Patrol?.Points ?? Array.Empty<GameObject>(),
                PatrolWaitTime: () => d.Patrol?.WaitSeconds ?? 0f,
                ReturnHomeDist: () => d.Patrol?.ReturnHomeDistance ?? 0f
            );

            var ai = new TwinGoblinAI(ctx, move, combat, disp, cfg, td, pair?.Id ?? Guid.Empty);
            var core = new TwinGoblin(ctx.EnemyId, d.MaxHp, 0.3f, disp, pair?.Id ?? Guid.Empty);

            var unit = new EnemyDomainService(core, move, combat, ai, disp);
            ai.Initialize(unit);
            return unit;
        }
    }
}
