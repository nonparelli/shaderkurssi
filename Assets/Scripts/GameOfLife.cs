using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

public class GameOfLife : MonoBehaviour
{
    [SerializeField, Range(0f, 5f)] private float simSpeed;
    [SerializeField] private float timeAccu;
    [SerializeField] private int2 TexSize;
    
    [SerializeField] private ComputeShader GoLShader;
    //[SerializeField] private Mesh PlaneMesh;
    [SerializeField] private Material GoLMaterial;
    [SerializeField] private Color color;

    [SerializeField]
    private enum GameSeed
    {
        FullTexture,
        RPentomino,
        Acorn,
        GosperGun
    }
    [SerializeField] private GameSeed Seed = GameSeed.FullTexture;

    private static int Update1Kernel;
    private static int Update2Kernel;
    private static int seedKernel;
    
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
    private static readonly int TextureSize = Shader.PropertyToID("TextureSize");
    private static readonly int State1Tex = Shader.PropertyToID("State1");
    private static readonly int State2Tex = Shader.PropertyToID("State2");
    private static readonly int State1RTex = Shader.PropertyToID("State1Read");
    private static readonly int State2RTex = Shader.PropertyToID("State2Read");
    private static readonly int CellColour = Shader.PropertyToID("CellColour");
    private static readonly int DeadColour = Shader.PropertyToID("DeadColour");

    private RenderTexture State1;
    private RenderTexture State2;
    
    private bool[,] Status1;
    private bool[,] Status2;

    private ComputeBuffer Buffer;
    private AsyncGPUReadbackRequest GPURequest;
    // Start is called before the first frame update
    void Start()
    {
        State1 = new RenderTexture(TexSize.x, TexSize.y, 0,DefaultFormat.LDR)
        {
            enableRandomWrite = true
        };
        State1.Create();
        State1.filterMode = FilterMode.Point;
        
        State2 = new RenderTexture(TexSize.x, TexSize.y, 0,DefaultFormat.LDR)
        {
            enableRandomWrite = true
        };
        State2.Create();
        State2.filterMode = FilterMode.Point;
        
        GoLMaterial.SetTexture(BaseMap,State1);
        //GoLMaterial.SetTexture(State2Tex,State2);

        Update1Kernel = GoLShader.FindKernel("Update1");
        Update2Kernel = GoLShader.FindKernel("Update2");
        
        seedKernel = Seed switch
        {
            GameSeed.FullTexture => GoLShader.FindKernel("InitFullTexture"),
            GameSeed.RPentomino => GoLShader.FindKernel("InitRPentomino"),
            GameSeed.Acorn => GoLShader.FindKernel("InitAcorn"),
            GameSeed.GosperGun => GoLShader.FindKernel("InitGun"),
            _ => GoLShader.FindKernel("InitFullTexture"),
        };
        seedKernel = GoLShader.FindKernel("InitFullTexture");

        GoLShader.SetTexture(Update1Kernel, State1Tex, State1);
        GoLShader.SetTexture(Update1Kernel, State2Tex, State2);
        GoLShader.SetTexture(Update1Kernel, State1RTex, State1);
        GoLShader.SetTexture(Update1Kernel, State2RTex, State2);
        
        GoLShader.SetTexture(Update2Kernel, State1Tex, State1);
        GoLShader.SetTexture(Update2Kernel, State2Tex, State2);
        GoLShader.SetTexture(Update2Kernel, State1RTex, State1);
        GoLShader.SetTexture(Update2Kernel, State2RTex, State2);
        
        GoLShader.SetTexture(seedKernel, State1Tex, State1);
        
        //colours
        GoLShader.SetVector(CellColour, new float4(color.r, color.g, color.b, color.a));
        GoLShader.SetVector(DeadColour, new float4(0, 0, 0, 1));
        GoLShader.SetVector(TextureSize, new Vector4(TexSize.x,TexSize.y));
        
        GoLShader.Dispatch(seedKernel,TexSize.x/8,TexSize.y/8,1);
        
    }

    void DispatchCells()
    {
        GoLShader.SetVector(CellColour,new float4(color.r, color.g, color.b, color.a));
        GoLShader.SetVector(DeadColour,new float4(0, 0, 0, 1));
        GoLShader.Dispatch(Update1Kernel,TexSize.x/8,TexSize.y/8,1);
    }
    // Update is called once per frame
    void Update()
    {
        timeAccu += Time.deltaTime;
        if (timeAccu > simSpeed)
        {
            // We read previous state into our buffer?
            for (int x = 0; x < TexSize.x; x++)
            {
                for (int y = 0; y < TexSize.y; y++)
                {
                }
            }
            //DispatchCells();

            timeAccu = 0f;
        }
    }
    private void OnDisable()
    {
        State1.Release();
        State2.Release();
    }

    private void OnDestroy()
    {
        State1.Release();
        State2.Release();
    }
}