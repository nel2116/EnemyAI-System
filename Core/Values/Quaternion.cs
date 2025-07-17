namespace app.enemy.core.values
{
    /// <summary>
    /// Engine agnostic quaternion structure.
    /// </summary>
    public readonly struct Quaternion
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public float W { get; }

        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static readonly Quaternion Identity = new(0f, 0f, 0f, 1f);
    }
}
