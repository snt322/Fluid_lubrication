using System.Collections;
using System.Collections.Generic;
using PressureMesh;
using UnityEngine;

using UnityEngine.EventSystems;

public class PressureMeshController : MonoBehaviour, PressureMesh.ISendMessage
{
    [Header("レイノルズ方程式 (コンピュート・シェーダ)")]
    [SerializeField, Tooltip("レイノルズ方程式の計算を行うスクリプトをアタッチしてください")]
    private Reynolds_CShader m_RCShader = null;

    [SerializeField, Tooltip("本スクリプトが操作するMeshFilterを持つオブジェクトをセットしてください。")]
    private GameObject m_TargetMeshObj = null;

    /// <summary>
    /// 動的確保したMesh数を判別するカウンタ
    /// </summary>
    private int m_MeshCounter = 0;


    private void CreatePressureMesh()
    {
        //使用するコンポーネントを取得する--------------------------------------------
        //MeshFilterを取得する。アタッチされていない場合は追加する。
        MeshFilter meshFilter = m_TargetMeshObj.gameObject.GetComponent<MeshFilter>() as MeshFilter;
        if(meshFilter == null)
        {
            meshFilter = m_TargetMeshObj.gameObject.AddComponent<MeshFilter>() as MeshFilter;
        }

        //MeshColliderを取得する。アタッチされていない場合は追加する。
        MeshCollider meshCollider = m_TargetMeshObj.gameObject.GetComponent<MeshCollider>() as MeshCollider;
        if(meshCollider == null)
        {
            meshCollider = m_TargetMeshObj.gameObject.AddComponent<MeshCollider>() as MeshCollider;
        }

        //計算格子点数を取得する。
        int x = (int)m_RCShader.MeshXCount;                     //X方向格子点数
        int z = (int)m_RCShader.MeshZCount;                     //Z方向格子点数

        float[] xPos = (float[])m_RCShader.XPosArray;           //格子点座標のX方向座標を取得
        float[] zPos = (float[])m_RCShader.ZPosArray;           //格子点座標のZ方向座標を取得
        float[] yPressure = (float[])m_RCShader.Pressure;       //格子点座標のY方向座標(圧力値)を取得する。
                                                                //座標点(x,y)のY方向座標はyPressure[x,y]とする。

        float max = float.MinValue;
        float min = float.MaxValue;

        for (int j = 1; j < (z - 1); j++)
        {
            for (int i = 1; i < (x - 1); i++)
            {
                int num = i + (int)x * j;
                float a = yPressure[num];
                max = (max < a) ? a : max;
                min = (min > a) ? a : min;
            }
        }
        Debug.Log("max = " + max);
        Debug.Log("min = " + min);

        //頂点バッファの作成 ---------------------------------------------------------
        Vector3[] vertices = new Vector3[x * z];                //頂点座標の配列
        Color[] colors = new Color[x * z];                      //頂点色の配列
        Vector2[] uv = new Vector2[x * z];                      //頂点UV座標の配列

        for (int num = 0, j = 0; j < z; j++)                    //Z方向
        {
            for (int i = 0; i < x; i++)                         //X方向
            {
                num = i + j * x;                                                                                //2次元配列を1次元配列に変換する、番号
                colors[num] = new Color(1.0f, 0.0f, 1.0f);                                                      //頂点色
                vertices[num] = new Vector3(xPos[i], yPressure[num] / max, zPos[j]);                                 //頂点座標
                uv[num] = new Vector2(1.0f / (float)(x - 1) * (float)i, 1.0f / (float)(z - 1) * (float)j);      //頂点UV座標
            }
        }


        //インデックスバッファの作成 -------------------------------------------------
        int numIndices = 0;
        int[] indices = new int[2 * (x - 1) * z + 2 * x * (z - 1)];
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < (z - 1); j++)
            {
                indices[numIndices++] = i + j * x;
                indices[numIndices++] = i + (j + 1) * x;
            }
        }

        for (int j = 0; j < z; j++)
        {
            for (int i = 0; i < (x - 1); i++)
            {
                indices[numIndices++] = i + j * x;
                indices[numIndices++] = i + 1 + j * x;
            }
        }

/*
        //MeshCollider用の頂点バッファの作成 -----------------------------------------

        List<Vector3> inVertices = new List<Vector3>(4);
        inVertices.Add(new Vector3(0, 0, 0));
        inVertices.Add(new Vector3(xPos[x - 1], yPressure[x - 1, 0], zPos[0]));
        inVertices.Add(new Vector3(xPos[x - 1], yPressure[x - 1, z - 1], zPos[z - 1]));
        inVertices.Add(new Vector3(xPos[0], yPressure[0, z - 1], zPos[z - 1]));


        //MeshCollider用のインデックスバッファの作成 ----------------------------------
        int[] inIndices = new int[6];
        inIndices[0] = 0;
        inIndices[1] = 2;
        inIndices[2] = 1;
        inIndices[3] = 2;
        inIndices[4] = 3;
        inIndices[5] = 1;
*/

        //MeshFilter用の頂点バッファの作成 --------------------------------------------------------------------
        Mesh meshRederer = new Mesh();                          //新規作成
        meshRederer.name = "PressureMesh" + (m_MeshCounter++);  //Mesh名をセット

        //(Mesh)meshRenderに頂点情報をセットする
        meshRederer.vertices = vertices;                        //頂点座標をセット
        meshRederer.colors = colors;                            //頂点色をセット
        meshRederer.uv = uv;                                    //頂点UV座標をセット

        //MeshFilter用の頂点バッファをセット -----------------------------------------
        Mesh tmpMesh = meshFilter.mesh;
        

        if(tmpMesh != null)
        {
            Debug.Log("?????????????????????????????????");
            Mesh.Destroy(meshFilter.mesh);
        }

        meshFilter.mesh = meshRederer;                          //MeshFilter.meshに(Mesh)meshRenderをセットする。

        //MeshFilter用のインデックスバッファをセット ---------------------------------
        meshRederer.SetIndices(indices, MeshTopology.Lines, 0); //MeshFilter.meshに(int[])indicesをセットする


        Debug.Log("GetIndexCount = " + meshRederer.GetIndexCount(0));
        Debug.Log("x = " + x + " z = " + z + " total = " + (x * z));
        Debug.Log("numIndices = " + numIndices);


        /*
                //MeshCollider用の頂点バッファの作成 -------------------------------------------------------------------
                Mesh meshForCollider = new Mesh();
                meshForCollider.name = "PressureMesh_for_Collider";

                //(Mesh)meshForColliderに頂点情報をセットする
                meshForCollider.SetVertices(inVertices);                                //頂点バッファをセット
                meshForCollider.SetIndices(inIndices, MeshTopology.Triangles, 0);       //インデックスバッファをセット

                //MeshCollider用の頂点バッファをセット ---------------------------------------
                meshCollider.sharedMesh = meshForCollider;              //meshCollider.sharedMeshに(Mesh)をセットする。
        */

        Debug.Log("is OK ?::::::::::::::::::::::::::::::::::::::::::::::::::::");

    }


    void ISendMessage.CreateAndSetPressureMesh()
    {
        CreatePressureMesh();
    }

}


namespace PressureMesh
{
    public interface ISendMessage : IEventSystemHandler
    {
        void CreateAndSetPressureMesh();
    }
}