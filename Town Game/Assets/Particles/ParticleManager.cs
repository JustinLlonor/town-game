using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;

public class ParticleManager : MonoBehaviour
{
    public Particle[] particles;
    public static ParticleManager instance;

    [System.Serializable]
    public class Particle
    {
        public string name;
        public GameObject particle;
    }

    private void Awake()
    {
        instance = this;
    }

    //[PunRPC]
    public void SpawnParticle(string name, Vector3 position, Vector3 direction, Vector3 tColor)
    {
        Particle p = Array.Find(particles, particle => particle.name == name);
        Color color = new Color(tColor.x, tColor.y, tColor.z, 1f);
        
        
        if (p == null)
        {
            Debug.LogWarning("Particle " + name + " not found!");
            return;
        }

        GameObject phys = Instantiate(p.particle, position, Quaternion.Euler(direction));
        phys.transform.rotation = Quaternion.LookRotation(phys.transform.up);
        ParticleSystem ps = phys.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule psmain = ps.main;
        psmain.startColor = color;
    }
}
