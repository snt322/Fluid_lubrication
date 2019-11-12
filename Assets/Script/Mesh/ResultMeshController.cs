using System.Collections;
using System.Collections.Generic;
using ResultMesh;
using UnityEngine;

using UnityEngine.EventSystems;


public class ResultMeshController : MonoBehaviour, ResultMesh.ISendMessage
{
    [SerializeField]
    private UnityEngine.MeshFilter m_MeshFilter = null;
    [SerializeField]
    private UnityEngine.MeshCollider m_MCollider = null;

    [Header("等高線図の表示設定")]
    [SerializeField, Tooltip("等高線図の原点ビルボードにアタッチしたTextBillBoardをセットしてください")]
    private GameObject m_OrigineBillBoard = null;
    [SerializeField, Tooltip("等高線図のX軸ビルボードにアタッチしたTextBillBoardをセットしてください")]
    private GameObject m_XaxisBillBoard = null;
    [SerializeField, Tooltip("等高線図のZ軸ビルボードにアタッチしたTextBillBoardをセットしてください")]
    private GameObject m_ZaxisBillBoard = null;
    [SerializeField, Tooltip("等高線図のY軸ビルボードにアタッチしたTextBillBoardをセットしてください")]
    private GameObject m_YaxisBillBoard = null;
    [Space(1)]

    [Header("等高線図の軸")]
    [SerializeField, Tooltip("X軸")]
    private UnityEngine.LineRenderer m_Xaxis = null;
    [SerializeField, Tooltip("Y軸")]
    private UnityEngine.LineRenderer m_Yaxis = null;
    [SerializeField, Tooltip("Z軸")]
    private UnityEngine.LineRenderer m_Zaxis = null;

    [Header("等高線の軸ラベル")]
    [SerializeField, Tooltip("")]
    private CreateLabels m_AxiLabel = null;
    [Space(1)]

    private MyMesh.Mesh m_Mesh;

    [Header("レイノルズ方程式 (コンピュート・シェーダ)")]
    [SerializeField, Tooltip("レイノルズ方程式の計算を行うスクリプトをアタッチしてください")]
    private Reynolds_CShader m_RCShader = null;
    [Space(1)]

    [Header("3Dグラフ表示の設定")]
    [SerializeField, Tooltip("X座標の表示幅"), Range(1, 10)]
    private float m_Xwidth = 1.0f;

    [SerializeField, Tooltip("Z座標の表示幅"), Range(1, 10)]
    private float m_Zwidth = 1.0f;

    [SerializeField, Tooltip("Y座標の表示幅"), Range(0.001f, 10)]
    private float m_Ywidth = 1.0f;

    [Header("メッシュの更新頻度(回/frame)")]
    [SerializeField, Tooltip("メッシュの更新レート(回/frame)"), Range(1, 1000)]
    private int m_MeshUpdatePerFrame = 1;




    private ResultMesh.enumMeshType m_MeshType = ResultMesh.enumMeshType.CalcuMesh;


    // Use this for initialization
    void Start()
    {
        SetPlaneMeshFromCShader();
        float Xwidth = m_RCShader.CalAreaX;
        float Zwidth = m_RCShader.CalAreaZ;
        float Ywidth = m_RCShader.CalAreaY;
        float magnitudeX = m_Xwidth / Xwidth;
        float magnitudeZ = m_Zwidth / Zwidth;
        float magnitudeY = m_Ywidth / Ywidth;

        this.m_MeshFilter.transform.localScale = new Vector3(magnitudeX, magnitudeY, magnitudeZ);

        MyOnValidate();

        Debug.Log("SetPlaneMesh()");
    }

    /// <summary>
    /// インスペクタ上で値が変更された場合
    /// </summary>
    private void OnValidate()
    {
        MyOnValidate();
    }

    private void MyOnValidate()
    {
        switch (m_MeshType)
        {
            case ResultMesh.enumMeshType.CalcuMesh:
                MyOnValidate_Calc();
                break;
            case ResultMesh.enumMeshType.PressureMesh:
                MyOnValidate_Pressure();
                break;
        }

    }

