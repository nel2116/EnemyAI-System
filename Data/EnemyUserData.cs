/// <summary>
/// EnemyUserData
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using via;
using via.attribute;

namespace app.enemy.data
{
    /// <summary>
    /// AI / Factory が解釈するテンプレート種別
    /// Enemyの種類を増やしたときはここを増やしてください
    /// </summary>
    public enum EnemyAITemplate { Basic, Twin, }

    public sealed record PatrolSettings(GameObject[] Points, float WaitSeconds, float ReturnHomeDistance);

    /// <summary>
    /// すべての敵タイプ共通パラメータ
    /// 派生クラスを作れば自由に拡張できます。
    /// </summary>
    [DynamicDisplayName(nameof(InspectorTitle))]
    public class EnemyUserData : via.UserData
    {
        // --- Core Stats ---
        [IgnoreDataMember, GroupSeparator, DisplayName("CoreStats")]
        private const bool grpCore = true;

        [DataMember, DisplayName("最大HP"), Slider(1, 10000)]
        public int MaxHp = 100;

        [DataMember, DisplayName("移動速度"), Slider(0f, 20f)]
        private float _moveSpeed = 5.0f;
        public float MoveSpeed { get { return _moveSpeed; } }

        // --- Detection ---
        [IgnoreDataMember, GroupSeparator, DisplayName("Detection")]
        private const bool grpDetect = true;

        [DataMember, DisplayName("索敵距離(m)"), Slider(0, 100)]
        private float _detectRange = 25f;
        public float DetectRange { get { return _detectRange; } }

        [DataMember, DisplayName("視野角(°)"), Slider(1f, 180f)]
        private float _fovDegree = 90.0f;
        public float FovDegree { get { return _fovDegree; } }

        // --- Patrol ---
        [IgnoreDataMember, GroupSeparator, DisplayName("PatrolStats")]
        private const bool grpPatrol = true;

        [DataMember, DisplayName("巡回ポイント")]
        public GameObjectRef[] PatrolPoints = Array.Empty<GameObjectRef>();

        [DataMember, DisplayName("待機時間(秒)"), Slider(0f, 60f)]
        private float _patrolWaitSeconds = 0f;
        public float PatrolWaitSeconds { get { return _patrolWaitSeconds; } }

        [DataMember, DisplayName("帰還距離(m)"), Slider(0f, 30f)]
        private float _returnHomeDistance = 15f;
        public float ReturnHomeDistance { get { return _returnHomeDistance; } }

        // --- Attack ---
        [IgnoreDataMember, GroupSeparator, DisplayName("AttackStats")]
        private const bool grpAttack = true;

        [DataMember, DisplayName("攻撃力"), Slider(1, 100)]
        private int _attackPower = 10;
        public int AttackPower { get { return _attackPower; } }

        [DataMember, DisplayName("攻撃間隔(秒)"), Slider(0f, 20f)]
        private float _cooldownSeconds = 1.2f;
        public float CooldownSeconds { get { return _cooldownSeconds; } }

        [DataMember, DisplayName("攻撃射程(m)"), Slider(0f, 30f)]
        private float _attackRange = 2.5f;
        public float AttackRange { get { return _attackRange; } }

        // --- Other ---
        [IgnoreDataMember, GroupEndSeparator]
        private const bool grpEnd = false;

        [DataMember, DisplayName("AIテンプレート")]
        public EnemyAITemplate Template = EnemyAITemplate.Basic;

        // --- Dynamic Label ---
        [IgnoreDataMember]
        public string InspectorTitle => $"{Template} Enemy";

        // --- Helper ---
        public PatrolSettings? Patrol => PatrolPoints.Length == 0 ? null : GetPatrolSettings();

        private PatrolSettings GetPatrolSettings()
        {
            GameObject[] objs = new GameObject[PatrolPoints.Length];
            for (int i = 0; i < PatrolPoints.Length; ++i)
            {
                objs[i] = PatrolPoints[i].Target;
            }

            return new PatrolSettings(objs, PatrolWaitSeconds, ReturnHomeDistance);
        }
    }
}
