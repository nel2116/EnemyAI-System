namespace app.enemy.core.interfaces
{
    /// <summary>
    /// Abstraction of engine-specific GameObject.
    /// </summary>
    public interface IGameObject
    {
        string Name { get; }
        ITransform Transform { get; }
        T? GetComponent<T>() where T : class;
        bool IsActive { get; }
    }
}
