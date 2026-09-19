using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PooledParticle : MonoBehaviour, IPoolCallbackReceiver
{
    [SerializeField] private string sfxKey;
    [SerializeField] private bool playSoundOnRent = true;

    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    public void OnRent()
    {
        ps.Play();
        if (playSoundOnRent && !string.IsNullOrEmpty(sfxKey))
            AudioManager.Instance?.PlaySfx(sfxKey);
    }

    public void OnReturn() => ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

    private void OnParticleSystemStopped() // Callback π›¿¿
    {
        WorldPoolManager.instance.Return(gameObject);
    }
}
