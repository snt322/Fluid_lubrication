using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FLOAT
using MyVar = System.Single;
#else
using MyVar = System.Double;
#endif

public class Reynolds_CShader : MonoBehaviour
{
    [SerializeField]
    private ComputeShader m_ComputeShader = null;

    private ComputeBuffer m_CSInput = null;

    private ComputeBuffer m_AP = null;
    private ComputeBuffer m_AN = null;
    private ComputeBuffer m_AS = null;
    private ComputeBuffer m_AE = null;
    private ComputeBuffer m_AW = null;
    private ComputeBuffer m_SP = null;

    private ComputeBuffer m_CSOutput = null;
    private ComputeBuffer m_CSResudial = null;

    private int kernelNum = 0;

    [SerializeField]
    private string kernelName = "CS_Reynolds";

    
    /// <summary>
    /// ComputeShaderで起動するX Group数
    /// </summary>
    private uint m_GroupXCount = 1;
    /// <summary>
    /// ComputeShaderで起動するY Group数
    /// </summary>
    private uint m_GroupYCount = 1;
    /// <summary>
    /// ComputeShaderで起動するZ group数
    /// </summary>
    private uint m_GroupZCount = 1;

    private bool m_IsFinish = false;


    [SerializeField, Tooltip("ComputeShaderを使用可否のToggleスイッチをセットしてください")]
    private UnityEngine.UI.Toggle m_Toggle = null;

    /// <summary>
    /// 流体潤滑(三次元レイノルズ方程式)の計算に必要なオブジェクト
    /// </summary>
    private ReynoldsFunc.Mesh mesh = null;
    /// <summary>
    /// X成分の計算格子点数、void SetMeshCount()メソッドからセットする。
    /// 未設定の場合はmeshX = 500を初期値とする
    /// </summary>
    private uint meshX = 500;
    /// <summary>
    /// Z成分の計算格子点数、void SetMeshCount()メソッドっからセットする。
    /// 未設定の場合はmeshZ = 500を初期値とする
    /// </summary>
    private uint meshZ = 500;
    /// <summary>
    /// Y成分の格子点数は常に1とする
    /// </summary>
    private uint meshY = 1;

    /// <summary>
    /// ComputeShaderで繰り返し計算を行う毎に、
    /// m_ComputeShader.SetBuffer(kernelNum, "inPressure", m_CSInput)と
    /// m_ComputeSader.SetBuffer(kernelNum, "inPressure", m_CSOutput)を交互にセットする。
    /// </summary>
    private bool m_ExchangeBuffer = false;

