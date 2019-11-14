using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFunction
{
    /// <summary>
    /// マウスボタン押下した最初のスクリーン座標を格納する辺陬
    /// </summary>
    private Vector2 m_MouseDownPos;

    /// <summary>
    /// m_MouseUpPosは未使用。
    /// ただし、MouseUp(Vector2 pos)は左ボタン押下の終了フラグをセットするので削除しないでください。
    /// </summary>
    private Vector2 m_MouseUpPos;

    /// <summary>
    /// マウスドラッグ中かの判定に使用する。
    /// ドラッグ中か検査するフレームでのマウス座標(Input.mousePosition)をCurrentMousePosプロパティを通してセットしてください。
    /// </summary>
    private Vector2 m_MouseCurrentPos;

    /// <summary>
    /// マウスドラッグ中かの判定に使用する。
    /// マウスボタン押下中のマウスのスクリーン座標、m_MouseCurrentPosを更新する前の値
    /// m_MouseCurrentPos != m_MouseFormerPosの場合にMouseDraggedVectorプロパティのゲッタにVector2.zero以外を返す。
    /// </summary>
    private Vector2 m_MouseFormerPos;



    /// <summary>
    /// マウスボタン押下中のマウスドラッグ量を格納する
    /// </summary>
    private Vector2 m_MouseDragVector2;


    /// <summary>
    /// マウスボタンの状態(押下orNot)を格納する変数
    /// </summary>
    private bool m_IsMouseDown;

    /// <summary>
    /// デフォルトコンストラクタ
    /// </summary>
    public MouseFunction()
    {
        Initialize();
    }

    /// <summary>
    /// 初期化処理、マウスボタンがリリースされている状態にする(m_IsMouseDown = false)
    /// </summary>
    private void Initialize()
    {
        m_IsMouseDown = false;
    }

    /// <summary>
    /// マウスボタンの押下中かどうかを取得するプロパティ
    /// </summary>
    public bool IsMouseDown
    {
        get { return m_IsMouseDown; }
    }

    /// <summary>
    /// マウスボタン押下中フラグ(m_IsMouseDown == true)をリセットするメソッド。
    /// 押下が終了したら必ず呼んでください。
    /// </summary>
    /// <param name="pos"></param>
    public void MouseUp(Vector2 pos)
    {
        m_IsMouseDown = false;
        m_MouseUpPos = pos;             //マウス押下が終了したスクリーン座標を格納する。この値は未使用。
    }

    /// <summary>
    /// マウスボタン押下中フラグをセットして、押下スタートしたスクリーン座標を格納するメソッド。
    /// 押下したら必ず呼んでください。
    /// </summary>
    /// <param name="pos">ボタン押下時のマウス座標(スクリーン座標)</param>
    public void MouseDown(Vector2 pos)
    {
        m_IsMouseDown = true;
        m_MouseDownPos = pos;
    }

    /// <summary>
    /// マウスドラッグを検出するためのプロパティ
    /// Monobehavior.Update()内でm_IsMouseDown == trueの場合に、マウス座標PointerEventData.positionをセットしてください。
    /// </summary>
    public Vector3 CurrentMousePos
    {
        set { m_MouseCurrentPos = new Vector2(value.x, value.y); }
    }

    /// <summary>
    /// マウスドラッグを検出するためのプロパティ
    /// IPointerDownHandler.OnPointerDown(PointerEventData eventData)内でマウスドラッグを開始する場合にeventData.positionをセットしてください
    /// </summary>
    public Vector2 FormerMousePos
    {
        set { m_MouseFormerPos = value; }
    }

    /// <summary>
    /// 現在のフレームと前のフレーム間でマウスがドラッグされたスクリーン座標上での距離を返す。
    /// </summary>
    public Vector2 MouseDragedVector
    {
        get
        {
            Vector2 MouseDragVector = m_MouseCurrentPos - m_MouseFormerPos;                                 //マウスがドラッグされたスクリーン座標系での方向ベクトル
                                                                                                            //「現在のマウス位置(m_MouseCurrentPos)」 - 「1フレーム前のマウス位置(m_MouseFormerPos)」
            m_MouseFormerPos = (MouseDragVector == Vector2.zero) ? m_MouseFormerPos : m_MouseCurrentPos;    //マウスがドラッグされた場合は1フレーム前のマウス位置(m_MouseFormerPos)を現在のマウス位置(m_MouseCurrentPos)に更新する

            return MouseDragVector;
        }
    }


}
