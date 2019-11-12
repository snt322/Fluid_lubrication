using System.Collections;
using System.Collections.Generic;
using MenuContext;
using UnityEngine;

using UnityEngine.EventSystems;

/// <summary>
/// MenuContex(UI.Panel)の表示をコントロールするスクリプト
/// 親オブジェクト(Canvas)にPivot(0.5,0.5)で配置してあることを前提とする
/// このオブジェクトのローカル座標はScreenの中心座標とする
/// </summary>
public class MenuContextController : MonoBehaviour, MenuContext.ISendMessage, IPointerUpHandler, IPointerDownHandler
{
    [Header("カメラ操作")]
    [SerializeField, Tooltip("操作するカメラをGameObjectをセットしてください。")]
    GameObject m_SceneCamera = null;
    [Space(1)]

    [Header("圧力表示/メッシュ表示の選択")]
    [SerializeField, Tooltip("PressureMeshControllerをセットしたGameObjectをセットしてください。")]
    private GameObject m_MeshControllerObj = null;
    [SerializeField, Tooltip("ResultMeshControllerをセットしたGameObjectをセットしてください。")]
    private GameObject m_ResultMeshControllerObj = null;


    Transform m_ThisTrans = null;

    private void Awake()
    {
        m_ThisTrans = this.gameObject.transform;
    }

    private void Start()
    {
        if(m_SceneCamera == null)
        {
            m_SceneCamera = GameObject.FindGameObjectWithTag("MainCamera") as GameObject;
        }
    }

    void ISendMessage.Show(Vector3 pos)
    {
        var sHalfWidth = Screen.width / 2;
        var sHalfHight = Screen.height / 2;

        var screenCenterPos = new Vector3(sHalfWidth, sHalfHight, 0);   //スクリーンの中心座標
        var posRelateToParent = pos - screenCenterPos;

        m_ThisTrans.localPosition = posRelateToParent;
//        Debug.Log("ISendMessage.Show(Vector3).");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventData"></param>
    void ISendMessage.Show(PointerEventData eventData)
    {
        float x = eventData.position.x;
        float y = eventData.position.y;
        float z = 0;

        var sHalfWidth = Screen.width / 2;
        var sHalfHight = Screen.height / 2;

        var screenCenterPos = new Vector3(sHalfWidth, sHalfHight, 0);   //スクリーンの中心座標
        var pointerPos = new Vector3(x, y, z);
        var posRelateToParent = pointerPos - screenCenterPos;

        m_ThisTrans.localPosition = posRelateToParent;
    }

    /// <summary>
    /// 画面外の遠くへ移動
    /// </summary>
    void ISendMessage.Hide()
    {
        Hide();
    }
    private void Hide()
    {
        var movX = Screen.width / 2;
        var movY = Screen.height;
        m_ThisTrans.localPosition = new Vector3(movX, 0, 0);
//        Debug.Log("ISendMessage.Hide().");
    }


    public void Button_ResetGraphPos()
    {
        UnityEngine.EventSystems.ExecuteEvents.Execute<IMessage>(m_SceneCamera, null, (sender, eventData) => { sender.MessageResetPos(); });
//        Debug.Log("Button_ResetGraphPos().");

        Hide();         //MenuContexを隠す。
    }

    public void Button_ShowPressure()
    {
//        Debug.Log("Button_ShowPressure().");

        UnityEngine.EventSystems.ExecuteEvents.Execute<PressureMesh.ISendMessage>(m_MeshControllerObj, null, (sender,eventData)=> { sender.CreateAndSetPressureMesh(); });

        UnityEngine.EventSystems.ExecuteEvents.Execute<ResultMesh.ISendMessage>(m_ResultMeshControllerObj, null, (sender, eventData) => { sender.ShowPressureMesh(); });


        Hide();         //MenuContexを隠す。
    }


    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        Hide();
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("PointerDown().");
    }
}

namespace MenuContext
{
    public interface ISendMessage : IEventSystemHandler
    {
        void Show(Vector3 pos);
        void Show(PointerEventData eventData);
        void Hide();
    }
}