    // Use this for initialization
    void Start()
    {
        //-------------------------------------------------------------------------------------
        this.SetMeshCount(500, 500);            //計算格子点数をセット
        this.CreateComputeBufferOfCoefArray();  //ComputeBufferを作成、要素は未初期化
        this.CalcGroupNum();                    //ComputeShaderで起動するGroup数を算出

        InitReynoldsMesh();                     //ReynoldsFunc.meshオブジェクトを作成する

        this.SetValueToComputeBufferFromReynoldsMesh(); //ReynoldsFunc.meshオブジェクトからComputeBufferに値をコピーする

        this.SetMeshCountToCShader();           //ComputeShaderに計算格子点数をセットする

        this.SetBufferToComputeShader();        //ComputeShaderにComputeBufferをセットする
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Toggle.isOn)
        {
            if (!m_IsFinish)
            {
                Debug.Log("Execute ComputeShader.");
                if(m_ExchangeBuffer)
                {
                    m_ComputeShader.SetBuffer(kernelNum, "inPressure", m_CSOutput);      //
                    m_ComputeShader.SetBuffer(kernelNum, "outPressure", m_CSInput);        //
                }
                else
                {
                    m_ComputeShader.SetBuffer(kernelNum, "inPressure", m_CSInput);        //
                    m_ComputeShader.SetBuffer(kernelNum, "outPressure", m_CSOutput);        //
                }

                m_ExchangeBuffer = !m_ExchangeBuffer;

                m_ComputeShader.Dispatch(kernelNum, (int)m_GroupXCount, (int)m_GroupYCount, (int)m_GroupZCount);
                Debug.Log("Excecuted ComputeShader.");

                float[] outData = new float[meshX * meshY * meshZ];
                m_CSOutput.GetData(outData);

                float maxPressure = 0.0f;
                foreach(float a in outData)
                {
                    maxPressure = (maxPressure < a) ? a : maxPressure;
                }



                Debug.Log("Max Preassure = " + maxPressure);

            }
        }

    }

    /// <summary>
    /// ComputeBufferの後始末
    /// </summary>
    private void OnDestroy()
    {
        m_CSInput.Release();
        m_CSOutput.Release();

        m_AP.Release();
        m_AN.Release();
        m_AS.Release();
        m_AE.Release();
        m_AW.Release();
        m_SP.Release();

        m_CSResudial.Release();
    }
    
    /// <summary>
    /// 計算格子点数をセットする。
    /// </summary>
    /// <param name="xc">X成分の計算格子点数</param>
    /// <param name="zc">Z成分の計算格子点数</param>
    private void SetMeshCount(uint xc, uint zc)
    {
        this.meshX = xc;
        this.meshZ = zc;
    }

    /// <summary>
    /// ComputeShader内で使用するComputeBufferを作成する
    /// 要素は未初期化
    /// </summary>
    void CreateComputeBufferOfCoefArray()
    {
        kernelNum = m_ComputeShader.FindKernel(kernelName);

        int bufferLen = (int)(this.meshX * this.meshZ * this.meshY);
        m_CSInput = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);

        m_AP = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_AN = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_AS = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_AE = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_AW = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_SP = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);

        m_CSOutput = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_CSResudial = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);

    }

    /// <summary>
    /// ComputeShaderで起動するGroup数を計算して、m_GroupXCount,m_GroupYCount,m_GroupZCountへ格納する
    /// </summary>
    void CalcGroupNum()
    {
        uint xn, yn, zn;
        m_ComputeShader.GetKernelThreadGroupSizes(kernelNum, out xn, out yn, out zn);

        Debug.Log("x =" + xn + " y = " + yn + " z = " + zn);

        m_GroupXCount = (meshX / xn) + (uint)((meshX % xn) != 0 ? 1 : 0);
        m_GroupYCount = (meshY / yn) + (uint)((meshY % yn) != 0 ? 1 : 0);
        m_GroupZCount = (meshZ / zn) + (uint)((meshZ % zn) != 0 ? 1 : 0);
    }

    /// <summary>
    /// 計算に使用する係数AP,AN,AS,AE,AW,SPに値をセットする
    /// m_CSInputは全ての要素を1で初期化する
    /// </summary>
    private void SetValueToComputeBufferFromReynoldsMesh()
    {
        MyVar[,] arrayAP = mesh.CoefAP;
        MyVar[,] arrayAN = mesh.CoefAN;
        MyVar[,] arrayAS = mesh.CoefAS;
        MyVar[,] arrayAE = mesh.CoefAE;
        MyVar[,] arrayAW = mesh.CoefAW;
        MyVar[,] arraySP = mesh.CoefSP;

        m_AP.SetData(arrayAP);
        m_AN.SetData(arrayAN);
        m_AS.SetData(arrayAS);
        m_AE.SetData(arrayAE);
        m_AW.SetData(arrayAW);
        m_SP.SetData(arraySP);

        int bufferLen = (int)(meshX * meshY * meshZ);
        float[] data = new float[bufferLen];
        for (int i = 0; i < bufferLen; i++)
        {
            data[i] = 1.0f;
        }

        m_CSInput.SetData(data);

    }

    /// <summary>
    /// ComputeShader内のグローバル変数(計算格子点数)に値をセットする
    /// ※注意!! 計算格子点数に誤りがある場合、RWStructuredBufferの要素外にアクセスしてしまうかも?
    /// </summary>
    private void SetMeshCountToCShader()
    {
        m_ComputeShader.SetInt("xLimit", (int)meshX);
        m_ComputeShader.SetInt("yLimit", (int)meshY);
        m_ComputeShader.SetInt("zLimit", (int)meshZ);
    }

    /// <summary>
    /// ComputeShaderにComputeBufferをセットする
    /// ※注意!! ComputeBufferがnullptrの場合の動作わ未確認。
    /// </summary>
    private void SetBufferToComputeShader()
    {
        m_ComputeShader.SetBuffer(kernelNum, "inPressure2D", m_CSInput);        //圧力を入力
        m_ComputeShader.SetBuffer(kernelNum, "AP", m_AP);                       //係数を入力
        m_ComputeShader.SetBuffer(kernelNum, "AN", m_AN);
        m_ComputeShader.SetBuffer(kernelNum, "AS", m_AS);
        m_ComputeShader.SetBuffer(kernelNum, "AE", m_AE);
        m_ComputeShader.SetBuffer(kernelNum, "AW", m_AW);
        m_ComputeShader.SetBuffer(kernelNum, "SP", m_SP);

        m_ComputeShader.SetBuffer(kernelNum, "outPressure2D", m_CSOutput);      //計算結果を入力するバッファをセット
        m_ComputeShader.SetBuffer(kernelNum, "outResudial2D", m_CSResudial);    //計算結果の残差を入力するバッファをセット
    }



    /// <summary>
    /// ReynoldsFunc.meshオブジェクトを作成する
    /// </summary>
    void InitReynoldsMesh()
    {
        mesh = new ReynoldsFunc.Mesh(meshX, meshZ, 0.001f, 0.001f);     //計算を行う格子を作成する
        mesh.CreateHeightArray(0.00003, meshZ / 2);
        mesh.CreateDeltaXYArray();

        mesh.Initialize_Calcu_PerFrame_Optimized();
    }
}
