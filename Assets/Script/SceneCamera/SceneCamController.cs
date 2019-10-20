using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class SceneCamController : MonoBehaviour, IMessage
{
    private Transform m_ThisCamTForm;
    private Quaternion m_TmpQuaternion;

    private float m_Distance = 10.0f;
    private readonly float m_DistMinLimit = 0.5f;

    private Vector3 m_CameraFocusPos = new Vector3(0,0,0);
    private Vector3 m_CameraCurrentPos;

    /// <summary>
    /// 現在未使用
    /// </summary>
    private Vector3 m_CameraPos;

    void IMessage.MessageDistance(float distance)
    {
        float tempDist = m_Distance + distance;
        m_Distance = (tempDist < m_DistMinLimit) ? m_DistMinLimit : tempDist;               //m_Distanceの更新



        m_ThisCamTForm.position = m_CameraFocusPos - m_ThisCamTForm.forward * m_Distance;

        Debug.Log(m_Distance);
    }

    void IMessage.MessageRotate(Vector3 rot)
    {
        m_ThisCamTForm.RotateAround(m_CameraFocusPos, m_ThisCamTForm.up, -rot.y);
        m_ThisCamTForm.RotateAround(m_CameraFocusPos, m_ThisCamTForm.right, -rot.x);
        
    }

    void IMessage.MessageMove(Vector3 move)
    {
        if(move == new Vector3(0, 0, 0)) { return; }

        //        float ClipPlane;
        //        Vector3 clipPlaneVect = m_CameraFocusPos - Camera.main.transform.position;
        //       ClipPlane = clipPlaneVect.magnitude;

        //        Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(move.x, move.y, ClipPlane));
        //        Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(move.x, move.y, Camera.main.farClipPlane));

        //        Debug.Log("ClipPlane = " + ClipPlane);


        //        point /= 1000.0f;

        //        Debug.Log("Locale = " + move);
        //        Debug.Log("ScreenToWorld = " + point);
        //        Debug.Log("ScreenToWorld = " + Camera.main.farClipPlane);

        /*
                float ClipPlaneDist = (m_CameraCurrentPos - Camera.main.transform.position).magnitude;

                Vector3 distVect = Camera.main.ScreenToWorldPoint(new Vector3(move.x, move.y, ClipPlaneDist));
                float mag = distVect.magnitude;

                move.Normalize();
                move *= mag;

        Debug.Log("clip dist = " + mag);
        */

        move /= 100.0f;


        Debug.Log("move = " + move);

        Vector3 vect1, vect2;
        vect1 = -move.x * Camera.main.transform.right;
        vect2 = -move.y * Camera.main.transform.up;

        m_CameraFocusPos += (vect1 + vect2);

        Transform tform = Camera.main.transform;
        tform.position += (vect1 + vect2);


//        Camera.main.transform.Translate(0, -point.y, 0);


    }

    void IMessage.MessageRotateConfirm()
    {
        m_TmpQuaternion = m_ThisCamTForm.rotation;
    }

    // Use this for initialization
    void Start()
    {
        m_ThisCamTForm = this.gameObject.GetComponent<Transform>() as Transform;

        m_ThisCamTForm.position = m_CameraFocusPos - m_ThisCamTForm.forward * m_Distance;


        m_TmpQuaternion = m_ThisCamTForm.rotation;

        m_CameraPos = this.gameObject.transform.position;
        m_CameraCurrentPos = this.gameObject.transform.position;
    }

}

public interface IMessage : IEventSystemHandler
{
    void MessageRotate(Vector3 rot);
    void MessageDistance(float distance);
    void MessageRotateConfirm();
    void MessageMove(Vector3 move);
}
