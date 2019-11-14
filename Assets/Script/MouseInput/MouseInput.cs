using System.Collections;
using System.Collections.Generic;
using MyMouseInputInterface;
using UnityEngine;

using UnityEngine.EventSystems;

/*
 * IPointerHandlerなどのイベントシステムはRaycasterによって入力イベントがどこで発生したかを検出する。
 * //https://docs.unity3d.com/jp/540/Manual/Raycasters.html
 * 入力イベントを取得するには
 * ①カメラにレイキャスタをアタッチする。
 * ②入力イベントを受けるオブジェクトにコライダーをアタッチする。
 * カメラにレイキャスタをアタッチしていない場合はすべての入力イベントは受け付けない。 
 */




public class MouseInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IScrollHandler, MyMouseInputInterface.ISendMessage
{
    [SerializeField]
    GameObject m_SceneCamera = null;


    [SerializeField, Tooltip("左マウスクリックで表示するパネル(MenuContext)をセットしてください。")]
    private GameObject m_GameObjectMenuContext = null;

    /// <summary>
    /// マウス左ボタンのマウスドラッグ動作を制御するオブジェクト
    /// </summary>
    private MouseFunction m_MouseLeft = new MouseFunction();

    /// <summary>
    /// マウスホイールボタンのマウスドラッグ動作を制御するオブジェクト
    /// </summary>
    private MouseFunction m_MouseMiddle = new MouseFunction();




    // Update is called once per frame
    void Update()
    {

        if (m_MouseLeft.IsMouseDown)
        {
            m_MouseLeft.CurrentMousePos = Input.mousePosition;                                          //現在のマウス座標をセット

            Vector2 tmpRot = m_MouseLeft.MouseDragedVector;
            tmpRot /= 20.0f;                                                                            //20.0fはマウスドラッグ量を減衰して回転が大きくなりすぎないようにするため
            Vector3 rot = new Vector3(tmpRot.y, -tmpRot.x, 0);

            UnityEngine.EventSystems.ExecuteEvents.Execute<IMessage>(m_SceneCamera, null, (sender, eventData) => { sender.MessageRotate(rot); });
        }

        if(m_MouseMiddle.IsMouseDown)
        {
            m_MouseMiddle.CurrentMousePos = Input.mousePosition;

            Vector2 mov = m_MouseMiddle.MouseDragedVector;
            mov /= 20.0f;
            Vector3 move = new Vector3(mov.x, mov.y, 0);

            UnityEngine.EventSystems.ExecuteEvents.Execute<IMessage>(m_SceneCamera, null, (sender, eventData) => { sender.MessageMove(move); });
        }

    }


    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            m_MouseLeft.MouseUp(eventData.position);
            UnityEngine.EventSystems.ExecuteEvents.Execute<IMessage>(m_SceneCamera, null, (sender, sendEventData) => { sender.MessageRotateConfirm(); });

            UnityEngine.EventSystems.ExecuteEvents.Execute<MenuContext.ISendMessage>(m_GameObjectMenuContext, eventData, (sender, eData) => { sender.Hide(); });
        }

        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            m_MouseMiddle.MouseUp(eventData.position);

            UnityEngine.EventSystems.ExecuteEvents.Execute<MenuContext.ISendMessage>(m_GameObjectMenuContext, eventData, (sender, eData) => { sender.Hide(); });
        }

        if(eventData.button == PointerEventData.InputButton.Right)
        {
            UnityEngine.EventSystems.ExecuteEvents.Execute<MenuContext.ISendMessage>(m_GameObjectMenuContext, eventData, (sender, eData) => { sender.Show(eventData); });
        }

    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            m_MouseLeft.MouseDown(eventData.position);

            m_MouseLeft.FormerMousePos = eventData.position;
        }

        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            m_MouseMiddle.MouseDown(eventData.position);
            m_MouseMiddle.FormerMousePos = eventData.position;
        }

    }


    void IScrollHandler.OnScroll(PointerEventData eventData)
    {
        var delta = eventData.scrollDelta / 10.0f;

        UnityEngine.EventSystems.ExecuteEvents.Execute<IMessage>(m_SceneCamera, null, (sender, sendEventData) => { sender.MessageDistance(delta.y); });
    }

    void ISendMessage.SendMouseDelta(float delta)
    {
        UnityEngine.EventSystems.ExecuteEvents.Execute<IMessage>(m_SceneCamera, null, (sender, sendEventData) => { sender.MessageDistance(delta); });
    }
}


namespace MyMouseInputInterface
{
    public interface ISendMessage : IEventSystemHandler
    {
        void SendMouseDelta(float delta);
    }
}