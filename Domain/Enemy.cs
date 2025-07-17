//====================================================================
// <summary>
// Enemy
// </summary>
// <author>CGC_10_田中 ミノル</author>
//====================================================================
using System;
using System.Collections.Generic;
using app.enemy.domain.events;
using app.enemy.domain.interfaces;
using via;
using via.attribute;

namespace app.enemy.domain
{
    #region Enemy
    public sealed class Enemy
    {
        #region Propertys
        public EnemyId Id { get; }

        public EnemyStatus Status => _status;
        #endregion
        #region Fields
        /// <summary>
        /// エネミーのステータス
        /// </summary>
        private EnemyStatus _status;

        /// <summary>
        /// 情報状態に入る HP 割合(例:0.3f = 30%以下)
        /// </summary>
        private readonly float _enrageThresholdRate;

        /// <summary>
        /// </summary>
        private readonly DomainEventDispatcher _dispatcher;
        #endregion
        #region Methods
        public Enemy(EnemyId id, int maxHp, float enrageThresholdRatio, DomainEventDispatcher dispatcher)
        {
            if (maxHp <= 0) throw new ArgumentOutOfRangeException(nameof(maxHp));
            if (enrageThresholdRatio is < 0f or > 1f)
                throw new ArgumentOutOfRangeException(nameof(enrageThresholdRatio));

            Id = id;
            _status = new EnemyStatus(maxHp, maxHp, false);
            _enrageThresholdRate = enrageThresholdRatio;
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// ダメージを適用。必要に応じてイベントを発火。
        /// </summary>
        public void ApplyDamage(int amount)
        {
            if (amount <= 0) return; // 0以下は無視
            var hp = math.max(0, _status.CurrentHp - amount);
            _status = _status with { CurrentHp = hp };
            _dispatcher.Dispatch(new EnemyDamagedEvent(Id, amount, hp));

            // 情報判定
            if (!_status.IsEnraged && _status.HpRatio < _enrageThresholdRate && hp > 0)
            {
                _status = _status with { IsEnraged = true };
                _dispatcher.Dispatch(new EnemyEnragedEvent(Id));
            }

            // 死亡判定
            if (hp == 0)
            {
                PublishDeathEvent();
            }
        }

        /// <summary>
        /// デバッグ用にHPを全回復
        /// </summary>
        public void RestoreFullHp() => _status = _status with { CurrentHp = _status.MaxHp, IsEnraged = false };
        
        protected virtual void PublishDeathEvent()
        {
            _dispatcher.Dispatch(new EnemyDiedEvent(Id));
        }
        #endregion
    }
    #endregion
    #region EnemyStatus
    /// <summary>
    /// 敵の現在HP・フェーズ状態を表す不変値オブジェクト
    /// </summary>
    public readonly record struct EnemyStatus(int CurrentHp, int MaxHp, bool IsEnraged)
    {
        public float HpRatio => (float)CurrentHp / MaxHp;
    }

    /// <summary>
    /// 敵エンティティの一意 ID
    /// </summary>
    public readonly record struct EnemyId(Guid Value)
    {
        public static EnemyId NewId() => new(Guid.NewGuid());
        public override string ToString() => Value.ToString();
    }
    #endregion
}
