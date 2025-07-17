/// <summary>
/// TwinGoblinSpawner
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

using System;
using System.Collections.Generic;
using app.enemy.domain.events;
using app.enemy.vias;
using via;
using via.attribute;

namespace app.enemy.infrastructure.spawner
{
    public class TwinGoblinSpawner : via.Behavior
    {
        [DataMember]
        private Prefab _goblinA = default!;
        [DataMember]
        private Prefab _goblinB = default!;

        [DataMember, DisplayName("スポーン距離"), Slider(1f, 5f)]
        private float _offset = 1.5f;

        public override void start()
        {
            // via.debug.infoLine("[Spawner] Start");
            var disp = new DomainEventDispatcher();
            var pairId = Guid.NewGuid();

            vec3 left = new vec3(1f, 0f, 0f);
            vec3 right = new vec3(-1f, 0f, 0f);
            var pos = GameObject.Transform.Position;

            Spawn(_goblinA, pos + left * _offset, disp, pairId);
            Spawn(_goblinB, pos + right * _offset, disp, pairId);
        }
        void Spawn(Prefab prefab, vec3 pos, DomainEventDispatcher disp, Guid id)
        {
            if (prefab == null) return;

            var obj = prefab.instantiate(pos);
            var presenter = obj.getSameComponent<EnemyPresenter>();

            presenter.InjectDispatcher(disp);
            presenter.SetPairLink(new ai.PairLink(id));

            // via.debug.infoLine($"[Spawner] Spawn {obj.Name}");
        }
    }
}
