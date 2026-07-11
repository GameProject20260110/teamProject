public interface IPoolCallbackReceiver
{
    // 풀에서 꺼내져 활성화된 직후 호출된다.
    void OnRent();

    // 풀로 반환되어 비활성화되기 직전에 호출된다.
    void OnReturn();
}
