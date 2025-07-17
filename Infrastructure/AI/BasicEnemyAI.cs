/// <summary>
/// BasicEnemyAI
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

using System;
using System.Collections.Generic;
using app.enemy.domain.enums;
using app.enemy.domain.events;
using app.enemy.domain.interfaces;
using app.enemy.infrastructure;
using via;
using via.attribute;
using via.effect.script;

namespace app.enemy.ai
{
    /// <summary>
    /// 基本AI
    /// </summary>
    public class BasicEnemyAI : IEnemyAi, IDisposable
    {
        #region Fields

        // --- パラメータ ---
        private readonly Func<float> _detectRange;
        private readonly Func<float> _attackRange;
        private readonly GameObject[] _patrolPoints;
        private readonly Func<float> _patrolWait;
        private readonly Func<float> _returnDist;

        // --- 依存コンポーネント ---
        private readonly IAIContext _ctx;
        private readonly IMoveLogic _move;
        private readonly ICombatLogic _combat;
        private readonly DomainEventDispatcher _dispatcher;

        // --- ステート管理 ---
        private readonly Dictionary<AiState, IAiState> _states = new();
        private IAiState _current;
        public AiState Current => _currKey;
        private AiState _currKey;

        #endregion

        #region Methods

        public sealed record Config(
            Func<float> DetectionRange,
            Func<float> AttackRange,
            GameObject[] PatrolPoints,
            Func<float> PatrolWaitTime,
            Func<float> ReturnHomeDist
        );

