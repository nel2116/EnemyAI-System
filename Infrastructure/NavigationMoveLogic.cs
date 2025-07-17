/// <summary>
/// NavigationMoveLogic
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using app.enemy.domain.interfaces;
using via;
using via.attribute;

namespace app.enemy.infrastructure
{
    /// <summary>
    /// エンジンの Navigation 機能をラップする簡易 MoveLogic
    /// </summary>
    public sealed class NavigationMoveLogic : IMoveNavigator, IDisposable
    {
        #region Fields
        private readonly NavigationAgent _agent;
        private readonly Func<float> _speed;
        private readonly Action<float> _onSpeed;
        private readonly GameObject _owner;

        public float CurrentSpeed { get; private set; }

        private const float K = 16f;        // 大きいほど俊敏
        private const float C = 4f;         // 大きいほどブレーキ
        private const float ANG_EPS = 0.001f; // 終了閾値

        private float _speedMul = 1.0f;
        private bool _disposed;
        #endregion

        #region Methods
        public void SetSpeedMultiplier(float m) => _speedMul = m;

        public NavigationMoveLogic(Func<float> speedGetter, NavigationAgent agent)
        {
            _speed = speedGetter ?? throw new ArgumentNullException(nameof(speedGetter));
            _agent = agent ?? throw new ArgumentNullException(nameof(agent));
            _owner = _agent.GameObject;

            CurrentSpeed = _speed();
            _onSpeed = v => CurrentSpeed = v;
        }

        public void SetTarget(GameObject target) => _agent.SetDestination(target);

        public void Stop() => _agent.Stop();

        public void Tick(float dt)
        {
            var frameMove = _agent.FrameMove;
            if (frameMove == vec3.Zero) return;

            float speedBase = _speed() * _speedMul;

            // 目標方向
            vec3 targetDir = vector.normalize(new vec3(frameMove.x, 0f, frameMove.z));
            if (targetDir == vec3.Zero) return;

            var tf = _owner.Transform;
            Quaternion currentRot = tf.Rotation;
            vec3 currentFwd = tf.AxisZ;
            Quaternion arcRot = quaternion.makeRotationArc(currentFwd, targetDir);
            Quaternion desiredRot = quaternion.normalize(currentRot * arcRot);

            Quaternion relRot = quaternion.inverse(currentRot) * desiredRot;
            float angleRad = quaternion.getAngle(relRot);

            const float deg30 = 0.523599f;  // 30°
            const float deg60 = 1.047198f;  // 60°

            // 移動速度係数
            float moveFactor = 1f;
            if (angleRad >= deg60) moveFactor = 0f;
            else if (angleRad > deg30)
                moveFactor = 1f - (angleRad - deg30) / (deg60 - deg30);

            float speed = speedBase * moveFactor;

            // 位置更新
            var delta = frameMove * speed * dt;
            tf.Position += delta;

            // 回転更新
            if (angleRad > ANG_EPS)
            {
                const float angSpeed = Math.PI;
                float t = Math.Clamp((angSpeed * dt) / angleRad, 0f, 1f);
                float easeT = 1f - Math.Pow(1f - t, 2f);
                tf.Rotation = quaternion.slerp(currentRot, desiredRot, easeT);
            }
        }

        public bool IsArrived => _agent.NaviState == NavigationAgent.NavigationState.Complete;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
        #endregion
    }
}
