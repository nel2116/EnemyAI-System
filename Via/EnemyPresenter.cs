/// <summary>
/// EnemyPresenter
/// </summary>
/// <author>CGC_10_田中 ミノル</author>
#nullable enable
using System;
using System.Collections.Generic;
using app.enemy.ai;
using app.enemy.app;
using app.enemy.data;
using app.enemy.domain.events;
using app.enemy.domain.interfaces;
using app.enemy.infrastructure;
using app.enemy.infrastructure.messaging;
using via;
using via.attribute;
using via.dynamics;

namespace app.enemy.vias
{
    /// <summary>
    /// エンジンとPresenterCoreを接着するブリッジ
    /// </summary>
    public class EnemyPresenter : via.Behavior
    {
        [DataMember]
        private GameObjectRef Target = default!;

        public EnemyUserData Data = null!;
        public SimpleEnemyView View = null!;
        public WorldAdapter World = null!;

        private readonly IMessageBus _bus = new QueueMessageBus();
        private readonly IEnemyFactory _factory = new EnemyFactory();

        private EnemyPresenterCore _core = null!;
        private IAIContext _ctx = null!;

        private float _baseFPS = 60.0f;

        private DomainEventDispatcher? _injectedDispatcher;
        public void InjectDispatcher(DomainEventDispatcher d) => _injectedDispatcher = d;

        private PairLink? _pair;
        public void SetPairLink(PairLink link) => _pair = link;

        public override void start()
        {
            BuildAndInit();
        }

        public override void update()
        {
            via.debug.infoLine("[Presenter] Tick Start");
            float dt = DeltaTime / _baseFPS;

            _ctx.Tick(dt);
            _core.Tick(dt);
            _bus.Flush(0.2f);
            // via.debug.infoLine("[Presenter] Tick End");
        }

        public override void onDestroy()
        {
            _core?.Dispose();
            (_ctx as IDisposable)?.Dispose();
            (_bus as IDisposable)?.Dispose();
        }

        private void BuildAndInit()
        {
            if (_core != null) return;

            var selector = new FixedTargetSelector(Target);
            World = new WorldAdapter("Stage");
            _ctx = new CustomAIContext(World, GameObject, selector, 0.05f);
            var agent = GameObject.getComponent<NavigationAgent>();
            View = new SimpleEnemyView();
            _core = new EnemyPresenterCore(View, _bus, _factory);
            _core.InjectDispatcher(_injectedDispatcher);
            _core.SetPairLink(_pair);
            _core.Initialize(Data, _ctx, agent);
        }

        [Action, DisplayName("死亡させる(テスト用)")]
        public void TestDead()
        {
            int damage = Data.MaxHp;
            _core.ApplyDamage(damage);
        }
    }
}
