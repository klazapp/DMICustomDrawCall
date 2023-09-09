using System;
using Unity.Collections;
using Unity.Mathematics;

[Serializable]
public class ParticleMovementJobComponent
{
    //Basic components
    public NativeArray<float3> posNa;
    public NativeArray<float3> scaleNa;
    public NativeArray<quaternion> rotNa;
    
    //Pos and rot multiplier components
    public NativeArray<float> moveSpeedNa;
    public NativeArray<quaternion> rotDirectionNa;

    //X & Z movement direction 
    public NativeArray<float> moveXDirNa;
    public NativeArray<float> moveZDirNa;
    
    //Y movement direction
    public NativeArray<float> moveYDirNa;
    public NativeArray<float> moveYQuadraticXNa;
}
