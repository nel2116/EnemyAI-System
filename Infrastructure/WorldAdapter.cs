/// <summary>
/// WorldAdapter
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

using System;
using System.Collections.Generic;
using app.enemy.infrastructure;
using via;
using via.attribute;

namespace app
{
    public sealed class WorldAdapter : IWorld
    {
        private readonly string _obstacleLayer;

        public WorldAdapter(string obstacleLayer = "Stage")
            => _obstacleLayer = obstacleLayer;

        public vec3 GetPosition(GameObject obj)
            => obj.Transform.Position;

        public vec3 GetPosition(GameObjectRef objRef)
            => objRef.Target.Transform.Position;

        /// <summary>
        /// start -> end を飛ばし、障害物レイヤーにヒットしたら true を返す
        /// </summary>
        public bool Raycast(vec3 start, vec3 end)
        {
            var query = new via.physics.CastRayQuery();
            query.setRay(start, end);

            var filter = new via.physics.FilterInfo();
            filter.Layer = via.physics.System.getLayerIndex(_obstacleLayer);
            filter.MaskBits = (1U << (int)filter.Layer);
            query.FilterInfo = filter;

            var result = new via.physics.CastRayResult();
            via.physics.System.castRay(query, result);

            return result.Finished && result.NumContactPoints > 0;
        }
    }
}
