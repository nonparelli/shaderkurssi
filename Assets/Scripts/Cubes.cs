using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Cubes : MonoBehaviour
{
    [SerializeField, Range(0f,360f)] private float Angle;
    [SerializeField,Range(0f,40f)] private float Frequency;
    [SerializeField] private ComputeShader CubeShader;
    [SerializeField] private Mesh CubeMesh;
    [SerializeField] private Material CubeMaterial;

    private static int SimulationKernel;

    private static readonly int Direction = Shader.PropertyToID("Direction");
    private static readonly int Positions = Shader.PropertyToID("Positions");
    private static readonly int CurrentTime = Shader.PropertyToID("Time");
    private static readonly int Freq = Shader.PropertyToID("Frequency");
    private const int CubeAmount = 128 * 128;
    
    private Vector4[] CubePositions = new Vector4[CubeAmount];

    private Matrix4x4[] CubeMatrices = new Matrix4x4[CubeAmount];

    private ComputeBuffer CubeBuffer;

    private AsyncGPUReadbackRequest GPURequest;
    
    private void PopulateCubes(Vector4[] positions)
    {
        for (uint x = 0; x < 128; ++x)
        {
            for (uint y = 0; y < 128; ++y)
            {
                var idx = x * 128 + y;
                positions[idx] = new Vector4(x / 128f, 0, y / 128f);
            }
        }
        Debug.Log("Cubes populated!");
    }

    private void DispatchCubes()
    {
        Vector2 dir = new Vector2(Mathf.Cos(Mathf.Deg2Rad * Angle), Mathf.Sin(Mathf.Deg2Rad * Angle));
        CubeShader.SetFloat(CurrentTime, Time.time);
        CubeShader.SetFloat(Freq,Frequency);
        CubeShader.SetVector(Direction,dir);
        CubeShader.Dispatch(SimulationKernel,128/8,128/8,1);
    }
    void Start()
    {
        Debug.Log("Start!");
        SimulationKernel = CubeShader.FindKernel("Simulate");
        CubeBuffer = new ComputeBuffer(CubeAmount, 4 * sizeof(float));
        
        PopulateCubes(CubePositions);
        CubeBuffer.SetData(CubePositions);
        CubeShader.SetBuffer(SimulationKernel,Positions,CubeBuffer);

        DispatchCubes();
        
        GPURequest = AsyncGPUReadback.Request(CubeBuffer);
    }
    
    void Update()
    {
        if (GPURequest.done)
        {
            CubePositions = GPURequest.GetData<Vector4>().ToArray();


            for (int i = 0; i < CubeAmount; ++i)
                CubeMatrices[i] = Matrix4x4.TRS(
                    (Vector3)CubePositions[i] + transform.position,
                    Quaternion.identity,
                    Vector3.one * (1 / 128f)
                );
            GPURequest = AsyncGPUReadback.Request(CubeBuffer);
        }
        DispatchCubes();
        Graphics.DrawMeshInstanced(CubeMesh,0,CubeMaterial,CubeMatrices);
    }

    private void OnDisable()
    {
        CubeBuffer.Release();
    }

    private void OnDestroy()
    {
        CubeBuffer.Release();
        
    }
}
