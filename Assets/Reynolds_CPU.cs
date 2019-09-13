/*
 * Update()内で繰り返し計算をするメソッドを選択する
 * アンコメントの場合はCalcu_PerFrame_Optimized()
 * コメントアウトの場合はCalcu_PerFrame()
 * Calcu_PerFrame_Optimized()はほんの少し早い
 */
#define USE_OPTIMIZED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 3次元レイノルズ方程式を計算するクラス
/// </summary>
public class Reynolds_CPU : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Toggle m_Toggle = null;


    private const uint meshX = 500, meshZ = 500;

    private ReynoldsFunc.Mesh mesh = null;


    // Use this for initialization
    void Start()
    {
        mesh = new ReynoldsFunc.Mesh(meshX, meshZ, 0.001f, 0.001f);
        mesh.CreateHeightArray(0.00003, meshZ / 2);
        mesh.CreateDeltaXYArray();

#if USE_OPTIMIZED
        mesh.Initialize_Calcu_PerFrame_Optimized();
#else
        mesh.Initialize_Calcu_PerFrame();
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Toggle.isOn)
        {

#if USE_OPTIMIZED
            mesh.Calcu_PerFrame_Optimized();
#else
        mesh.Calcu_PerFrame();
#endif
        }
    }
}
