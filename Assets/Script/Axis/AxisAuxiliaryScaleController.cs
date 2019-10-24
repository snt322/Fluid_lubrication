using System.Collections;
using System.Collections.Generic;
using MyAxisGrid;
using UnityEngine;

using UnityEngine.EventSystems;

/// <summary>
/// 軸目盛りのラベル、補助目盛を管理するスクリプト
/// </summary>
public class AxisAuxiliaryScaleController : MonoBehaviour, MyAxisGrid.ISendMessage
{
    [SerializeField]
    Shader m_Shader = null;


    /// <summary>
    /// AxisX,Y,Zの長さ、要素x,y,zがそれぞれ対応
    /// </summary>
    private Vector3 m_AxisMax = new Vector3(1, 1, 1);

    private List<GameObject> m_AxisLabels = null;

    private int m_ThisAxisDivideNum = 5;
    private float m_ThisAxisMaxValue = 1.0f;

    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialize()
    {
        string tag = this.tag;
        var myEnum = (MyAxisGrid.MyAxisEnum)System.Enum.Parse(typeof(MyAxisGrid.MyAxisEnum), tag);


        if (m_AxisLabels == null)
        {
            m_AxisLabels = new List<GameObject>(1);
        }

        foreach (GameObject g in m_AxisLabels)
        {
            if (g != null)
                GameObject.Destroy(g);
        }

        m_AxisLabels.Clear();

        for (int i = 0; i < m_ThisAxisDivideNum; i++)
        {
            GameObject g = new GameObject();
            g.name = string.Format("Auxiliary scale" + (i + 1));
            g.transform.SetParent(this.gameObject.transform);

            g.AddComponent<BillBoardToMainCamera>();


            var lRender = g.AddComponent<LineRenderer>();
            lRender.startWidth = 0.005f;
            lRender.endWidth = 0.005f;


            var txt = g.AddComponent<TextMesh>();
            txt.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            txt.fontSize = 40;
            txt.characterSize = 0.02f;
            var value = m_ThisAxisMaxValue / m_ThisAxisDivideNum;
            txt.text = string.Format("{0:F1}", value * (i + 1));

            float pos = 0;
            float x, y, z;
            x = y = z = 0;
            switch (myEnum)
            {
                case MyAxisGrid.MyAxisEnum.AxisX:
                    pos = m_AxisMax.x / m_ThisAxisDivideNum;
                    x = pos * (i + 1);
                    z = -AxisLabel.ConstantValue.offset.z;
                    txt.color = Color.red;

                    {
                        Vector3[] lRenderPos = new Vector3[2];
                        lRenderPos[0] = new Vector3(x, 0, 0);
                        lRenderPos[1] = new Vector3(x, 0, m_AxisMax.z);
                        lRender.positionCount = 2;
                        lRender.SetPositions(lRenderPos);
                    }
                    break;
                case MyAxisGrid.MyAxisEnum.AxisY:
                    pos = m_AxisMax.y / m_ThisAxisDivideNum;
                    x = -AxisLabel.ConstantValue.offset.x;
                    y = pos * (i + 1);
                    z = -AxisLabel.ConstantValue.offset.z;
                    txt.color = Color.green;
                    {
                        Vector3[] lRenderPos = new Vector3[3];
                        lRenderPos[0] = new Vector3(0, y, m_AxisMax.z);
                        lRenderPos[1] = new Vector3(0, y, 0);
                        lRenderPos[2] = new Vector3(m_AxisMax.x, y, 0);
                        lRender.positionCount = 3;
                        lRender.SetPositions(lRenderPos);
                    }
                    break;
                case MyAxisGrid.MyAxisEnum.AxisZ:
                    pos = m_AxisMax.z / m_ThisAxisDivideNum;
                    x = -AxisLabel.ConstantValue.offset.x;
                    z = pos * (i + 1);
                    txt.color = Color.blue;

                    {
                        Vector3[] lRenderPos = new Vector3[2];
                        lRenderPos[0] = new Vector3(0, 0, z);
                        lRenderPos[1] = new Vector3(m_AxisMax.z, 0, z);
                        lRender.positionCount = 2;
                        lRender.SetPositions(lRenderPos);
                    }
                    break;
            }
            g.transform.position = new Vector3(x, y, z);

            m_AxisLabels.Add(g);
        }



    }

