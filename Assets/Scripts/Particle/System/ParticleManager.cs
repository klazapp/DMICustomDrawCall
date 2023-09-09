using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class ParticleManager : MonoBehaviour
{
    #region Templates
    [Header("Render Template")] 
    [SerializeField]
    private ParticleRenderTemplate particleRenderTemplate;

    [Header("Adjustable Template")]
    [SerializeField]
    private ParticleAdjustableTemplate particleAdjustableTemplate;
    #endregion
    
    //Mat props block
    private MaterialPropertyBlock matPropBlock;
   
    //Colors
    private Vector4[] particleColors;

    //Matrix
    private Matrix4x4[] particleMatrices;
    
    //Index
    private int currentIndexMin;
    private int currentIndexMax;
    
    //Total count
    private const int TOTAL_COUNT = 1023;

    //Job components
    #region Culling Job
    private ParticleCullingJobSystem particleCullingJobSystem;
    private JobHandle particleCullingJobHandle;
    private ParticleCullingJobComponent particleCullingJobComponent;
    #endregion
    
    #region Movement Job
    private ParticleMovementJobSystem particleMovementJobSystem;
    private JobHandle particleMovementJobHandle;
    private ParticleMovementJobComponent particleMovementJobComponent;
    #endregion

    private void Awake()
    {
        //Init values 
        particleMatrices = new Matrix4x4[TOTAL_COUNT];
        particleColors = new Vector4[TOTAL_COUNT];

        for (var i = 0; i < TOTAL_COUNT; i++)
        {
            particleColors[i] = particleRenderTemplate.startCol;
        }

        matPropBlock = new MaterialPropertyBlock();
        
        //Init job components
        particleCullingJobComponent = new();
        particleMovementJobComponent = new();
    }

    private void OnEnable()
    {
        AllocateNa();
        SubscribeEvents();
    }

    private void OnDisable()
    {
        DeallocateNa();
        UnsubscribeEvents();
    }
    
    private void LateUpdate()
    {
        //Complete job systems
        CompleteJobSystems();

        //Retrieve job values
        RetrieveCullingJobValues();
        RetrieveMovementJobValues();
        
        //Set mat props block
        matPropBlock.SetVectorArray(GlobalManager._ColorsINT, particleColors);

        //Draw mesh
        //Alternate with other variants of graphics draw mesh commands
        //for different use cases
        Graphics.DrawMeshInstanced(particleRenderTemplate.renderMesh, 0, particleRenderTemplate.renderMat, particleMatrices, particleCullingJobComponent.visibleCounter, matPropBlock, ShadowCastingMode.Off, false);

        //Assign job values
        AssignCullingJobValues();
        AssignMovementJobValues();
        
        //Schedule jobs
        ScheduleCullingJob();
        ScheduleMovementJob();
    }
    
    #region Jobs
    //
    //
    
    //Allocates and deallocates all native arrays
    #region Allocation/Deallocations
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AllocateNa()
    {
        #region Culling Job
        //Basic components
        particleCullingJobComponent.visiblePosNa = new NativeArray<float3>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        particleCullingJobComponent.visibleScaleNa = new NativeArray<float3>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        particleCullingJobComponent.visibleRotNa = new NativeArray<quaternion>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        particleCullingJobComponent.visibleMatrixNa = new NativeArray<Matrix4x4>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        
        //Visibility counter
        particleCullingJobComponent.visibleCountNa = new NativeArray<int>(1, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        
        //Colors
        particleCullingJobComponent.colorsNa = new NativeArray<Vector4>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        particleCullingJobComponent.colorsLerpTNa = new NativeArray<float>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        particleCullingJobComponent.colorsSpeedNa = new NativeArray<float>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        #endregion
        
        #region Movement Job
        //Basic components
        particleMovementJobComponent.posNa = new NativeArray<float3>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        particleMovementJobComponent.scaleNa = new NativeArray<float3>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        particleMovementJobComponent.rotNa = new NativeArray<quaternion>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        
        //Pos and rot multiplier
        particleMovementJobComponent.moveSpeedNa = new NativeArray<float>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        particleMovementJobComponent.rotDirectionNa = new NativeArray<quaternion>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        
        //X & Z movement direction         
        particleMovementJobComponent.moveXDirNa = new NativeArray<float>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        particleMovementJobComponent.moveZDirNa = new NativeArray<float>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        
        //Y movement direction
        particleMovementJobComponent.moveYDirNa = new NativeArray<float>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        particleMovementJobComponent.moveYQuadraticXNa = new NativeArray<float>(TOTAL_COUNT, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        #endregion
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DeallocateNa()
    {
        //Force complete all job systems before deallocation
        particleCullingJobHandle.Complete();
        particleMovementJobHandle.Complete();
        
        //Deallocate all NA
        #region Culling Job
        particleCullingJobComponent.visiblePosNa.Dispose();
        particleCullingJobComponent.visibleScaleNa.Dispose();
        particleCullingJobComponent.visibleRotNa.Dispose();
        particleCullingJobComponent.visibleMatrixNa.Dispose();
 
        //Visibility counter
        particleCullingJobComponent.visibleCountNa.Dispose();
 
        //Colors
        particleCullingJobComponent.colorsNa.Dispose();
        particleCullingJobComponent.colorsLerpTNa.Dispose();
        particleCullingJobComponent.colorsSpeedNa.Dispose();
        #endregion
        
        #region Movement Job
        particleMovementJobComponent.posNa.Dispose();
        particleMovementJobComponent.scaleNa.Dispose();
        particleMovementJobComponent.rotNa.Dispose();
 
        //Pos and rot multiplier
        particleMovementJobComponent.moveSpeedNa.Dispose(); 
        particleMovementJobComponent.rotDirectionNa.Dispose();
 
        //X & Z movement direction         
        particleMovementJobComponent.moveXDirNa.Dispose();
        particleMovementJobComponent.moveZDirNa.Dispose();
 
        //Y movement direction
        particleMovementJobComponent.moveYDirNa.Dispose();
        particleMovementJobComponent.moveYQuadraticXNa.Dispose();
        #endregion
    }
    #endregion
    
    //Completes all job systems
    #region Complete Job Systems
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CompleteJobSystems()
    {
        particleCullingJobHandle.Complete();
        particleMovementJobHandle.Complete();
    }
    #endregion
    
    //Retrieve values
    #region Retrieve Values
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RetrieveCullingJobValues()
    {
        particleCullingJobComponent.visibleMatrixNa.CopyTo(particleMatrices);
        particleCullingJobComponent.visibleCounter = particleCullingJobComponent.visibleCountNa[0];
        
        particleCullingJobComponent.colorsNa.CopyTo(particleColors);
        particleCullingJobComponent.colorsLerpTNa.CopyTo(particleCullingJobComponent.colorsLerpTNa);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RetrieveMovementJobValues()
    {
        particleMovementJobComponent.moveYDirNa.CopyTo(particleMovementJobComponent.moveYDirNa);
    }
    #endregion
    
    //Assign values
    #region Assign Values
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssignCullingJobValues()
    {
        particleCullingJobComponent.visiblePosNa.CopyFrom(particleMovementJobComponent.posNa);
        particleCullingJobComponent.visibleScaleNa.CopyFrom(particleMovementJobComponent.scaleNa);
        particleCullingJobComponent.visibleRotNa.CopyFrom(particleMovementJobComponent.rotNa);

        particleCullingJobComponent.colorsLerpTNa.CopyFrom(particleCullingJobComponent.colorsLerpTNa);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssignMovementJobValues()
    {
        particleMovementJobComponent.moveYDirNa.CopyFrom(particleMovementJobComponent.moveYDirNa);
    }
    #endregion
    
    //Schedule
    #region Schedules
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScheduleCullingJob()
    {
        particleCullingJobHandle = new ParticleCullingJobSystem
        {
            //Basic components
            visiblePosNa = particleCullingJobComponent.visiblePosNa,
            visibleScaleNa = particleCullingJobComponent.visibleScaleNa,
            visibleRotNa = particleCullingJobComponent.visibleRotNa,
            visibleMatrixNa = particleCullingJobComponent.visibleMatrixNa,
            
            //Colors
            colorsNa = particleCullingJobComponent.colorsNa,
            colorsLerpT = particleCullingJobComponent.colorsLerpTNa,
            colorsSpeedNa = particleCullingJobComponent.colorsSpeedNa,
            startColor = particleRenderTemplate.startCol,
            endColor = particleRenderTemplate.endCol,
            
            //Visibility counters
            visibleCountNa = particleCullingJobComponent.visibleCountNa,
            
            //Misc
            deltaTime = GlobalManager.deltaTime,
            totalCount = TOTAL_COUNT,
            
        }.Schedule();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScheduleMovementJob()
    {
        particleMovementJobHandle = new ParticleMovementJobSystem()
        {
            //Basic components
            posNa = particleMovementJobComponent.posNa,
            scaleNa = particleMovementJobComponent.scaleNa,
            rotNa = particleMovementJobComponent.rotNa,
            
            //Pos and rot multiplier components
            moveSpeedNa = particleMovementJobComponent.moveSpeedNa,
            rotDirectionNa = particleMovementJobComponent.rotDirectionNa,
            
            //X & Z movement direction 
            moveXDirNa = particleMovementJobComponent.moveXDirNa,
            moveZDirNa = particleMovementJobComponent.moveZDirNa,

            //Y movement direction
            moveYDirNa = particleMovementJobComponent.moveYDirNa,
            moveYQuadraticXNa = particleMovementJobComponent.moveYQuadraticXNa,
                
            //Misc
            deltaTime = GlobalManager.deltaTime,
            bounceYes = particleAdjustableTemplate.bounceYes,
    
        }.Schedule(TOTAL_COUNT, 256);
    }
    #endregion
    
    //Culling + movement job system
    #region Systems
    //Culling job
    [BurstCompile]
    private struct ParticleCullingJobSystem : IJob
    {
        //Basic components
        [ReadOnly] 
        public NativeArray<float3> visiblePosNa;
        [ReadOnly] 
        public NativeArray<float3> visibleScaleNa;
        [ReadOnly] 
        public NativeArray<quaternion> visibleRotNa;
        [WriteOnly] 
        public NativeArray<Matrix4x4> visibleMatrixNa;

        //Colors
        public NativeArray<Vector4> colorsNa;
        public NativeArray<float> colorsLerpT;
        [ReadOnly] 
        public NativeArray<float> colorsSpeedNa;
        [ReadOnly] 
        public Color startColor;
        [ReadOnly] 
        public Color endColor;

        //Visibility counters
        [WriteOnly] 
        public NativeArray<int> visibleCountNa;
        private int visibleCounter;
        
        //Misc
        [ReadOnly] 
        public float deltaTime;
        [ReadOnly]
        public int totalCount;
        
        public void Execute()
        {
            visibleCounter = 0;
            for (var i = 0; i < totalCount; i++)
            {
                if (visibleScaleNa[i].x <= 0)
                    continue;

                //(scale > 0) --> (visible = true)
                visibleMatrixNa[visibleCounter] = Matrix4x4.TRS(visiblePosNa[i], visibleRotNa[i], visibleScaleNa[i]);

                //Colors over time
                colorsLerpT[i] = math.clamp(colorsLerpT[i] + deltaTime * colorsSpeedNa[i], 0f, 1f);
                colorsNa[visibleCounter] = Color.Lerp(startColor, endColor, colorsLerpT[i]);
                
                //Convert gamma to linear space
                colorsNa[visibleCounter] = math.pow(colorsNa[visibleCounter], 2.2f);
                
                visibleCounter++;
            }

            //Make sure visible count does not exceed maximum possible
            visibleCountNa[0] = math.clamp(visibleCounter, 0, totalCount);
        }
    }
    
    //Movement job
    [BurstCompile]
    private struct ParticleMovementJobSystem : IJobParallelFor
    {
        //Basic components
        public NativeArray<float3> posNa;
        public NativeArray<float3> scaleNa;
        public NativeArray<quaternion> rotNa;
        
        //Pos and rot multiplier components
        [ReadOnly] 
        public NativeArray<float> moveSpeedNa;
        [ReadOnly] 
        public NativeArray<quaternion> rotDirectionNa;

        //X & Z movement direction 
        [ReadOnly] 
        public NativeArray<float> moveXDirNa;
        [ReadOnly] 
        public NativeArray<float> moveZDirNa;
        
        //Y movement direction
        public NativeArray<float> moveYDirNa;
        public NativeArray<float> moveYQuadraticXNa;
        
        //Misc
        [ReadOnly] 
        public float deltaTime;
        private float3 tempPos;
        [ReadOnly] 
        public bool bounceYes;

        public void Execute(int index)
        {
            if (scaleNa[index].x <= 0)
                return;
            
            //Scale reduction
            //Reduction to 0 slowed down if bounceYes = true
            scaleNa[index] = math.clamp(scaleNa[index] - deltaTime * moveSpeedNa[index] / (bounceYes ? 3f : 1f), 0f, 1f);

            //Rotation over time
            rotNa[index] = math.normalize(
                math.slerp(rotNa[index], 
                    math.mul(rotNa[index], rotDirectionNa[index]), moveSpeedNa[index] * deltaTime)
            );
            
            //Position over time
            tempPos = posNa[index];

            moveYQuadraticXNa[index] =
                math.clamp(moveYQuadraticXNa[index] + deltaTime * moveSpeedNa[index], 0f, 1f);
            
            //Bounce by reducing Y amplitude
            if (moveYQuadraticXNa[index] >= 1f)
            {
                moveYQuadraticXNa[index] = 0;
                moveYDirNa[index] = math.clamp(moveYDirNa[index] / 2f, 0f, 20f);
            }
            
            //Y quadratic curve movement
            tempPos.y = -moveYDirNa[index] * math.pow(moveYQuadraticXNa[index], 2f) + moveYDirNa[index] * moveYQuadraticXNa[index];

            //X, Z linear movement
            tempPos.x += moveXDirNa[index] * moveSpeedNa[index] * deltaTime;
            tempPos.z += moveZDirNa[index] * moveSpeedNa[index] * deltaTime;
            
            posNa[index] = tempPos;
        }
    }
    #endregion
    
    //
    //
    #endregion
    
    #region Event
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SubscribeEvents()
    {
        EventManager.GenerateParticlesEvent += GenerateParticlesCallback;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UnsubscribeEvents()
    {
        EventManager.GenerateParticlesEvent -= GenerateParticlesCallback;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateParticlesCallback()
    {
        currentIndexMax += Random.Range(10, 20);

        //Reset to 0 if exceed max count
        if (currentIndexMax > TOTAL_COUNT)
        {
            currentIndexMin = 0;
            currentIndexMax = Random.Range(10, 20);
        }
        
        //Complete job systems
        CompleteJobSystems();
        
        //Assign new values
        for (var i = currentIndexMin; i < currentIndexMax; i++)
        {
            //Basic components
            particleMovementJobComponent.posNa[i] = 0;
            particleMovementJobComponent.scaleNa[i] = 1;
            particleMovementJobComponent.rotNa[i] = Random.rotation;
            
            particleMovementJobComponent.rotDirectionNa[i] = Random.rotation;
            
            //Adjustable
            particleMovementJobComponent.moveSpeedNa[i] = particleAdjustableTemplate.OnRetrieveRandomMoveSpeed();
            particleCullingJobComponent.colorsSpeedNa[i] = particleAdjustableTemplate.OnRetrieveRandomColorsSpeed();
            particleMovementJobComponent.moveXDirNa[i] = particleAdjustableTemplate.OnRetrieveRandomMoveXDirSpeed();
            particleMovementJobComponent.moveZDirNa[i] = particleAdjustableTemplate.OnRetrieveRandomMoveZDirSpeed();
            particleMovementJobComponent.moveYDirNa[i] = particleAdjustableTemplate.moveYDir;

            particleMovementJobComponent.moveYQuadraticXNa[i] = 0;

            particleCullingJobComponent.colorsNa[i] = particleRenderTemplate.startCol;
            particleCullingJobComponent.colorsLerpTNa[i] = 0;
        }

        currentIndexMin = currentIndexMax;
    }
    #endregion
}
