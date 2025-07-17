/// <summary>
/// Adapter for via.Transform implementing ITransform.
/// </summary>
#nullable enable
using System;
using app.enemy.core.interfaces;
using app.enemy.core.values;
using app.enemy.infrastructure.extensions;
using via;

namespace app.enemy.infrastructure.adapters
{
    public sealed class ViaTransform : ITransform
    {
        private readonly Transform _viaTransform;

        public ViaTransform(Transform viaTransform)
        {
            _viaTransform = viaTransform ?? throw new ArgumentNullException(nameof(viaTransform));
        }

        public Vector3 Position
        {
            get => _viaTransform.Position.ToVector3();
            set => _viaTransform.Position = value.ToViaVec3();
        }

        public Quaternion Rotation
        {
            get => _viaTransform.Rotation.ToQuaternion();
            set => _viaTransform.Rotation = value.ToViaQuaternion();
        }

        public Vector3 Forward => _viaTransform.Forward.ToVector3();
        public Vector3 Right => _viaTransform.Right.ToVector3();
        public Vector3 Up => _viaTransform.Up.ToVector3();

        public Vector3 Scale
        {
            get => _viaTransform.Scale.ToVector3();
            set => _viaTransform.Scale = value.ToViaVec3();
        }
    }
}
