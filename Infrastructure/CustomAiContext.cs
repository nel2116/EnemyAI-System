/// <summary>
/// CustomAiContext
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

#nullable enable

using System;
using System.Collections.Generic;
using app.enemy.domain;
using app.enemy.domain.interfaces;
using via;
using via.attribute;

namespace app.enemy.infrastructure
{
    public sealed class CustomAiContext : IAIContext, IDisposable
    {
        #region Fields
        private readonly IWorld _world;
        private readonly GameObject _self;
        private readonly ITargetSelector _selector;
        private readonly EnemyId _enemyId;

        // キャッシュ
        private GameObjectRef _target;
        private vec3 _cachedSelfPos;
        private vec3 _cachedTargetPos;
        private float _cachedDistance;
        private bool _cachedLos;

        // 更新タイミング制御
        private readonly float _checkInterval;
        private float _timer;

        private bool _disposed;

        // 定数
        private static readonly vec3 EyeOffset = new vec3(0f, 1.6f, 0f);
        #endregion
        #region Properties
        public GameObject Self => _self;
        public GameObject Target => _target.Target;
        public vec3 SelfPosition => _cachedSelfPos;
        public vec3 TargetPosition => _cachedTargetPos;
        public float DistanceToTarget => _cachedDistance;
        public bool HasLineOfSight => _cachedLos;
        public EnemyId EnemyId => _enemyId;
        #endregion

        #region Methods
        public CustomAiContext(
            IWorld world,
            GameObject self,
            ITargetSelector selector,
            float losCheckInterval = 0.05f)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _self = self ?? throw new ArgumentNullException(nameof(self));
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
            _checkInterval = math.max(0.02f, losCheckInterval);
            _enemyId = EnemyId.NewId();

            // 初期キャッシュ
            _target = selector.GetTarget(world);
            UpdateCache();
        }

        public void Tick(float dt)
        {
            _timer += dt;
            if (_timer < _checkInterval) return;
            _timer = 0;

            // ターゲット再選択
            _target = _selector.GetTarget(_world);
            UpdateCache();
        }

        private void UpdateCache()
        {
            _cachedSelfPos = _world.GetPosition(_self);
            _cachedTargetPos = _world.GetPosition(_target);
            _cachedDistance = vector.distance(_cachedSelfPos + EyeOffset, _cachedTargetPos + EyeOffset);
            _cachedLos = !_world.Raycast(_cachedSelfPos, _cachedTargetPos);
        }

        public T? GetExtra<T>() where T : class
            => typeof(T) == typeof(GameObjectRef) ? _target as T : null;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            // キャッシュ解放や World 監視解除など必要ならここで
        }
        #endregion
    }    
}

