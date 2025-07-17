//====================================================================
// <summary>
// EnemyDomainService
// </summary>
// <author>CGC_10_田中 ミノル</author>
//====================================================================
using System;
using System.Collections.Generic;
using app.enemy.domain.events;
using app.enemy.domain.interfaces;
using app.enemy.infrastructure;
using via;
using via.attribute;

namespace app.enemy.domain.services
{
    /// <summary>
    /// Move / Combat / AI を束ね、<see cref="Enemy"/>エンティティの状態変化を外部へ公開するサービス
    /// </summary>
    public sealed class EnemyDomainService : IEnemyUnit
    {
        #region Propertys
        public EnemyStatus Status => _core.Status;
        public float CurrentSpeed => _move.CurrentSpeed;
        #endregion
        #region Fields
        private readonly Enemy _core;
        private readonly IMoveLogic _move;
        private readonly SimpleCombatLogic _combat;
        private readonly IEnemyAi _ai;
        private readonly DomainEventDispatcher _dispatcher;

        private bool _disposed;
        #endregion
        #region Methods
        public EnemyDomainService(Enemy core, IMoveLogic move, SimpleCombatLogic combat, IEnemyAi ai, DomainEventDispatcher dispatcher)
        {
            _core = core;
            _move = move;
            _combat = combat;
            _ai = ai;
            _dispatcher = dispatcher;

            // 攻撃をドメインイベントへ変換
            _combat.OnAttack += OnAttack;
        }

        public void Tick(float dt)
        {
            _ai.Tick(dt);
            _move.Tick(dt);
            _combat.Tick(dt);
        }

        public void ApplyDamage(int amount) => _core.ApplyDamage(amount);

        private void OnAttack() => _dispatcher.Dispatch(new EnemyAttackEvent(_core.Id));

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // イベント解除
            _combat.OnAttack -= OnAttack;

            // ロジック側を破棄
            (_move as IDisposable)?.Dispose();
            _combat.Dispose();
            (_ai as IDisposable)?.Dispose();
        }
        #endregion
    }
}
