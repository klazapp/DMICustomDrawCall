using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

//Variables to be used in culling job system
[Serializable]
public class ParticleCullingJobComponent
{
    //Basic components
    public NativeArray<float3> visiblePosNa;
    public NativeArray<float3> visibleScaleNa;
    public NativeArray<quaternion> visibleRotNa;
    public NativeArray<Matrix4x4> visibleMatrixNa;

    //Colors components
    public NativeArray<Vector4> colorsNa;
    public NativeArray<float> colorsLerpTNa;
    public NativeArray<float> colorsSpeedNa;

    //Visibility counters components
    public NativeArray<int> visibleCountNa;
    public int visibleCounter;
}
