using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

/*
 * IPointerHandlerなどのイベントシステムはRaycasterによって入力イベントがどこで発生したかを検出する。
 * //https://docs.unity3d.com/jp/540/Manual/Raycasters.html
 * 入力イベントを取得するには
 * ①カメラにレイキャスタをアタッチする。
 * ②入力イベントを受けるオブジェクトにコライダーをアタッチする。
 * カメラにレイキャスタをアタッチしていない場合はすべての入力イベントは受け付けない。
 * 
 * サポートされているイベント
 * 
 */




public class MouseInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IScrollHandler
{
    [SerializeField]
    GameObject m_SceneCamera = null;


    [SerializeField, Tooltip("左マウスクリックで表示するパネル(MenuContext)をセットしてください。")]
    GameObject m_GameObjectMenuContext = null;


    private MouseFunction m_MouseFunc = new MouseFunction();

    private Vector2 m_FormerMousePos;
    private Vector2 m_FormerMouseMiddlePos;

    private bool m_IsMiddleDown = false;



    // Update is called once per frame
    void Update()
    {

        if (m_MouseFunc.IsMouseDown)
        {
            Vector2 tmpVect = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector2 MouseDragVector = tmpVect - m_FormerMousePos;
            m_FormerMousePos = (MouseDragVector == new Vector2(0, 0)) ? m_FormerMousePos : tmpVect;


            MouseDragVector /= 20.0f;

            Vector3 rot = new Vector3(MouseDragVector.y, -MouseDragVector.x, 0);


            UnityEngine.EventSystems.ExecuteEvents.Execute<IMessage>(m_SceneCamera, null, (sender, eventData) => { sender.MessageRotate(rot); });
            Debug.Log("Mouse Delta = " + tmpVect);
        }

        if(m_IsMiddleDown)
        {
            Vector2 tmpVect = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector2 MouseDragVector = tmpVect - m_FormerMouseMiddlePos;
            m_FormerMouseMiddlePos = (MouseDragVector == new Vector2(0, 0)) ? m_FormerMouseMiddlePos : tmpVect;

            Vector3 move = new Vector3(MouseDragVector.x, MouseDragVector.y, 0);

            UnityEngine.EventSystems.ExecuteEvents.Execute<IMessage>(m_SceneCamera, null, (sender, eventData) => { sender.MessageMove(move); });
        }

    }


    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            m_MouseFunc.MouseUp(eventData.position);
            UnityEngine.EventSystems.ExecuteEvents.Execute<IMessage>(m_SceneCamera, null, (sender, sendEventData) => { sender.MessageRotateConfirm(); });
            Debug.Log("OnPointerUp" + eventData.position);

            UnityEngine.EventSystems.ExecuteEvents.Execute<MenuContext.ISendMessage>(m_GameObjectMenuContext, eventData, (sender, eData) => { sender.Hide(); });
        }

        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            m_IsMiddleDown = false;
            Debug.Log("Middle Up.");

            UnityEngine.EventSystems.ExecuteEvents.Execute<MenuContext.ISendMessage>(m_GameObjectMenuContext, eventData, (sender, eData) => { sender.Hide(); });
        }

        if(eventData.button == PointerEventData.InputButton.Right)
        {
            UnityEngine.EventSystems.ExecuteEvents.Execute<MenuContext.ISendMessage>(m_GameObjectMenuContext, eventData, (sender, eData) => { sender.Show(eventData); });            Debug.Log("Right Up.");
        }

    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            m_MouseFunc.MouseDown(eventData.position);
            m_MouseFunc.CurrentMousePos = eventData.position;
            Debug.Log("OnPointerDown" + eventData.position);

            m_FormerMousePos = eventData.position;
        }

        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            m_IsMiddleDown = true;
            m_FormerMouseMiddlePos = eventData.position;

            Debug.Log("Middle Down.");
        }

    }


    void IScrollHandler.OnScroll(PointerEventData eventData)
    {
        var delta = eventData.scrollDelta / 10.0f;

        UnityEngine.EventSystems.ExecuteEvents.Execute<IMessage>(m_SceneCamera, null, (sender, sendEventData) => { sender.MessageDistance(delta.y); });

        Debug.Log("Scroll. : " + delta);


    }
}
