using System;
using UnityEngine;
using Random = UnityEngine.Random;

//Template for adjustable variables of each particles
[Serializable]
[CreateAssetMenu(fileName = "Particle Adjustable Template", menuName = "Templates/Particle/Adjustable", order = 1)]
public class ParticleAdjustableTemplate : ScriptableObject
{
    [Header("Move Speed")]
    [Range(0.1f, 20f)]
    public float moveSpeedMin = 2f;
    [Range(0.1f, 20f)]
    public float moveSpeedMax = 4f;
    
    [Header("Color Speed")]
    [Range(0.1f, 20f)]
    public float colorsSpeedMin = 2f;
    [Range(0.1f, 20f)]
    public float colorsSpeedMax = 4f;
    
    [Header("Move X Dir")]
    [Range(0.1f, 20f)]
    public float moveXDirMin = -6f;
    [Range(0.1f, 20f)]
    public float moveXDirMax = 6f;
    
    [Header("Move Z Dir")]
    [Range(0.1f, 20f)]
    public float moveZDirMin = -6f;
    [Range(0.1f, 20f)]
    public float moveZDirMax = 6f;
 
    [Header("Move Y Dir")]
    [Range(0.1f, 20f)]
    public float moveYDir = 8f;
    public bool bounceYes;

    public float OnRetrieveRandomMoveSpeed()
    {
        return Random.Range(moveSpeedMin, moveSpeedMax);
    }
    
    public float OnRetrieveRandomColorsSpeed()
    {
        return Random.Range(colorsSpeedMin, colorsSpeedMax);
    }
    
    public float OnRetrieveRandomMoveXDirSpeed()
    {
        return Random.Range(moveXDirMin, moveXDirMax);
    }
    
    public float OnRetrieveRandomMoveZDirSpeed()
    {
        return Random.Range(moveZDirMin, moveZDirMax);
    }
}