        public BasicEnemyAI(
            IAIContext ctx,
            IMoveLogic move,
            ICombatLogic combat,
            DomainEventDispatcher dispatcher,
            Config cfg)
        {
            _ctx = ctx;
            _move = move;
            _combat = combat;
            _dispatcher = dispatcher;

            _detectRange = cfg.DetectionRange;
            _attackRange = cfg.AttackRange;
            _patrolPoints = cfg.PatrolPoints.Length == 0 ? new[] { ctx.Self } : cfg.PatrolPoints;
            _patrolWait = cfg.PatrolWaitTime;
            _returnDist = cfg.ReturnHomeDist;

            // ステート生成
            _states[AiState.Idle] = new IdleState(this);
            _states[AiState.Patrol] = new PatrolState(this);
            _states[AiState.Chase] = new ChaseState(this);
            _states[AiState.Attack] = new AttackState(this);
            _states[AiState.Cooldown] = new CooldownState(this);
            _states[AiState.Return] = new ReturnState(this);
            _states[AiState.Dead] = new DeadState(this);

            // 初期ステート
            SwitchState(AiState.Patrol);

            // 死亡イベント登録
            _dispatcher.Register<EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnEnemyDied(EnemyDiedEvent e)
        {
            if (e.Id.Value != _ctx.EnemyId.Value) return;
            if (Current == AiState.Dead) return;
            SwitchState(AiState.Dead);
        }

        public void Tick(float dt)
        {
            // via.debug.infoLine($"[AI] Now State: {_currKey}");
            _current.Tick(dt);

            if (_current is ITransition st && st.TryGetNext(out var next))
            {
                SwitchState(next);
            }
        }

        public void Dispose()
        {
            _current?.Exit();
            _dispatcher?.Dispose();
        }

        protected void SwitchState(AiState next)
        {
            if (_currKey == next) return;

            // via.debug.infoLine($"[AI] {_currKey} -> {next}");
            _current?.Exit();
            _current = _states[next];
            _currKey = next;
            _current.Enter();
        }

        protected void SetSpeedMultiplier(float m)
        {
            _move.SetSpeedMultiplier(m);
        }

        protected void SetAttackMultiplier(float m)
        {
            _combat.SetAttackMultiplier(m);
        }

        #endregion
#region StateBase

private interface ITransition
{
    bool TryGetNext(out AiState next);
}

private abstract class AiStateBase : IAiState, ITransition
{
    protected readonly BasicEnemyAI A;

    protected AiStateBase(BasicEnemyAI a) => A = a;

    public virtual void Enter() { }
    public abstract void Tick(float dt);
    public virtual void Exit() { }

    public virtual bool TryGetNext(out AiState next)
    {
        next = A.Current;
        return false;
    }
}

private sealed class IdleState : AiStateBase
{
    private float _t;

    public IdleState(BasicEnemyAI a) : base(a) { }

    public override void Tick(float dt)
    {
        _t += dt;
    }

    public override bool TryGetNext(out AiState next)
    {
        next = _t > 1f ? AiState.Patrol : AiState.Idle;
        return next != AiState.Idle;
    }
}

private sealed class PatrolState : AiStateBase
{
    private int _idx;
    private float _waitTimer;

    public PatrolState(BasicEnemyAI a) : base(a) { }

    public override void Enter()
    {
        // via.debug.infoLine("[AI] Enter Patrol");
        MoveNext();
        _waitTimer = 0f;
    }

    public override void Tick(float dt)
    {
        // via.debug.infoLine($"[AI] Distance: {A._ctx.DistanceToTarget}");
        if (A._ctx.DistanceToTarget <= A._detectRange()) return;

        var navigator = A._move as IMoveNavigator;
        if (navigator == null) return;

        if (navigator.IsArrived)
        {
            _waitTimer += dt;
            if (_waitTimer >= A._patrolWait())
            {
                MoveNext();
            }
        }
        else
        {
            _waitTimer = 0f;
        }
    }

    public override void Exit()
    {
        // via.debug.infoLine("[AI] Exit Patrol");
    }

    public override bool TryGetNext(out AiState next)
    {
        if (A._ctx.DistanceToTarget <= A._detectRange())
        {
            next = AiState.Chase;
            return true;
        }

        next = AiState.Patrol;
        return false;
    }

    private void MoveNext()
    {
        _waitTimer = 0f;
        var target = A._patrolPoints[_idx];
        // via.debug.infoLine($"[AI] Patrol 移動先: {target.Name}");
        if (A._move is IMoveNavigator nav)
        {
            nav.SetTarget(target);
        }
        _idx = (_idx + 1) % A._patrolPoints.Length;
    }
}

private sealeed class ChaseState : AiStateBase
{
	public ChaseState(BasicEnemyAI a) : base(a) {}
	
	public override void Enter()
	{
		// via.debug.infoLine("[AI] Enter Chase");
		
		var tgt = A._ctx.Target;
		if(A._move is IMoveNavigator nav)
		{
			// via.debug.infoLine($"[AI] Chase SetTarget = {tgt}");
			nav.SetTarget(tgt);
		}
	}

	public override void Tick(float dt)
	{
		if((A._move as IMoveNavigator)?.IsArrived == true)
		{
			// via.debug.infoLine($"[AI] Chase ReSetTarget = {tgt}");
			nav.SetTarget(tgt);
		}
	}
	
	public override void Exit()
	{
		// via.debug.infoLine("[AI] Exit Chase");
	}
	
	public override bool TryGetNext(out AiState next)
	{
		if(A._ctx.DistanceToTarget > A._returnDist())
		{
			next = AiState.Return;
			return true;
		}
		
		if(A._ctx.DistanceToTarget < A._attackRange() && A._combat.IsReady)		
		{
			next = AiState.Attack;
			return true;
		}
		
		next = AiState.Chase;
		return false;
	}
}

private sealed class AttackState : AiStateBase
{
	public AttackState(BasicEnemyAI a) : base(a){}
	public override void Enter() => A._combat.Use();
	public override void Tick(dt){}
	
	public override bool TryGetNext(out AiState next)
	{
		next = AiState.Cooldown;
		return true;
	}
}

private sealed class CooldownState : AiStateBase
{
    public CooldownState(BasicEnemyAI a) : base(a) { }

    public override void Tick(float dt) { }

    public override bool TryGetNext(out AiState next)
    {
        if (A._combat.IsReady)
        {
            // 距離によって Attack を繰り返し or Chase
            next = A._ctx.DistanceToTarget <= A._attackRange()
                ? AiState.Attack
                : AiState.Chase;
            return true;
        }

        next = AiState.Cooldown;
        return false;
    }
}

private sealed class ReturnState : AiStateBase
{
    private readonly GameObject _home;

    public ReturnState(BasicEnemyAI a) : base(a)
        => _home = a._patrolPoints.Length > 0 ? a._patrolPoints[0] : a._ctx.Self;

    public override void Enter()
    {
        if (A._move is IMoveNavigator nav)
        {
            nav.SetTarget(_home);
        }
    }

    public override void Tick(float dt) { }

    public override bool TryGetNext(out AiState next)
    {
        if (vector.distance(A._ctx.SelfPosition, _home.Transform.Position) < 0.5f)
        {
            next = AiState.Idle;
            return true;
        }

        next = AiState.Return;
        return false;
    }
}

private sealed class DeadState : AiStateBase
{
    public DeadState(BasicEnemyAI a) : base(a) { }

    public override void Tick(float dt) { }

    public override void Enter()
    {
        // 行動停止
        (A._move as IMoveNavigator)?.Stop();

        // TODO: A._combat.Cancel();
        // TODO: 死亡アニメーション
        // TODO: オブジェクト破棄
    }
}

#endregion
	}
}
