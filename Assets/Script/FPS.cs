using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : MonoBehaviour {

    [SerializeField, Tooltip("FPSを表示するUI.Textをセットしてください。")]
    private UnityEngine.UI.Text m_Text = null;              //FPSを表示するUI.Textをセット


    private int m_FrameCounter = 0;
    private System.DateTime m_StartTime;
    private System.DateTime m_EndTime;
    private System.TimeSpan m_Span;

    private float m_FPS = 0;

	// Use this for initialization
	void Start () {
        m_StartTime = System.DateTime.Now;
        m_EndTime = System.DateTime.Now;
        m_Span = m_EndTime - m_StartTime;
        m_FPS = 0;
        m_FrameCounter = 0;
	}
	
	// Update is called once per frame
	void Update () {

        m_EndTime = System.DateTime.Now;

        m_Span = m_EndTime - m_StartTime;

        m_FrameCounter++;

        if (m_Span.TotalMilliseconds >= 100.0f)
        {
            double sec = m_Span.TotalMilliseconds / 1000.0;
            m_FPS = (float)((double)m_FrameCounter / sec);

            string str = System.String.Format("FPS = {0,3:F}fps" ,m_FPS);
//            Debug.Log(str);

            m_Text.text = str;

            m_StartTime = System.DateTime.Now;
            m_FrameCounter = 0;
        }

	}
}
