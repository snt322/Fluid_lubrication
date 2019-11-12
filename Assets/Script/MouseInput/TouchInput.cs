using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInput : MonoBehaviour
{
    UnityEngine.UI.Text m_Text = null;

    [Header("Editor上のマウス処理")]
    [SerializeField, Tooltip("MouseInput.csスクリプトをアタッチしたGameObjectをセットしてください。")]
    private GameObject m_InterfaceTarget = null;

    [Space(1)]

    [Header("Android上のタッチ処理")]
    [SerializeField, Tooltip("左マウスクリックで表示するパネル(MenuContext)をセットしてください。")]
    private GameObject m_GameObjectMenuContext = null;

    [Space(1)]

    [Header("ピンチ操作による拡大・縮小の度合いの設定")]
    [SerializeField, Tooltip("m_DrawScaleが正の値の場合、ピンチアウトで拡大、ピンチインで縮小となる。"), Range(-10, 10)]
    private float m_DrawScale = 1.0f;

    // Use this for initialization
    void Start()
    {
        GameObject gObj = GameObject.Find("Statul_Panel_Right") as GameObject;

        m_Text = gObj.GetComponentInChildren<UnityEngine.UI.Text>() as UnityEngine.UI.Text;
    }

    // Update is called once per frame
    void Update()
    {
        updateTouch();
        updateTap();
    }

    private int m_Counter = 0;

    private Vector2 m_Touch0Pos;
    private Vector2 m_Touch1Pos;


    private void updateTouch()
    {
        int touchCount = Input.touchCount;

        if(touchCount <= 0) { return; }

        if(touchCount == 2)         //タッチ操作で拡大・縮小操作を行う
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (touch0.phase == TouchPhase.Began || touch0.phase == TouchPhase.Stationary)
            {
                m_Touch0Pos = touch0.position;
            }
            if (touch1.phase == TouchPhase.Began || touch1.phase == TouchPhase.Stationary)
            {
                m_Touch1Pos = touch1.position;
            }



            if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                var valueCurrent = (touch0.position - touch1.position).magnitude;
                var valueFormer = (m_Touch0Pos - m_Touch1Pos).magnitude;

                var value = valueCurrent - valueFormer;
                value /= - ((float)Screen.width * m_DrawScale);                     //スクリーン幅を基準にピンチ操作での画面表示の拡大・縮小量を変更する。

                UnityEngine.EventSystems.ExecuteEvents.Execute<MyMouseInputInterface.ISendMessage>(m_InterfaceTarget, null, (sender, eventData) => { sender.SendMouseDelta(value); });
            }


        }


        

    }

    private void updateTap()
    {
        var touchCount = Input.touchCount;
        if(touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.tapCount == 2)
            {
                var tapCount = touch.tapCount;
                Debug.Log("tapCount = " + tapCount);
                UnityEngine.EventSystems.ExecuteEvents.Execute<MenuContext.ISendMessage>(m_GameObjectMenuContext, null, (sender, eData) => { sender.Show(touch.position); });
            }
            else
            {
//                UnityEngine.EventSystems.ExecuteEvents.Execute<MenuContext.ISendMessage>(m_GameObjectMenuContext, null, (sender, eData) => { sender.Hide(); });
            }
        }
    }






}
