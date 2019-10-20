/*
 * ComputeShder
 * 参考URL http://wordpress.notargs.com/blog/blog/2015/01/27/unity%E3%82%B3%E3%83%B3%E3%83%94%E3%83%A5%E3%83%BC%E3%83%88%E3%82%B7%E3%82%A7%E3%83%BC%E3%83%80%E3%81%A8%E3%82%A4%E3%83%B3%E3%82%B9%E3%82%BF%E3%83%B3%E3%82%B7%E3%83%B3%E3%82%B0%E3%81%A71%E4%B8%87/
 * クラスのサイズ
 * 参考URL https://araramistudio.jimdo.com/2018/09/21/c-%E3%81%A7%E5%9E%8B%E3%82%92%E5%88%A4%E5%88%A5%E3%81%99%E3%82%8Btypeof%E3%81%A8is%E6%BC%94%E7%AE%97%E5%AD%90/
 * 
 * ComputeBufferに渡すクラスのアライメントについてよくわからない
 * (c++/directX11の定数バッファの様に16バイト壁はない？  #pragma align(1)でパックしなくても大丈夫?)
 * 
 */


#define FLOAT

#define USE_STRUCTURED_CBUFFER

//#define CONSOLE_ThreadGroupSize

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

    /// <summary>
    /// ComputeShaderへ渡すバッファ、m_CSOutputと交互に入力と出力に切り替える
    /// </summary>
    private ComputeBuffer m_CSInput = null;
    /// <summary>
    /// ComputeShaderへ渡すバッファ、m_CSInputと交互に入力と出力に切り替える
    /// </summary>
    private ComputeBuffer m_CSOutput = null;

    private ComputeBuffer m_AP = null;
    private ComputeBuffer m_AN = null;
    private ComputeBuffer m_AS = null;
    private ComputeBuffer m_AE = null;
    private ComputeBuffer m_AW = null;
    private ComputeBuffer m_SP = null;

    private ComputeBuffer m_CSResudial = null;

    private int kernelNum = 0;

    //ComputeBuffer
    private ComputeBuffer m_Coef = null;



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

    /// <summary>
    /// 計算結果が収束したらtrueをセットする
    /// trueのセット部分は未実装
    /// </summary>
    private bool m_IsFinish = false;

    /// <summary>
    /// ComputeShaderの計算結果を格納したComputeBuffer m_CSInputまたはm_CSOutputに格納されるので
    /// </summary>
    private float[] m_CalcResult = null;


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

    public uint MeshXCount
    {
        private set { }
        get { return this.meshX; }
    }
    public uint MeshZCount
    {
        private set { }
        get { return this.meshZ; }
    }

    [SerializeField, Tooltip("X方向の格子点数"), Range(1, 256)]
    private uint MeshX;

    [SerializeField, Tooltip("Z方向の格子点数"), Range(1, 256)]
    private uint MeshZ;


    /// <summary>
    /// ComputeShaderで繰り返し計算を行う毎に、
    /// m_ComputeShader.SetBuffer(kernelNum, "inPressure", m_CSInput)と
    /// m_ComputeSader.SetBuffer(kernelNum, "inPressure", m_CSOutput)を交互にセットする。
    /// </summary>
    private bool m_ExchangeBuffer = false;

    // Use this for initialization
    void Awake()
    {
        //-------------------------------------------------------------------------------------
        this.SetMeshCount(MeshX, MeshZ);            //計算格子点数をセット
        InitReynoldsMesh();                     //ReynoldsFunc.meshオブジェクトを作成する
#if USE_STRUCTURED_CBUFFER
        this.CreateComputeBufferOfCoefStruct();
        this.SetValueToStructuredComputeBufferFromReynoldsMesh();
        SetStructuredBufferToComputeShader();
#else
        this.CreateComputeBufferOfCoefArray();  //ComputeBufferを作成、要素は未初期化
        this.SetValueToComputeBufferFromReynoldsMesh(); //ReynoldsFunc.meshオブジェクトからComputeBufferに値をコピーする
        this.SetBufferToComputeShader();        //ComputeShaderにComputeBufferをセットする
#endif
        this.CalcGroupNum();                    //ComputeShaderで起動するGroup数を算出
        this.CreateIOSBuffer();

        this.SetIIOutBufferToComputeShader();


        this.SetMeshCountToCShader();           //ComputeShaderに計算格子点数をセットする

    }

    // Update is called once per frame
    void Update()
    {
        if (m_Toggle.isOn)
        {
            if (!m_IsFinish)
            {
                Debug.Log("Execute ComputeShader.");
                if (m_ExchangeBuffer)
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


                //計算結果を取得して、最大値、最小値を取得する
                m_CSOutput.GetData(m_CalcResult);

                float max = float.MinValue;
                float min = float.MaxValue;

                for (int j = 1; j < (meshZ - 1); j++)
                {
                    for (int i = 1; i < (meshX - 1); i++)
                    {
                        int num = i + (int)meshX * j;
                        float a = m_CalcResult[num];
                        max = (max < a) ? a : max;
                        min = (min > a) ? a : min;
                    }
                }

                //ComputeShaderで1グループ内で起動するスレッド数を表示する
//                Debug.Log("x = " + m_GroupXCount + "  , y = " + m_GroupYCount + " , z = " + m_GroupZCount);


                Debug.Log("Pressure : max = " + max + " : min = " + min);

            }
        }

    }

    /// <summary>
    /// ComputeBufferの後始末
    /// </summary>
    private void OnDestroy()
    {
#if USE_STRUCTURED_CBUFFER
        ReleaseComputeBufferOfCoefStruct();
#else
        ReleaseComputeBufferOfCoefArray();
#endif
        ReleaseComputeBufferInOutResudial();
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

        m_AP = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_AN = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_AS = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_AE = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_AW = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_SP = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
    }
    /// <summary>
    /// ComputeShader内で使用する係数(m_AP,m_AN,m_AS,m_AE,m_AW,m_SP)を格納しているComputeBufferを解放する
    /// </summary>
    void ReleaseComputeBufferOfCoefArray()
    {
        m_AP.Release();
        m_AN.Release();
        m_AS.Release();
        m_AE.Release();
        m_AW.Release();
        m_SP.Release();
    }
    /// <summary>
    /// ComputeShader内で使用するComputeBufferのうち、ReleaseComputeBufferOfCoefArray()及びReleaseComputeBufferOfCoefStruct()の残りを解放する
    /// </summary>
    void ReleaseComputeBufferInOutResudial()
    {
        m_CSInput.Release();
        m_CSOutput.Release();
        m_CSResudial.Release();
    }

    void CreateIOSBuffer()
    {
        int bufferLen = (int)(this.meshX * this.meshZ * this.meshY);
        m_CSInput = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_CSOutput = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_CSResudial = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);

        m_CalcResult = new float[bufferLen];


        float[] data = new float[bufferLen];
        for (int i = 0; i < bufferLen; i++)
        {
            data[i] = 1.0f;
        }

        m_CSInput.SetData(data);
    }


    /// <summary>
    /// ComputeShaderのRWStructuredBufferに自前で定義した構造体を渡す
    /// </summary>
    void CreateComputeBufferOfCoefStruct()
    {
        kernelName = "CS_Reynolds_StructuredInput";
        kernelNum = m_ComputeShader.FindKernel(kernelName);

        int bufferLen = (int)(this.meshX * this.meshZ * this.meshY);

        int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MyComputeBufferStruct.BufferStruct));
        m_Coef = new ComputeBuffer(bufferLen, stride, ComputeBufferType.Default);
    }
    /// <summary>
    /// ComputeShader内で使用するComputeBuffer(structured)を作成する
    /// </summary>
    private void SetValueToStructuredComputeBufferFromReynoldsMesh()
    {
        MyVar[,] arrayAP = mesh.CoefAP;
        MyVar[,] arrayAN = mesh.CoefAN;
        MyVar[,] arrayAS = mesh.CoefAS;
        MyVar[,] arrayAE = mesh.CoefAE;
        MyVar[,] arrayAW = mesh.CoefAW;
        MyVar[,] arraySP = mesh.CoefSP;

        MyComputeBufferStruct.BufferStruct[] arryBuffer = new MyComputeBufferStruct.BufferStruct[arrayAP.GetLength(0) * arrayAP.GetLength(1)];

        Debug.Log("-------------2");
        float max = float.MinValue;
        float min = float.MaxValue;
        for (int j = 1; j < meshZ; j++)
        {
            for (int i = 1; i < meshX; i++)
            {
                int num = i + j * (int)meshX;
                //int num = i + j * arrayAP.GetLength(0);
                arryBuffer[num].AP = arrayAP[i, j];
                arryBuffer[num].AN = arrayAN[i, j];
                arryBuffer[num].AS = arrayAS[i, j];
                arryBuffer[num].AE = arrayAE[i, j];
                arryBuffer[num].AW = arrayAW[i, j];
                arryBuffer[num].SP = arraySP[i, j];

                float a = arryBuffer[num].AP;
                max = (max < a) ? a : max;
                min = (min > a) ? a : min;
            }
        }

        Debug.Log("structured reynoldsMesh_MAX = " + max + " : reynoldsMesh_MIN = " + min);
        Debug.Log("############2");

        m_Coef.SetData(arryBuffer);
    }
    /// <summary>
    /// ComputeShaderのRWStructuredBufferを解放する
    /// </summary>
    void ReleaseComputeBufferOfCoefStruct()
    {
        m_Coef.Release();
    }
    /// <summary>
    /// 
    /// </summary>
    void SetStructuredBufferToComputeShader()
    {
        m_ComputeShader.SetBuffer(kernelNum, "inputs", m_Coef);                   //圧力を入力
    }

    void SetIIOutBufferToComputeShader()
    {
        m_ComputeShader.SetBuffer(kernelNum, "inPressure", m_CSInput);        //圧力を入力
        m_ComputeShader.SetBuffer(kernelNum, "outPressure", m_CSOutput);      //計算結果を入力するバッファをセット
        m_ComputeShader.SetBuffer(kernelNum, "outResudial", m_CSResudial);    //計算結果の残差を入力するバッファをセット
    }

    /// <summary>
    /// ComputeShaderで起動するGroup数を計算して、m_GroupXCount,m_GroupYCount,m_GroupZCountへ格納する
    /// </summary>
    void CalcGroupNum()
    {
        uint xn, yn, zn;
        m_ComputeShader.GetKernelThreadGroupSizes(kernelNum, out xn, out yn, out zn);

#if CONSOLE_ThreadGroupSize
        Debug.Log("x =" + xn + " y = " + yn + " z = " + zn);
#endif
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

        MyVar[] arryAP = new MyVar[arrayAP.GetLength(0) * arrayAP.GetLength(1)];
        MyVar[] arryAN = new MyVar[arrayAN.GetLength(0) * arrayAN.GetLength(1)];
        MyVar[] arryAS = new MyVar[arrayAS.GetLength(0) * arrayAS.GetLength(1)];
        MyVar[] arryAE = new MyVar[arrayAE.GetLength(0) * arrayAE.GetLength(1)];
        MyVar[] arryAW = new MyVar[arrayAW.GetLength(0) * arrayAW.GetLength(1)];
        MyVar[] arrySP = new MyVar[arraySP.GetLength(0) * arraySP.GetLength(1)];

        Debug.Log("-------------");
        float max = float.MinValue;
        float min = float.MaxValue;
        for (int j = 1; j < meshZ; j++)
        {
            for (int i = 1; i < meshX; i++)
            {
                int num = i + j * (int)meshX;
//                int num = i + j * arrayAP.GetLength(0);
                arryAP[num] = arrayAP[i, j];
                arryAN[num] = arrayAN[i, j];
                arryAS[num] = arrayAS[i, j];
                arryAE[num] = arrayAE[i, j];
                arryAW[num] = arrayAW[i, j];
                arrySP[num] = arraySP[i, j];

                float a = arryAP[num];
                max = (max < a) ? a : max;
                min = (min > a) ? a : min;
            }
        }
        Debug.Log("meshX = " + (int)meshX + " : arrayAP.GetLength(0) = " + arrayAP.GetLength(0));
        Debug.Log("reynoldsMesh_MAX = " + max + " : reynoldsMesh_MIN = " + min);
        Debug.Log("############");

        m_AP.SetData(arryAP);
        m_AN.SetData(arryAN);
        m_AS.SetData(arryAS);
        m_AE.SetData(arryAE);
        m_AW.SetData(arryAW);
        m_SP.SetData(arrySP);


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
    /// ※注意!! ComputeBufferがnullptrの場合の動作は未確認。
    /// </summary>
    private void SetBufferToComputeShader()
    {
        m_ComputeShader.SetBuffer(kernelNum, "AP", m_AP);                       //係数を入力
        m_ComputeShader.SetBuffer(kernelNum, "AN", m_AN);
        m_ComputeShader.SetBuffer(kernelNum, "AS", m_AS);
        m_ComputeShader.SetBuffer(kernelNum, "AE", m_AE);
        m_ComputeShader.SetBuffer(kernelNum, "AW", m_AW);
        m_ComputeShader.SetBuffer(kernelNum, "SP", m_SP);
    }

    

    /// <summary>
    /// ReynoldsFunc.meshオブジェクトを作成する
    /// </summary>
    void InitReynoldsMesh()
    {
        mesh = new ReynoldsFunc.Mesh(meshX, meshZ, 0.001f, 0.001f);     //計算を行う格子を作成する
        mesh.CreateHeightArray(0.00003f, meshZ / 2);
        mesh.CreateDeltaXYArray();

        mesh.Initialize_Calcu_PerFrame_Optimized();
    }

    /// <summary>
    /// 格子点上の高さ配列のコピーを返す。1次要素X、2次要素Z
    /// </summary>
    public float[,] HeightArray
    {
        get { return this.mesh.HeightArray; }
    }

    /// <summary>
    /// 格子点のX座標配列のコピーを返す
    /// </summary>
    public float[] XPosArray
    {
        get { return this.mesh.XposArray; }
    }

    /// <summary>
    /// 格子点のZ座標配列のコピーを返す
    /// </summary>
    public float[] ZPosArray
    {
        get { return this.mesh.ZposArray; }
    }


    /// <summary>
    /// 計算領域のX方向長さを返す
    /// </summary>
    public MyVar CalAreaX
    {
        get { return mesh.CalAreaX; }
    }

    /// <summary>
    /// 計算領域のZ方向長さを返す
    /// </summary>
    public MyVar CalAreaZ
    {
        get { return mesh.CalAreaZ; }
    }

    /// <summary>
    /// 計算領域のY方向(高さ)の最大値を返す
    /// </summary>
    public MyVar CalAreaY
    {
        get { return mesh.MaxHeight; }
    }

}

namespace MyComputeBufferStruct
{
    struct BufferStruct
    {
        public float AP;
        public float AN;
        public float AS;
        public float AE;
        public float AW;
        public float SP;
    }
}
