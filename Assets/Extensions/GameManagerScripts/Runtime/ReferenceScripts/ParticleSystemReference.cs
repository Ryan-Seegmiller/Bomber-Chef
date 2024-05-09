using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Particle System Reference", menuName = "GameData/ReferenceValues/PSReference")]
public class ParticleSystemReference : ScriptableObject
{
    
    [Serializable]
    public class Particles
    {
        public string name;
        public ParticleSystem particle;
        public int amount = 200;
    }
   

    public Particles[] particles;
}
