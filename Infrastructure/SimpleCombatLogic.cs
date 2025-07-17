/// <summary>
/// SimpleCombatLogic
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

#nullable enable

using System;
using System.Collections.Generic;
using app.enemy.domain.interfaces;
using app.enemy.shared;
using via;
using via.attribute;

namespace app.enemy.infrastructure
{
    /// <summary>
    /// 単発攻撃 + 固定クールダウン
    /// </summary>
    public class SimpleCombatLogic : ICombatLogic, IDisposable
    {
        private readonly Func<float> _attackPower;
        private readonly Func<float> _cooldownSeconds;
        private readonly Action<AttackStats> _onStatsChanged;

        private float _atkMul = 1.0f;
        private float _cooldownTimer;

        public float CooldownSeconds => _cooldownSeconds();
        public bool IsReady => _cooldownTimer <= 0f;
        public float CooldownRemaining => _cooldownTimer;

        // Enemy へ Attack 通知するためのフック
        public event Action? OnAttack;

        public void SetAttackMultiplier(float m) => _atkMul = m;

        public int CalcDamage() => (int)(_attackPower() * _atkMul);

        public SimpleCombatLogic(Func<float> attackPower, Func<float> cooldownSeconds)
        {
            _attackPower = attackPower;
            _cooldownSeconds = cooldownSeconds;
            _cooldownTimer = 0f;
            // クールダウン値が変わっても残り時間を維持したいので何もしない
            _onStatsChanged += _ => { };
        }

        public void Tick(float dt)
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer = math.max(0f, _cooldownTimer - dt);
        }

        public void Use()
        {
            if (!IsReady) return;
            _cooldownTimer = _cooldownSeconds();
            // via.debug.infoLine("攻撃！");
            OnAttack?.Invoke();
        }

        public void Dispose()
        {
            OnAttack = null;
        }
    }
}
