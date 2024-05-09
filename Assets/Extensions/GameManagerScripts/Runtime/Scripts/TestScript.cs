using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField, Audio] private string[] music = new string[0];

   

    [SerializeField, Particle] private string particleToPlayOnSam;

    public void Start()
    {
        //AudioManager.instance.PlayMusicAtRandom(music);
    }
    public void PlaySound()
    {
        AudioManager.instance.PlayWithPitchVariation(music[2], transform);
    }
    public void PlayParticle()
    {
       ParticleSystemHelper.GetInstance().Play(transform, particleToPlayOnSam, 5);
    }
}
