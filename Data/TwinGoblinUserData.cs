/// <summary>
/// TwinGoblinUserData
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

using System;
using System.Collections.Generic;
using app.enemy.data;
using via;
using via.attribute;

namespace app.enemy.data
{
    public sealed class TwinGoblinUserData : EnemyUserData
    {
        [DataMember, DisplayName("Enrage速度倍率"), Slider(1f, 5f)]
        private float _enrageSpeedMul = 2f;
        public float EnrageSpeedMul { get { return _enrageSpeedMul; } }

        [DataMember, DisplayName("Enrage攻撃倍率"), Slider(1f, 5f)]
        private float _enrageAttackMul = 2f;
        public float EnrageAttackMul { get { return _enrageAttackMul; } }

        [DataMember, DisplayName("怒り時のターゲット")]
        private GameObjectRef _enragedTarget = default!;
        public GameObjectRef EnragedTarget { get { return _enragedTarget; } }

        // TODO: 怒り時のエフェクト
    }
}
