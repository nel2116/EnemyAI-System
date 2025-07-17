using app.enemy.core.values;

namespace app.enemy.core.interfaces
{
    /// <summary>
    /// Physics system abstraction for engine independence.
    /// </summary>
    public interface IPhysicsSystem
    {
        bool Raycast(Vector3 origin, Vector3 direction, float maxDistance);
        Vector3 GetGravity();
    }
}
