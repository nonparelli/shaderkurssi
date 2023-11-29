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
    [SerializeField] private Color dcolor;

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
    private static readonly int State1Raw = Shader.PropertyToID("State1Raw");
    private static readonly int State2Raw= Shader.PropertyToID("State2Raw");
    private static readonly int CellColour = Shader.PropertyToID("CellColour");
    private static readonly int DeadColour = Shader.PropertyToID("DeadColour");
    private static readonly int LivingSum = Shader.PropertyToID("Living");
    private static readonly int DeadSum = Shader.PropertyToID("Dead");

    private RenderTexture State1;
    private RenderTexture State2;
    private RenderTexture RState1;
    private RenderTexture RState2;
    
    private bool UpdateState = false;

    // Start is called before the first frame update
    void Start()
    {
        State1 = new RenderTexture(TexSize.x, TexSize.y, 0, DefaultFormat.LDR)
        {
            enableRandomWrite = true
        };
        State1.Create();
        State1.filterMode = FilterMode.Point;

        State2 = new RenderTexture(TexSize.x, TexSize.y, 0, DefaultFormat.LDR)
        {
            enableRandomWrite = true
        };
        State2.Create();
        State2.filterMode = FilterMode.Point;
        
        RState1 = new RenderTexture(TexSize.x, TexSize.y, 0, DefaultFormat.LDR)
        {
            enableRandomWrite = true
        };
        RState1.Create();
        RState1.filterMode = FilterMode.Point;

        RState2 = new RenderTexture(TexSize.x, TexSize.y, 0, DefaultFormat.LDR)
        {
            enableRandomWrite = true
        };
        RState2.Create();
        RState2.filterMode = FilterMode.Point;
        
        GoLMaterial.SetTexture(BaseMap, State1);
        //GoLMaterial.SetTexture(State2Tex,State2);

        Update1Kernel = GoLShader.FindKernel("Update1");
        Update2Kernel = GoLShader.FindKernel("Update2");

        // switch (Seed)
        // {
        //     case GameSeed.FullTexture:
        //         seedKernel = GoLShader.FindKernel("InitFullTexture");
        //         break;
        //     case GameSeed.RPentomino:
        //         seedKernel = GoLShader.FindKernel("InitRPentomino");
        //         break;
        //     case GameSeed.Acorn:
        //         seedKernel = GoLShader.FindKernel("InitAcorn");
        //         break;
        //     case GameSeed.GosperGun: 
        //         seedKernel = GoLShader.FindKernel("InitGun");
        //         break;
        // }
        seedKernel = Seed switch
        {
            GameSeed.FullTexture => GoLShader.FindKernel("InitFullTexture"),
            GameSeed.RPentomino => GoLShader.FindKernel("InitRPentomino"),
            GameSeed.Acorn => GoLShader.FindKernel("InitAcorn"),
            GameSeed.GosperGun => GoLShader.FindKernel("InitGun"),
            _ => GoLShader.FindKernel("InitFullTexture"),
        };

        GoLShader.SetTexture(Update1Kernel, State1Tex, State1);
        GoLShader.SetTexture(Update1Kernel, State2Tex, State2);
        GoLShader.SetTexture(Update1Kernel, State1Raw, RState1);
        GoLShader.SetTexture(Update1Kernel, State2Raw, RState2);
        
        GoLShader.SetTexture(Update2Kernel, State1Tex, State1);
        GoLShader.SetTexture(Update2Kernel, State2Tex, State2);
        GoLShader.SetTexture(Update2Kernel, State1Raw, RState1);
        GoLShader.SetTexture(Update2Kernel, State2Raw, RState2);
        
        GoLShader.SetTexture(seedKernel, State1Tex, State1);
        GoLShader.SetTexture(seedKernel, State1Raw, RState1);

        //colours
        GoLShader.SetVector(CellColour, new float4(color.r, color.g, color.b, color.a));
        GoLShader.SetVector(DeadColour, new float4(dcolor.r, dcolor.g, dcolor.b, dcolor.a));
        GoLShader.SetFloat(LivingSum, color.r + color.g + color.b + color.a);
        GoLShader.SetFloat(DeadSum, dcolor.r + dcolor.g + dcolor.b + dcolor.a);
        GoLShader.SetVector(TextureSize, new Vector4(TexSize.x, TexSize.y));

        GoLShader.Dispatch(seedKernel, TexSize.x / 8, TexSize.y / 8, 1);
        Debug.Log("State 1 in use!");
    }

    void DispatchCells(int kernel)
    {
        GoLShader.SetVector(CellColour, new float4(color.r, color.g, color.b, color.a));
        GoLShader.SetVector(DeadColour, new float4(dcolor.r, dcolor.g, dcolor.b, dcolor.a));
        GoLShader.SetFloat(LivingSum, color.r + color.g + color.b + color.a);
        GoLShader.SetFloat(DeadSum, dcolor.r + dcolor.g + dcolor.b + dcolor.a);
        GoLShader.Dispatch(kernel, TexSize.x / 8, TexSize.y / 8, 1);
        UpdateState = !UpdateState;
    }

    // Update is called once per frame
    void Update()
    {
        timeAccu += Time.deltaTime;
        if (timeAccu > simSpeed)
        {
            if (UpdateState)
            {
                DispatchCells(Update1Kernel);
                GoLMaterial.SetTexture(BaseMap, State1);
            }
            else
            {
                DispatchCells(Update2Kernel);
                GoLMaterial.SetTexture(BaseMap, State2);
            }

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