using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class DMICustomCulling: MonoBehaviour
{
    
    #region MISC
    private readonly int _ColorsINT = Shader.PropertyToID("_Colors");
    private float DeltaTime;
    #endregion
    
    
    
    [Header("DMI Base Components")]
    [SerializeField] 
    private Material cubeMat;
    [SerializeField] 
    private Mesh cubeMesh;
    

    #region Adjustables
    [Header("DMI Universal Speed Range (0.1f ~ 20f)")]
    [Space(40)]
    
    [Range(0.1f, 20f)]
    [SerializeField] 
    private float cubeUniversalSpeedMin = 2f;
    [Range(0.1f, 20f)]
    [SerializeField] 
    private float cubeUniversalSpeedMax = 4f;

    [Header("DMI X Move Dir Range  (-10f ~ 10f)")]
    [Space(20)]
    
    //Random.Range vals for cube X Move Dir
    [Range(-10f, 10f)]
    [SerializeField] 
    private float cubeMoveXDirMin = -6f;
    [Range(-10f, 10f)]
    [SerializeField] 
    private float cubeMoveXDirMax = 6f;
    
    
    [Header("DMI Z Move Dir Range (-10f ~ 10f)")]
    [Space(20)]
    
    //Random.Range vals for cube Z Move Dir
    [Range(-10f, 10f)]
    [SerializeField] 
    private float cubeMoveZDirMin = -6f;
    [Range(-10f, 10f)]
    [SerializeField] 
    private float cubeMoveZDirMax = 6f;
    #endregion
    
    [Header("DMI Y Move Dir Range (0f ~ 10f)")]
    [Space(20)]
    
    [Range(0f, 10f)]
    [SerializeField]
    private float cubeMoveYDir = 8f;


    [Header("Y Bounce")]
    [Space(20)]
    [SerializeField]
    private bool bounceYes = true;
    
    
    
    //Mat Props Block
    [Header("Mat Props Block")] 
    [Space(40)]
    [SerializeField]
    private Color startCol;
    [SerializeField]
    private Color endCol;
    private MaterialPropertyBlock matPropsBlock;
    private Vector4[] CubeCols;
    
    

    //matrix and index
    private Matrix4x4[] CubeMatrices;
    private const int TotalCount = 1023;
    private int CubeCurrentIndexMin;
    private int CubeCurrentIndexMax;

    

    
    
    
    //Cube Culling Job Stuffs
    #region cube culling job stuffs
    private CubeCullingJobSystem cubeCullingJobSystem;
    private JobHandle cubeCullingJobHandle;


    //Basic components
    private NativeArray<float3> cubeVisiblePosNA;
    private NativeArray<float3> cubeVisibleScaleNA;
    private NativeArray<quaternion> cubeVisibleRotNA;
    private NativeArray<Matrix4x4> cubeVisbleMatricesNA;


    //Colors
    private NativeArray<Vector4> cubeColsNA;
    private NativeArray<float> cubeColsLerpT;


    //Visiblity counters
    private NativeArray<int> visibleCountNA;
    private int visibleCount;
    #endregion

    
    
    //Cube Movement Job Stuffs
    #region cube movement job stuffs
    private CubeMovementJobSystem cubeMovementJobSystem;
    private JobHandle cubemovementJobHandle;


    //Basic components
    private NativeArray<float3> CubePosNA;
    private NativeArray<float3> CubeScaleNA;
    private NativeArray<quaternion> CubeRotNA;
    
    
    //Basic position and rotation multipliers
    private NativeArray<float> CubeUniversalSpeedNA;
    private NativeArray<quaternion> CubeRotatingDirectionNA;


    //X & Z movement direction
    private NativeArray<float> CubeMoveXDirNA;
    private NativeArray<float> CubeMoveZDirNA;
    
    //Y movement direction
    private NativeArray<float> CubeMoveYDirNA;
    private NativeArray<float> CubeMoveYQuadraticXNA;
    #endregion





    private void OnEnable()
    {
        
        //Allocate ALL NA
        #region Initialize Culling Job NA
        cubeVisiblePosNA = new NativeArray<float3>(TotalCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        cubeVisibleScaleNA = new NativeArray<float3>(TotalCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        cubeVisibleRotNA = new NativeArray<quaternion>(TotalCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        cubeVisbleMatricesNA = new NativeArray<Matrix4x4>(TotalCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        
        visibleCountNA = new NativeArray<int>(1, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        #endregion
        
        
        #region Initialize Movement Job NA
        CubePosNA = new NativeArray<float3>(TotalCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        CubeScaleNA = new NativeArray<float3>(TotalCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        CubeRotNA = new NativeArray<quaternion>(TotalCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        
        CubeUniversalSpeedNA = new NativeArray<float>(TotalCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        CubeRotatingDirectionNA = new NativeArray<quaternion>(TotalCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        
        
        
        CubeMoveXDirNA = new NativeArray<float>(TotalCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        CubeMoveZDirNA = new NativeArray<float>(TotalCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        
        CubeMoveYDirNA = new NativeArray<float>(TotalCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        CubeMoveYQuadraticXNA = new NativeArray<float>(TotalCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        
        cubeColsNA = new NativeArray<Vector4>(TotalCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        cubeColsLerpT =  new NativeArray<float>(TotalCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        #endregion
    }

    private void OnDisable()
    {

        //Force complete all job systems before deallocation
        cubeCullingJobHandle.Complete();
        cubemovementJobHandle.Complete();
        
        
        
        //Deallocate all NA
        #region Dispose Culling Job NA
        cubeVisiblePosNA.Dispose();
        cubeVisibleScaleNA.Dispose();
        cubeVisibleRotNA.Dispose();
        cubeVisbleMatricesNA.Dispose();

        visibleCountNA.Dispose();
        #endregion

        
        #region Dispose Movement Job NA
        CubePosNA.Dispose();
        CubeScaleNA.Dispose();
        CubeRotNA.Dispose();

        CubeUniversalSpeedNA.Dispose();
        CubeRotatingDirectionNA.Dispose();
        
        CubeMoveXDirNA.Dispose();
        CubeMoveZDirNA.Dispose();
        CubeMoveYDirNA.Dispose();
        
        CubeMoveYQuadraticXNA.Dispose();
        
        cubeColsNA.Dispose();
        cubeColsLerpT.Dispose();
        #endregion





    }


    private void Awake()
    {
        //Initialize Array val
        CubeMatrices = new Matrix4x4[TotalCount];
        CubeCols = new Vector4[TotalCount];

        for (var i = 0; i < TotalCount; i++)
        { 
            CubeCols[i] = startCol;
        }

        
        matPropsBlock = new MaterialPropertyBlock();
    }



    private void LateUpdate()
    {
        
        DeltaTime = Time.deltaTime;
        
        
        
        //Complete Job systems
        cubeCullingJobHandle.Complete();
        cubemovementJobHandle.Complete();

    
        //Return values from NA
        cubeVisbleMatricesNA.CopyTo(CubeMatrices);
        visibleCount = visibleCountNA[0];
        
        cubeColsNA.CopyTo(CubeCols);
        cubeColsLerpT.CopyTo(cubeColsLerpT);
        CubeMoveYDirNA.CopyTo(CubeMoveYDirNA);

        matPropsBlock.SetVectorArray(_ColorsINT, CubeCols);

  
        Graphics.DrawMeshInstanced(cubeMesh, 0, cubeMat, CubeMatrices, visibleCount, matPropsBlock, ShadowCastingMode.On, false);






        //Copy values to NA
        cubeVisiblePosNA.CopyFrom(CubePosNA);
        cubeVisibleScaleNA.CopyFrom(CubeScaleNA);
        cubeVisibleRotNA.CopyFrom(CubeRotNA);
        CubeMoveYDirNA.CopyFrom(CubeMoveYDirNA);
        cubeColsLerpT.CopyFrom(cubeColsLerpT);
        
        #region Job Handle Initializations
        //Culling Job Initializations
        cubeCullingJobHandle = new CubeCullingJobSystem
        {
            TotalCountINT = TotalCount,
            cubeVisiblePosNA = cubeVisiblePosNA,
            cubeVisibleScaleNA = cubeVisibleScaleNA,
            cubeVisibleRotNA = cubeVisibleRotNA,
            cubeVisbleMatricesNA = cubeVisbleMatricesNA,
            visibleCountNA = visibleCountNA,
            
            cubeColsNA = cubeColsNA,
            startCol = startCol,
            endCol = endCol,
            cubeColsLerpT = cubeColsLerpT,
            DeltaTime = DeltaTime,
            CubeUniversalSpeedNA = CubeUniversalSpeedNA,
            
        }.Schedule();

        
        //Movement Job Initializations
        cubemovementJobHandle = new CubeMovementJobSystem
        {
            DeltaTime = DeltaTime,
            CubePosNA = CubePosNA,
            CubeScaleNA = CubeScaleNA,
            CubeRotNA = CubeRotNA,
            
            CubeRotatingDirectionNA = CubeRotatingDirectionNA,
            CubeUniversalSpeedNA = CubeUniversalSpeedNA,
            CubeMoveXDirNA = CubeMoveXDirNA,
            CubeMoveZDirNA = CubeMoveZDirNA,
            CubeMoveYDirNA = CubeMoveYDirNA,
            
            CubeMoveYQuadraticXNA = CubeMoveYQuadraticXNA,
            CubeYBounceYesNA = bounceYes,

        }.Schedule(TotalCount, 256);
        #endregion
    }





    
    //Use this method to generate particles
    public void ActivateDMIParticles(int noOfParticlesToActivate)
    {
        CubeCurrentIndexMax += noOfParticlesToActivate;

        //Reset to 0 if exceed max count
        if (CubeCurrentIndexMax > TotalCount)
        {
            CubeCurrentIndexMin = 0;
            CubeCurrentIndexMax = noOfParticlesToActivate;
        }

        
        
        //Complete Job systems
        cubeCullingJobHandle.Complete();
        cubemovementJobHandle.Complete();

        
        //Assign new values
        for (var i = CubeCurrentIndexMin; i < CubeCurrentIndexMax; i++)
        {
            CubePosNA[i] = 0;
            CubeScaleNA[i] = 1;
            CubeRotNA[i] = Random.rotation;

            
            CubeRotatingDirectionNA[i] = Random.rotation;
            CubeUniversalSpeedNA[i] = Random.Range(cubeUniversalSpeedMin, cubeUniversalSpeedMax);
            
            CubeMoveXDirNA[i] = Random.Range(cubeMoveXDirMin, cubeMoveXDirMax);
            CubeMoveZDirNA[i] = Random.Range(cubeMoveZDirMin, cubeMoveZDirMax);
            CubeMoveYDirNA[i] = cubeMoveYDir;

            CubeMoveYQuadraticXNA[i] = 0;

            cubeColsNA[i] = startCol;
            cubeColsLerpT[i] = 0;
        }


        CubeCurrentIndexMin = CubeCurrentIndexMax;
    }
    
    
    
    
    
    #region Job Systems
    //Culling Job
    [BurstCompile]
    private struct CubeCullingJobSystem : IJob
    {
        [ReadOnly] public int TotalCountINT;

        //Basic components
        [ReadOnly] public NativeArray<float3> cubeVisibleScaleNA;
        [ReadOnly] public NativeArray<float3> cubeVisiblePosNA;
        [ReadOnly] public NativeArray<quaternion> cubeVisibleRotNA;
        [WriteOnly] public NativeArray<Matrix4x4> cubeVisbleMatricesNA;


        //Colors
        public NativeArray<Vector4> cubeColsNA;
        [ReadOnly] public Color startCol;
        [ReadOnly] public Color endCol;
        public NativeArray<float> cubeColsLerpT;
        [ReadOnly] public float DeltaTime;
        //Universal variables
        [ReadOnly] public NativeArray<float> CubeUniversalSpeedNA;


        //Visibility Counters
        [WriteOnly] public NativeArray<int> visibleCountNA;
        private int visibleCountINT;
        
        
        public void Execute()
        {
            visibleCountINT = 0;
            for (var i = 0; i < TotalCountINT; i++)
            {
                if (cubeVisibleScaleNA[i].x <= 0)
                    continue;


                //(scale > 0) --> (visible = true)
                cubeVisbleMatricesNA[visibleCountINT] = Matrix4x4.TRS(cubeVisiblePosNA[i], cubeVisibleRotNA[i], cubeVisibleScaleNA[i]);


                //Colors over time
                cubeColsLerpT[i] = math.clamp(cubeColsLerpT[i] + DeltaTime * CubeUniversalSpeedNA[i] , 0f, 1f);
                cubeColsNA[visibleCountINT] = Color.Lerp(startCol, endCol, cubeColsLerpT[i]);
                
                //Convert gamma to linear space
                cubeColsNA[visibleCountINT] = math.pow(cubeColsNA[visibleCountINT], 2.2f);
                
                
                visibleCountINT++;
            }

            //Make sure visible count does not exceed maximum possible
            visibleCountNA[0] = math.clamp(visibleCountINT, 0, TotalCountINT);
        }
    }
    
    
    
    
    //Movement Job
    [BurstCompile]
    private struct CubeMovementJobSystem : IJobParallelFor
    {
        [ReadOnly] public float DeltaTime;
        
        
        //Basic components
        public NativeArray<float3> CubePosNA;
        public NativeArray<float3> CubeScaleNA;
        public NativeArray<quaternion> CubeRotNA;

        
        //Rotation variables
        [ReadOnly] public NativeArray<quaternion> CubeRotatingDirectionNA;

        //Position variables
        private float3 tempPos;
        [ReadOnly] public NativeArray<float> CubeMoveXDirNA;
        [ReadOnly] public NativeArray<float> CubeMoveZDirNA;
        public NativeArray<float> CubeMoveYDirNA;
        public NativeArray<float> CubeMoveYQuadraticXNA;
        [ReadOnly] public bool CubeYBounceYesNA;

        //Universal variables
        [ReadOnly] public NativeArray<float> CubeUniversalSpeedNA;



        public void Execute(int index)
        {
            if (CubeScaleNA[index].x <= 0)
                return;
            
            
            //Scale reduction
            //Reduction to 0 slowed down if bounceyes = true
            CubeScaleNA[index] = math.clamp(CubeScaleNA[index] - DeltaTime * CubeUniversalSpeedNA[index] / (CubeYBounceYesNA ? 3f : 1f), 0f, 1f);

            //Rotation over time
            CubeRotNA[index] = math.normalize(
                math.slerp(CubeRotNA[index], 
                   math.mul(CubeRotNA[index], CubeRotatingDirectionNA[index]), CubeUniversalSpeedNA[index] * DeltaTime)
            );
            
            
            
            //Position over time
            tempPos = CubePosNA[index];

            CubeMoveYQuadraticXNA[index] =
                math.clamp(CubeMoveYQuadraticXNA[index] + DeltaTime * CubeUniversalSpeedNA[index], 0f, 1f);
            
            //Bounce by reducing Y amplitude
            if (CubeMoveYQuadraticXNA[index] >= 1f)
            {
                CubeMoveYQuadraticXNA[index] = 0;
                CubeMoveYDirNA[index] = math.clamp(CubeMoveYDirNA[index] / 2f, 0f, 20f);
            }
            
            //Y quadratic curve movement
            tempPos.y = -CubeMoveYDirNA[index] * math.pow(CubeMoveYQuadraticXNA[index], 2f) + CubeMoveYDirNA[index] * CubeMoveYQuadraticXNA[index];


            //X, Z linear movmement
            tempPos.x += CubeMoveXDirNA[index] * CubeUniversalSpeedNA[index] * DeltaTime;
            tempPos.z += CubeMoveZDirNA[index] * CubeUniversalSpeedNA[index] * DeltaTime;
            
          
            CubePosNA[index] = tempPos;
        }
    }
    
    
    #endregion
}
