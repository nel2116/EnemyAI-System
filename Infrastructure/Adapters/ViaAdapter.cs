/// <summary>
/// Adapter for via.GameObject implementing IGameObject.
/// </summary>
#nullable enable
using System;
using app.enemy.core.interfaces;
using via;

namespace app.enemy.infrastructure.adapters
{
    public sealed class ViaAdapter : IGameObject
    {
        private readonly GameObject _viaObject;
        private ITransform? _transformCache;

        public ViaAdapter(GameObject viaObject)
        {
            _viaObject = viaObject ?? throw new ArgumentNullException(nameof(viaObject));
        }

        public string Name => _viaObject.Name;

        public ITransform Transform => _transformCache ??= new ViaTransform(_viaObject.Transform);

        public bool IsActive => _viaObject.IsActive;

        public T? GetComponent<T>() where T : class
        {
            try
            {
                return _viaObject.getComponent<T>();
            }
            catch
            {
                return null;
            }
        }
    }
}
