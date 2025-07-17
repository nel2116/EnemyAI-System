
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app.enemy.domain.events
{
	#region EnemyEvents

	/// <summary>
	/// 敵が攻撃を行った（Use()が呼ばれた）
	/// </summary>
	public sealed record EnemyAttackEvent(EnemyId Id) : DomainEventBase;

	/// <summary>
	/// 敵がEnraged状態に入った
	/// </summary>
	public sealed record class EnemyEnragedEvent(EnemyId Id) : DomainEventBase;

	/// <summary>
	/// 敵がダメージを受けた
	/// </summary>
	public sealed record class EnemyDamagedEvent(EnemyId Id, int Damage, int HpAfter) : DomainEventBase;

	public abstract record class EnemyDeathEvent(EnemyId Id) : DomainEventBase;

	/// <summary>
	/// 敵が死亡した
	/// </summary>
	public sealed record class EnemyDiedEvent(EnemyId Id) : EnemyDeathEvent(Id);

	/// <summary>
	/// 片割れが死んだ
	/// </summary>
	public sealed record class TwinMateDeedEvent(EnemyId Id, EnemyId PairId) : EnemyDeathEvent(Id);

	/// <summary>
	/// Goblinが怒った
	/// </summary>
	public sealed record class TwinEnragedEvent(EnemyId Id) : DomainEventBase;

	#endregion
}
