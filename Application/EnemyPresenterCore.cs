//===============================================================================
// <summary>
// EnemyPresenterCore
// </summary>
// <author>CGC_10_田中 ミノル</author>
//===============================================================================
/// <summary>
/// EnemyPresenterCore
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

#nullable enable

using System;
using System.Collections.Generic;
using app.enemy.ai;
using app.enemy.app.dto;
using app.enemy.data;
using app.enemy.domain;
using app.enemy.domain.events;
using app.enemy.domain.interfaces;
using app.enemy.infrastructure;
using app.enemy.presentation;
using via;
using via.attribute;

namespace app.enemy.app
{
    public sealed class EnemyPresenterCore : IDisposable
    {
        // --- DI 依存 ---
        private readonly IEnemyView _view;
        private readonly IMessageBus _bus;
        private readonly IEnemyFactory _factory;

        // --- 内部状態 ---
        private IEnemyUnit? _unit;
        private DomainEventDispatcher? _dispatcher = null;
        private DomainEventDispatcher? _injectedDispatcher;

        // Register解除用
        private IDisposable? _dispatcherToken;

        // キャッシュして無駄な Render() 呼び出しを抑制
        private EnemyViewModel _lastVm;
        private bool _hasLastVm = false;

        private EnemyUserData? _data;
        private PairLink? _pair;

        #region Methods
        public void InjectDispatcher(DomainEventDispatcher? disp) => _injectedDispatcher = disp;

        public void SetPairLink(PairLink? link) => _pair = link;

        public EnemyPresenterCore(IEnemyView view, IMessageBus bus, IEnemyFactory factory)
        {
            _view = view;
            _bus = bus;
            _factory = factory;
            _lastVm = default;
        }

        public void Initialize(EnemyUserData data, IAIContext ctx, NavigationAgent agent)
        {
            _data = data;

            // DomainEventDispatcher を生成
            _dispatcher = _injectedDispatcher ?? new DomainEventDispatcher();

            // Dispatcher -> Bus へ登録
            _dispatcherToken = _dispatcher.Register(_bus.Publish);

            var move = new NavigationMoveLogic(() => data.MoveSpeed, agent);
            _unit = _factory.Create(_data, _dispatcher, ctx, move, _pair);

            Render();
        }

        public void Tick(float dt)
        {
            _unit?.Tick(dt);
            Render();
        }

        public void ApplyDamage(int amount)
        {
            _unit?.ApplyDamage(amount);
            Render();
        }

        private void Render()
        {
            if (_unit is null) return;

            var vm = new EnemyViewModel(
                HpRatio: _unit.Status.HpRatio,
                IsEnraged: _unit.Status.IsEnraged,
                Speed: _unit.CurrentSpeed
            );

            if (!_hasLastVm || !vm.Equals(_lastVm))
            {
                _view.Render(vm);
                _lastVm = vm;
                _hasLastVm = true;
            }
        }

        public void Dispose()
        {
            _dispatcherToken?.Dispose(); // ハンドラ解除
            _unit?.Dispose(); // Domain 全体を開放
            if (_injectedDispatcher == null)
                _dispatcher?.Dispose();
        }
        #endregion
    }
}

