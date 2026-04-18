
/// <summary>
/// Initialize가 필요한 클래스달 인터페이스
/// </summary>
public interface IInitializePoolable
{
    void Initialize(object data);
}

/// <summary>
/// Release가 필요한 클래스에 달 인터페이스
/// </summary>
public interface IReleasePoolable
{
    void ReleaseObjectPool();
}