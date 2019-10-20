using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFunction
{
    private Vector2 m_MouseDownPos;
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

    public void MouseUp(Vector2 pos)
    {
        m_IsMouseDown = false;
        m_MouseUpPos = pos;
    }

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
