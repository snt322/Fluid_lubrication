using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFunction
{
    private Vector2 m_MouseDownPos;

    /// <summary>
    /// m_MouseUpPosは未使用。
    /// ただし、MouseUp(Vector2 pos)は左ボタン押下の終了フラグをセットするので削除しないでください。
    /// </summary>
    private Vector2 m_MouseUpPos;
    private Vector2 m_MouseCurrentPos;

    private bool m_IsMouseDown;

    /// <summary>
    /// デフォルトコンストラクタ
    /// </summary>
    public MouseFunction()
    {
        Initialize();
    }

    private void Initialize()
    {
        m_IsMouseDown = false;
    }

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
    /// 押下を開始したら必ず呼んでください。
    /// </summary>
    /// <param name="pos"></param>
    public void MouseDown(Vector2 pos)
    {
        m_IsMouseDown = true;
        m_MouseDownPos = pos;
    }

    public Vector2 CurrentMousePos
    {
        set { m_MouseCurrentPos = value; }
    }

    /// <summary>
    /// 現在使用していない
    /// </summary>
    public Vector2 MouseDragVector
    {
        get { return (m_MouseCurrentPos - m_MouseDownPos); }
    }

}
