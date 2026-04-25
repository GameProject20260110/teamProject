using DG.Tweening;
using UnityEngine;

public class VFXEffectHandler : MonoBehaviour
{
    public ParticleSystem diceParticle;

    public void PlayDiceParticle(Dice dice)
    {
        if (diceParticle == null || dice == null) return;
        diceParticle.transform.position = dice.transform.position;
        diceParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        diceParticle.Play();
    }
}
