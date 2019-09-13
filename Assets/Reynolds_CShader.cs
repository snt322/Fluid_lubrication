using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    private uint m_GridXCount = 100;
    private uint m_GridYCount = 100;
    private uint m_GridZCount = 1;

    /// <summary>
    /// X group数
    /// </summary>
    private uint m_GroupXCount;
    /// <summary>
    /// Y group数
    /// </summary>
    private uint m_GroupYCount;
    /// <summary>
    /// Z group数
    /// </summary>
    private uint m_GroupZCount;

    private bool m_IsFinish = false;


    [SerializeField, Tooltip("ComputeShaderを使用可否のToggleスイッチをセットしてください")]
    private UnityEngine.UI.Toggle m_Toggle = null;

    

    // Use this for initialization
    void Start()
    {
        kernelNum = m_ComputeShader.FindKernel(kernelName);



        int bufferLen = (int)(m_GridXCount * m_GridYCount * m_GridZCount);
        m_CSInput = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);

        m_AP = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_AN = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_AS = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_AE = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_AW = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_SP = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);

        m_CSOutput = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);
        m_CSResudial = new ComputeBuffer(bufferLen, sizeof(float), ComputeBufferType.Default);

        uint xn, yn, zn;
        m_ComputeShader.GetKernelThreadGroupSizes(kernelNum, out xn, out yn, out zn);

        Debug.Log("x =" + xn + " y = " + yn + " z = " + zn);

        m_GroupXCount = (m_GridXCount / xn) + (uint)((m_GridXCount % xn) != 0 ? 1 : 0);
        m_GroupYCount = (m_GridXCount / yn) + (uint)((m_GridYCount % yn) != 0 ? 1 : 0);
        m_GroupZCount = (m_GridXCount / zn) + (uint)((m_GridZCount % zn) != 0 ? 1 : 0);
        //-------------------------------------------------------------------------------------

        InitCoeff();

        //-------------------------------------------------------------------------------------
        m_ComputeShader.SetInt("xLimit", (int)m_GridXCount);
        m_ComputeShader.SetInt("yLimit", (int)m_GridYCount);
        m_ComputeShader.SetInt("zLimit", (int)m_GridZCount);

        m_ComputeShader.SetBuffer(kernelNum, "inPressure2D", m_CSInput);
        m_ComputeShader.SetBuffer(kernelNum, "AP", m_AP);
        m_ComputeShader.SetBuffer(kernelNum, "AN", m_AN);
        m_ComputeShader.SetBuffer(kernelNum, "AS", m_AS);
        m_ComputeShader.SetBuffer(kernelNum, "AE", m_AE);
        m_ComputeShader.SetBuffer(kernelNum, "AW", m_AW);
        m_ComputeShader.SetBuffer(kernelNum, "SP", m_SP);

        m_ComputeShader.SetBuffer(kernelNum, "outPressure2D", m_CSOutput);
        m_ComputeShader.SetBuffer(kernelNum, "outResudial2D", m_CSResudial);
        //------------------------------------------------------------------------------------

    }

    // Update is called once per frame
    void Update()
    {
        if (m_Toggle.isOn)
        {
            if (!m_IsFinish)
            {
                Debug.Log("CShader");
                m_ComputeShader.Dispatch(kernelNum, (int)m_GroupXCount, (int)m_GroupYCount, (int)m_GroupZCount);
            }
        }

    }

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

    void InitCoeff()
    {
        int bufferLen = (int)(m_GridXCount * m_GridYCount * m_GridZCount);

        float[] data = new float[bufferLen];
        for(int i=0; i<bufferLen; i++)
        {
            data[i] = 1.0f;
        }

        m_AP.SetData(data);
        m_AN.SetData(data);
        m_AS.SetData(data);
        m_AE.SetData(data);
        m_AW.SetData(data);
        m_SP.SetData(data);

        m_CSInput.SetData(data);
    }

    
}
