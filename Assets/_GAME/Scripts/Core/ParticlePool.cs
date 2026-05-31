using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviour
{
    public static ParticlePool Instance;

    [SerializeField] private ParticleSystem[] HitFx;

    private int currentHitFx;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayHit(Vector3 pos)
    {
        HitFx[currentHitFx].transform.position = pos;
        HitFx[currentHitFx].Play();
        currentHitFx++;
        if (currentHitFx == HitFx.Length)
            currentHitFx = 0;
    }
    

}
