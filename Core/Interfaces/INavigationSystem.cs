using app.enemy.core.values;

namespace app.enemy.core.interfaces
{
    /// <summary>
    /// Navigation system abstraction for path finding.
    /// </summary>
    public interface INavigationSystem
    {
        bool CalculatePath(Vector3 start, Vector3 end, out Vector3[] path);
        bool IsReachable(Vector3 position);
    }
}
