using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyMesh
{
    public class Mesh
    {
        private int m_MeshX;
        private int m_MeshZ;

        private float m_Xwidth;
        private float m_Zwidth;


        private float[] m_Height;
        private float[] m_XmeshPos;
        private float[] m_ZmeshPos;

        private bool m_IsMeshCountSetted = false;
        private bool m_IsWidthSetted = false;
        private bool m_IsMeshCreated = false;

        private bool m_IsInitialized = false;

        private Mesh()
        {
            TempInitializeMeshCount();
        }
        public Mesh(int x, int z,  float xWidth,  float zWidth)
        {
            if(CheckWidth(xWidth, zWidth))
            {
                m_Xwidth = xWidth;
                m_Zwidth = zWidth;

                m_IsWidthSetted = true;
            }
            else
            {
                TempInitializeWidth();
            }

            if(CheckMeshCount(x,z))
            {
                m_MeshX = x;
                m_MeshZ = z;

                m_IsMeshCountSetted = true;
            }
            else
            {
                TempInitializeMeshCount();
                m_IsMeshCountSetted = false;
            }

            if(m_IsMeshCountSetted && m_IsWidthSetted)
            {
                m_IsMeshCreated = CreateMesh() ? true : false;
            }
            else
            {
                m_Height = null;
                m_XmeshPos = null;
                m_ZmeshPos = null;
            }

            if(m_IsMeshCreated && m_IsWidthSetted && m_IsMeshCountSetted)
            {
                m_IsInitialized = SetValueToMesh() ? true : false;
            }
        }

        private bool CheckWidth(float xwidth, float zwidth)
        {
            bool isOk = false;

            if(xwidth <= 0.0f || zwidth <= 0.0f)
            {
                isOk = false;
            }
            else
            {
                isOk = true;
            }

            return isOk;
        }

        private void TempInitializeWidth()
        {
            m_Xwidth = 1.0f;
            m_Zwidth = 1.0f;
        }

        /// <summary>
        /// 格子点数を仮値で初期化
        /// </summary>
        private void TempInitializeMeshCount()
        {
            m_MeshX = m_MeshZ = 1;
        }

        /// <summary>
        /// 格子点数を初期化する引数が正しいかチェック
        /// </summary>
        /// <param name="x">格子のx方向要素数</param>
        /// <param name="z">格子のz方向要素数</param>
        /// <returns></returns>
        private bool CheckMeshCount(int x, int z)
        {
            bool isInitialized = false;
            if(x > 1 && z > 1)
            {
                isInitialized = true;
            }
            return isInitialized;
        }

        public float[] Xmesh
        {
            private set { m_XmeshPos = value; }
            get { return m_XmeshPos; }
        }

        public float[] Zmesh
        {
            private set { m_ZmeshPos = value; }
            get { return m_ZmeshPos; }
        }

        public float[] Height
        {
            private set { m_Height = value; }
            get { return m_Height; }
        }

        public int XmeshCount
        {
            get { return m_MeshX; }
        }
        public int ZmeshCount
        {
            get { return m_MeshZ; }
        }


        public bool IsInitialized
        {
            private set {; }
            get { return m_IsInitialized; }
        }

        private bool CreateMesh()
        {
            bool isSucceed = false;

            if(m_IsMeshCountSetted)
            {
                try
                {
                    m_Height = new float[m_MeshX * m_MeshZ];
                    m_XmeshPos = new float[m_MeshX];
                    m_ZmeshPos = new float[m_MeshZ];

                    isSucceed = true;
                }
                catch(System.Exception e)
                {
                    Debug.Log(e.Message);
                    isSucceed = false;
                }
            }
            else
            {
                isSucceed = false;
            }

            return isSucceed;
        }

        private bool SetValueToMesh()
        {
            bool isSucceed = false;

            try
            {
                for(int i=0; i<m_MeshX; i++)
                {
                    m_XmeshPos[i] = m_Xwidth / (m_MeshX - 1) * i;
                }
                for(int j=0; j<m_MeshZ; j++)
                {
                    m_ZmeshPos[j] = m_Zwidth / (m_MeshZ - 1) * j;
                }

                for (int i = 0; i < m_MeshX; i++)
                {
                    for (int j = 0; j < m_MeshZ; j++)
                    {
                        int num = i + j * m_MeshX;

                        float deg = 3.1415926f;
                        deg = deg / (float)(m_MeshX);
                        deg *= 3.0f;
                        deg *= (float)i;
                        //                        deg = deg / (float)(m_MeshX + m_MeshZ);
                        //                        deg *= (float)i + (float)j;

                        //                        m_Height[num] = 1.0f;
                        m_Height[num] = 0.5f * (float)System.Math.Sin((double)deg);

//                        m_Height[num] = 0.5f * (float)i / (float)m_MeshX;
                    }
                }
                isSucceed = true;
            }
            catch(System.Exception e)
            {
                isSucceed = false;
            }

            return isSucceed;
        }
    }
}
