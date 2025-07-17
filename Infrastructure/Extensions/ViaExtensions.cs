/// <summary>
/// Extension methods for converting between via engine types and engine-agnostic structures.
/// </summary>
#nullable enable
using app.enemy.core.values;
using via;
using app.enemy.infrastructure.adapters;

namespace app.enemy.infrastructure.extensions
{
    public static class ViaExtensions
    {
        public static Vector3 ToVector3(this vec3 viaVec)
            => new Vector3(viaVec.x, viaVec.y, viaVec.z);

        public static vec3 ToViaVec3(this Vector3 vector)
            => new vec3(vector.X, vector.Y, vector.Z);

        public static Quaternion ToQuaternion(this quaternion viaQuat)
            => new Quaternion(viaQuat.x, viaQuat.y, viaQuat.z, viaQuat.w);

        public static quaternion ToViaQuaternion(this Quaternion quat)
            => new quaternion(quat.X, quat.Y, quat.Z, quat.W);
    }
}
