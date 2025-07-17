/// <summary>
/// ワールドから座標と Raycast を取得する最低限の API
/// </summary>
public interface IWorld
{
    vec3 GetPosition(GameObject obj);

    /// <summary>
    /// ID からワールド座標を取得
    /// </summary>
    vec3 GetPosition(GameObjectRef obj);

    /// <summary>
    /// 障害物に当たると true を返す
    /// </summary>
    bool Raycast(vec3 from, vec3 to);
}

public interface ITargetSelector
{
    GameObjectRef GetTarget(IWorld world);
}

public sealed class FixedTargetSelector : ITargetSelector
{
    private readonly GameObjectRef _ref;
    public FixedTargetSelector(GameObjectRef target) => _ref = target;
    public GameObjectRef GetTarget(IWorld _) => _ref;
}
