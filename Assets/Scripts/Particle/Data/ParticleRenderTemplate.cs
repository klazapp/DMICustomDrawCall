using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Particle Render Template", menuName = "Templates/Particle/Render", order = 0)]
public class ParticleRenderTemplate : ScriptableObject
{
    [Header("Render Components")]
    public Material renderMat;
    public Mesh renderMesh;

    [Header("Render Colors")]
    public Color startCol;
    public Color endCol;
}
