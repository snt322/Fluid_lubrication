using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class CreateLabels : MonoBehaviour, IMessageCreateLabel
{
    [SerializeField]
    private Transform m_Parent = null;


    List<GameObject> m_Labels = null;
    

    // Use this for initialization
    void Start()
    {
        m_Labels = new List<GameObject>(10);

        
        for (int i = 0; i < 2; i++)
        {
            GameObject gObj = new GameObject();
            gObj.name = "Label X" + i;
            gObj.AddComponent<TextMesh>();

            gObj.transform.SetParent(m_Parent);

            TextMesh mesh = gObj.GetComponent<TextMesh>() as TextMesh;
            mesh.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            mesh.color = Color.red;

            mesh.text = System.String.Format("{0:F1}", (float)(i+1) * 0.5f);
            mesh.characterSize = 0.05f;
            mesh.fontSize = 40;

            gObj.transform.position = new Vector3(0.5f * (float)i + 0.5f, 0, 0);
            gObj.AddComponent<BillBoardToMainCamera>();


            m_Labels.Add(gObj);
        }

        for (int i = 0; i < 2; i++)
        {
            GameObject gObj = new GameObject();
            gObj.name = "Label Y" + i;
            gObj.AddComponent<TextMesh>();

            gObj.transform.SetParent(m_Parent);

            TextMesh mesh = gObj.GetComponent<TextMesh>() as TextMesh;
            mesh.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            mesh.color = Color.green;

            mesh.text = System.String.Format("{0:F1}", (float)(i + 1) * 0.5f);
            mesh.characterSize = 0.05f;
            mesh.fontSize = 40;

            gObj.transform.position = new Vector3(0,0.5f * (float)i + 0.5f, 0);

            gObj.AddComponent<BillBoardToMainCamera>();

            m_Labels.Add(gObj);
        }

        for (int i = 0; i < 2; i++)
        {
            GameObject gObj = new GameObject();
            gObj.name = "Label Z" + i;
            gObj.AddComponent<TextMesh>();

            gObj.transform.SetParent(m_Parent);

            TextMesh mesh = gObj.GetComponent<TextMesh>() as TextMesh;
            mesh.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            mesh.color = Color.blue;

            mesh.text = System.String.Format("{0:F1}", (float)(i + 1) * 0.5f);

            mesh.characterSize = 0.05f;
            mesh.fontSize = 40;

            gObj.transform.position = new Vector3(0, 0, 0.5f * (float)i + 0.5f);

            gObj.AddComponent<BillBoardToMainCamera>();

            m_Labels.Add(gObj);
        }


        //        gObj.AddComponent<UnityEngine.UI.Text>();
        //        gObj.GetComponent<UnityEngine.UI.Text>().text = "Textです。";

        //        Font font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        //        gObj.GetComponent<UnityEngine.UI.Text>().font = font;

        //        gObj.transform.SetParent(GameObject.FindGameObjectWithTag("MainCanvas").transform);

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        foreach(GameObject g in m_Labels)
        {
            Destroy(g);
        }
    }

    private void UpdateLabelPos(Vector3 pos)
    {
        try
        {
            int counter = 0;
            for (int i = 2; i > 0; i--)
            {
                m_Labels[counter].transform.position = new Vector3(pos.x / (float)i, 0, 0);
                counter++;
            }

            for (int i = 2; i > 0; i--)
            {
                m_Labels[counter].transform.position = new Vector3(0, pos.y / (float)i, 0);
                counter++;
            }

            for (int i = 2; i > 0; i--)
            {
                m_Labels[counter].transform.position = new Vector3(0, 0, pos.z / (float)i);
                counter++;
            }
        }
        catch(System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }


    void IMessageCreateLabel.UpdateLabelPos(Vector3 pos)
    {
        UpdateLabelPos(pos);
    }
}

public interface IMessageCreateLabel : IEventSystemHandler
{
    void UpdateLabelPos(Vector3 pos);
}