    /// <summary>
    /// 再初期化
    /// </summary>
    private void ReInitialize()
    {
        Initialize();
    }
    
    /// <summary>
    /// 
    /// </summary>
    private void UpdateLabelPos()
    {
        string tag = this.tag;
        var myEnum = (MyAxisGrid.MyAxisEnum)System.Enum.Parse(typeof(MyAxisGrid.MyAxisEnum), tag);

        for (int i = 0; i < m_AxisLabels.Count; i++)
        {
            var lRender = m_AxisLabels[i].GetComponent<LineRenderer>() as LineRenderer;

            //マテリアルの作成
            //https://docs.unity3d.com/ja/2017.4/ScriptReference/Material.html
            var tmpMaterial = new Material(m_Shader);
            tmpMaterial.color = Color.white;
            m_AxisLabels[i].GetComponent<Renderer>().material = tmpMaterial;

            float pos = 0;
            float x, y, z;
            x = y = z = 0;
            switch (myEnum)
            {
                case MyAxisGrid.MyAxisEnum.AxisX:
                    pos = m_AxisMax.x / m_ThisAxisDivideNum;
                    x = pos * (i + 1);
                    z = -AxisLabel.ConstantValue.offset.z;

                    {
                        Vector3[] lRenderPos = new Vector3[2];
                        lRenderPos[0] = new Vector3(x, 0, 0);
                        lRenderPos[1] = new Vector3(x, 0, m_AxisMax.z);
                        lRender.positionCount = 2;
                        lRender.SetPositions(lRenderPos);
                    }
                    break;
                case MyAxisGrid.MyAxisEnum.AxisY:
                    pos = m_AxisMax.y / m_ThisAxisDivideNum;
                    x = -AxisLabel.ConstantValue.offset.x;
                    y = pos * (i + 1);
                    z = -AxisLabel.ConstantValue.offset.z;

                    {
                        Vector3[] lRenderPos = new Vector3[3];
                        lRenderPos[0] = new Vector3(0, y, m_AxisMax.z);
                        lRenderPos[1] = new Vector3(0, y, 0);
                        lRenderPos[2] = new Vector3(m_AxisMax.x, y, 0);
                        lRender.positionCount = 3;
                        lRender.SetPositions(lRenderPos);
                    }
                    break;
                case MyAxisGrid.MyAxisEnum.AxisZ:
                    pos = m_AxisMax.z / m_ThisAxisDivideNum;
                    x = -AxisLabel.ConstantValue.offset.x;
                    z = pos * (i + 1);

                    {
                        Vector3[] lRenderPos = new Vector3[2];
                        lRenderPos[0] = new Vector3(0, 0, z);
                        lRenderPos[1] = new Vector3(m_AxisMax.x, 0, z);
                        lRender.positionCount = 2;
                        lRender.SetPositions(lRenderPos);
                    }
                    break;
            }
            m_AxisLabels[i].transform.position = new Vector3(x, y, z);
        }

    }


    void ISendMessage.Update(int divide, Vector3 vect)
    {
        m_ThisAxisDivideNum = divide;
        m_AxisMax = vect;

        UpdateLabelPos();
    }


}




namespace MyAxisGrid
{
    enum MyAxisEnum
    {
        AxisX,
        AxisY,
        AxisZ,
    }

    public interface ISendMessage : IEventSystemHandler
    {
        /// <summary>
        /// 軸目盛りの更新メッセージの発行、引数maxValueとdivideからi番目の目盛りラベルの表示値を決定する。
        /// </summary>
        /// <param name="divide">目盛りの分割数</param>
        /// <param name="maxValue">目盛りの最大値</param>
        void Update(int divide, Vector3 maxValue);

    }
}
