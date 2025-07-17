using app.enemy.core.values;

namespace app.enemy.core.interfaces
{
    /// <summary>
    /// Abstraction of engine-specific Transform component.
    /// </summary>
    public interface ITransform
    {
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
        Vector3 Forward { get; }
        Vector3 Right { get; }
        Vector3 Up { get; }
        Vector3 Scale { get; set; }
    }
}