    /// <summary>
    /// 計算格子を表示するメソッド
    /// </summary>
    private void MyOnValidate_Calc()
    {
        try
        {
            float Xwidth = m_RCShader.CalAreaX;
            float Zwidth = m_RCShader.CalAreaZ;
            float Ywidth = m_RCShader.CalAreaY;
            float magnitudeX = m_Xwidth / Xwidth;
            float magnitudeZ = m_Zwidth / Zwidth;
            float magnitudeY = m_Ywidth / Ywidth;

            this.m_MeshFilter.transform.localScale = new Vector3(magnitudeX, magnitudeY, magnitudeZ);

            Vector3 originePos = new Vector3(0, 0, 0);
            Vector3 xPos = new Vector3(m_Xwidth, 0, 0);
            Vector3 zPos = new Vector3(0, 0, m_Zwidth);
            Vector3 yPos = new Vector3(0, m_Ywidth, 0);


            UnityEngine.EventSystems.ExecuteEvents.Execute<AxisLabel.IMessageSend>(m_OrigineBillBoard, null, (sender, eventData) => { sender.UpdateLabelPos(originePos); });
            UnityEngine.EventSystems.ExecuteEvents.Execute<AxisLabel.IMessageSend>(m_ZaxisBillBoard, null, (sender, eventData) => { sender.UpdateLabelPos(zPos); });
            UnityEngine.EventSystems.ExecuteEvents.Execute<AxisLabel.IMessageSend>(m_XaxisBillBoard, null, (sender, eventData) => { sender.UpdateLabelPos(xPos); });
            UnityEngine.EventSystems.ExecuteEvents.Execute<AxisLabel.IMessageSend>(m_YaxisBillBoard, null, (sender, eventData) => { sender.UpdateLabelPos(yPos); });

            m_Xaxis.SetPosition(0, new Vector3(0, 0, 0));
            m_Xaxis.SetPosition(1, xPos);

            m_Yaxis.SetPosition(0, new Vector3(0, 0, 0));
            m_Yaxis.SetPosition(1, yPos);

            m_Zaxis.SetPosition(0, new Vector3(0, 0, 0));
            m_Zaxis.SetPosition(1, zPos);

            UnityEngine.EventSystems.ExecuteEvents.Execute<IMessageCreateLabel>(m_AxiLabel.gameObject, null, (sender, eventData) => { sender.UpdateLabelPos(new Vector3(xPos.x, yPos.y, zPos.z)); });

            var MyAxisGridVect = new Vector3(m_Xwidth, m_Ywidth, m_Zwidth);
            UnityEngine.EventSystems.ExecuteEvents.Execute<MyAxisGrid.ISendMessage>(m_Xaxis.gameObject, null, (sender, eventData) => { sender.Update(5, MyAxisGridVect); });
            UnityEngine.EventSystems.ExecuteEvents.Execute<MyAxisGrid.ISendMessage>(m_Yaxis.gameObject, null, (sender, eventData) => { sender.Update(5, MyAxisGridVect); });
            UnityEngine.EventSystems.ExecuteEvents.Execute<MyAxisGrid.ISendMessage>(m_Zaxis.gameObject, null, (sender, eventData) => { sender.Update(5, MyAxisGridVect); });
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    /// <summary>
    /// 計算結果を表示するメソッド
    /// </summary>
    private void MyOnValidate_Pressure()
    {
        try
        {
            float Xwidth = m_RCShader.CalAreaX;
            float Zwidth = m_RCShader.CalAreaZ;
            float Ywidth = m_RCShader.MaxPressure;

            Debug.Log("Pressure = " + Ywidth + "+++++++++++++++++++++++" );

            float magnitudeX = m_Xwidth / Xwidth;
            float magnitudeZ = m_Zwidth / Zwidth;
            float magnitudeY = m_Ywidth / Ywidth;

            Debug.Log("m_Ywidth = " + m_Ywidth + "+++++++++++++++++++++++");
            Debug.Log("magnitudeY = " + magnitudeY + "+++++++++++++++++++++++");

            this.m_MeshFilter.transform.localScale = new Vector3(magnitudeX, magnitudeY, magnitudeZ);

            Vector3 originePos = new Vector3(0, 0, 0);
            Vector3 xPos = new Vector3(m_Xwidth, 0, 0);
            Vector3 zPos = new Vector3(0, 0, m_Zwidth);
            Vector3 yPos = new Vector3(0, m_Ywidth, 0);


            UnityEngine.EventSystems.ExecuteEvents.Execute<AxisLabel.IMessageSend>(m_OrigineBillBoard, null, (sender, eventData) => { sender.UpdateLabelPos(originePos); });
            UnityEngine.EventSystems.ExecuteEvents.Execute<AxisLabel.IMessageSend>(m_ZaxisBillBoard, null, (sender, eventData) => { sender.UpdateLabelPos(zPos); });
            UnityEngine.EventSystems.ExecuteEvents.Execute<AxisLabel.IMessageSend>(m_XaxisBillBoard, null, (sender, eventData) => { sender.UpdateLabelPos(xPos); });
            UnityEngine.EventSystems.ExecuteEvents.Execute<AxisLabel.IMessageSend>(m_YaxisBillBoard, null, (sender, eventData) => { sender.UpdateLabelPos(yPos); });

            m_Xaxis.SetPosition(0, new Vector3(0, 0, 0));
            m_Xaxis.SetPosition(1, xPos);

            m_Yaxis.SetPosition(0, new Vector3(0, 0, 0));
            m_Yaxis.SetPosition(1, yPos);

            m_Zaxis.SetPosition(0, new Vector3(0, 0, 0));
            m_Zaxis.SetPosition(1, zPos);

            UnityEngine.EventSystems.ExecuteEvents.Execute<IMessageCreateLabel>(m_AxiLabel.gameObject, null, (sender, eventData) => { sender.UpdateLabelPos(new Vector3(xPos.x, yPos.y, zPos.z)); });

            var MyAxisGridVect = new Vector3(m_Xwidth, m_Ywidth, m_Zwidth);
            UnityEngine.EventSystems.ExecuteEvents.Execute<MyAxisGrid.ISendMessage>(m_Xaxis.gameObject, null, (sender, eventData) => { sender.Update(5, MyAxisGridVect); });
            UnityEngine.EventSystems.ExecuteEvents.Execute<MyAxisGrid.ISendMessage>(m_Yaxis.gameObject, null, (sender, eventData) => { sender.Update(5, MyAxisGridVect); });
            UnityEngine.EventSystems.ExecuteEvents.Execute<MyAxisGrid.ISendMessage>(m_Zaxis.gameObject, null, (sender, eventData) => { sender.Update(5, MyAxisGridVect); });
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    private void SetPlaneMeshFromCShader()
    {
        //http://narudesign.com/devlog/unity-fbx-max-polygon/
        //ポリゴン数は256*256を超えないこと

        int meshXCount = (int)m_RCShader.MeshXCount;
        int meshZCount = (int)m_RCShader.MeshZCount;


        Mesh meshRederer = new Mesh();
        m_MeshFilter.mesh = meshRederer;

        meshRederer.name = "MyPlaneMesh";

        int x = (int)m_RCShader.MeshXCount;
        int z = (int)m_RCShader.MeshZCount;

        Debug.Log("x = " + x);
        Debug.Log("z = " + z);

        float[] xPos = (float[])m_RCShader.XPosArray;
        float[] zPos = (float[])m_RCShader.ZPosArray;
        float[,] height = (float[,])m_RCShader.HeightArray;

        Vector3[] vertices = new Vector3[x * z];
        Color[] colors = new Color[x * z];
        int num = 0;
        for (int j = 0; j < z; j++)
        {
            for (int i = 0; i < x; i++)
            {
                num = i + j * x;
                colors[num] = new Color(0.0f, 0.0f, 1.0f);
                vertices[num] = new Vector3(xPos[i], height[i, j], zPos[j]);
            }
        }

        meshRederer.vertices = vertices;
        //        mesh.colors = colors;



        int numIndices = 0;
        int[] indices = new int[2 * (x - 1) * z + 2 * x * (z - 1)];
        for(int i=0; i<x; i++)
        {
            for(int j=0; j<(z - 1); j++)
            {
                indices[numIndices++] = i + j * x;
                indices[numIndices++] = i + (j + 1) * x;
            }
        }

        for(int j=0; j<z; j++)
        {
            for(int i=0; i<(x-1); i++)
            {
                indices[numIndices++] = i + j * x;
                indices[numIndices++] = i + 1 + j * x;
            }
        }

/*
        int[] indices = new int[6 * (x - 1) * (z - 1)];
        for (int i = 0; i <= (x - 2); i++)
        {
            for (int j = 0; j <= (z - 2); j++)
            {
                indices[numIndices++] = (i) + (j) * x;
                indices[numIndices++] = (i + 1) + (j + 1) * x;
                indices[numIndices++] = (i + 1) + (j) * x;

                indices[numIndices++] = (i) + (j) * x;
                indices[numIndices++] = (i) + (j + 1) * x;
                indices[numIndices++] = (i + 1) + (j + 1) * x;

            }
        }
*/



        Debug.Log("x = " + x + " z = " + z + " total = " + (x * z));
        Debug.Log("numIndices = " + numIndices);

        meshRederer.SetIndices(indices, MeshTopology.Lines, 0);
//        meshRederer.SetIndices(indices, MeshTopology.LineStrip, 0);




        Vector2[] uv = new Vector2[x * z];

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < z; j++)
            {
                num = i + j * x;
                uv[num] = new Vector2(1.0f / (float)(x - 1) * (float)i, 1.0f / (float)(z - 1) * (float)j);
            }
        }

        meshRederer.uv = uv;

        Debug.Log("GetIndexCount = " + meshRederer.GetIndexCount(0));


        //---------------------------------------------------------------
        Mesh meshCollider = new Mesh();

        List<Vector3> inVertices = new List<Vector3>(4);
        inVertices.Add(new Vector3(0, 0, 0));
        inVertices.Add(new Vector3(xPos[x - 1], height[x - 1, 0], zPos[0]));
        inVertices.Add(new Vector3(xPos[x - 1], height[x - 1, z - 1], zPos[z - 1]));
        inVertices.Add(new Vector3(xPos[0], height[0, z - 1], zPos[z - 1]));

        meshCollider.SetVertices(inVertices);

        int[] inIndices = new int[6];
        inIndices[0] = 0;
        inIndices[1] = 2;
        inIndices[2] = 1;
        inIndices[3] = 2;
        inIndices[4] = 3;
        inIndices[5] = 1;

        meshCollider.SetIndices(inIndices, MeshTopology.Triangles, 0);

        m_MCollider.sharedMesh = meshCollider;
    }


    private void SetPlaneMesh()
    {
        //http://narudesign.com/devlog/unity-fbx-max-polygon/
        //ポリゴン数は256*256を超えないこと
        int meshXCount = 256;
        int meshZCount = 256;

        m_Mesh = new MyMesh.Mesh(meshXCount, meshZCount, 10.0f, 10.0f);
        Debug.Log("初期化の結果：" + m_Mesh.IsInitialized);

        Mesh mesh = new Mesh();
        m_MeshFilter.mesh = mesh;

        int x = m_Mesh.XmeshCount;
        int z = m_Mesh.ZmeshCount;

        Debug.Log("x = " + x);
        Debug.Log("z = " + z);

        float[] xPos = (float[])m_Mesh.Xmesh.Clone();
        float[] zPos = (float[])m_Mesh.Zmesh.Clone();
        float[] height = (float[])m_Mesh.Height.Clone();

        Vector3[] vertices = new Vector3[m_Mesh.XmeshCount * m_Mesh.ZmeshCount];
        Color[] colors = new Color[m_Mesh.XmeshCount * m_Mesh.ZmeshCount];
        int num = 0;
        for (int j = 0; j < z; j++)
        {
            for (int i = 0; i < x; i++)
            {
                num = i + j * x;
                colors[num] = new Color(0.0f, 0.0f, 1.0f);
                vertices[num] = new Vector3(xPos[i], height[num], zPos[j]);
            }
        }

        mesh.vertices = vertices;
        mesh.colors = colors;

        int numIndices = 0;

        int[] indices = new int[6 * (x - 1) * (z - 1)];
        for (int i = 0; i < (x - 1); i++)
        {
            for (int j = 0; j < (z - 1); j++)
            {
                indices[numIndices++] = (i) + (j) * x;
                indices[numIndices++] = (i + 1) + (j + 1) * x;
                indices[numIndices++] = (i + 1) + (j) * x;

                indices[numIndices++] = (i) + (j) * x;
                indices[numIndices++] = (i) + (j + 1) * x;
                indices[numIndices++] = (i + 1) + (j + 1) * x;

            }
        }

        Debug.Log("x = " + x + " z = " + z + " total = " + (x * z));
        Debug.Log("numIndices = " + numIndices);

        mesh.triangles = indices;

        Vector2[] uv = new Vector2[m_Mesh.XmeshCount * m_Mesh.ZmeshCount];

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < z; j++)
            {
                num = i + j * x;
                uv[num] = new Vector2(1.0f / (float)(x - 1) * (float)i, 1.0f / (float)(z - 1) * (float)j);
            }
        }

        //        Debug.Log();

        mesh.uv = uv;

        Debug.Log("GetIndexCount = " + mesh.GetIndexCount(0));

        m_MeshFilter.mesh = mesh;

        m_MCollider.sharedMesh = mesh;
    }

    /// <summary>
    /// 計算格子のメッシュを表示するメッセージを受信する
    /// </summary>
    void ISendMessage.ShowCalcMesh()
    {
        this.m_MeshType = enumMeshType.CalcuMesh;
        MyOnValidate();
    }

    /// <summary>
    /// 計算結果の圧力をメッシュとして表示するメッセ―ジを受信する
    /// </summary>
    void ISendMessage.ShowPressureMesh()
    {
        m_MeshType = enumMeshType.PressureMesh;
        MyOnValidate();
    }
}

namespace ResultMesh
{
    public enum enumMeshType
    {
        CalcuMesh,
        PressureMesh,
    }

    public interface ISendMessage : IEventSystemHandler
    {
        void ShowCalcMesh();
        void ShowPressureMesh();
    }
}


