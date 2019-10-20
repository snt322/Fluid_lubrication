using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoardToMainCamera : MonoBehaviour
{
    [Header("BillBoardの設定")]
    [SerializeField, Tooltip("ターゲットなるカメラのTransformをセットしてください。")]
    private Transform m_TargetCam = null;

    private void Start()
    {
        GameObject gObj = GameObject.FindGameObjectWithTag("MainCamera");

        m_TargetCam = gObj.GetComponent<Transform>() as Transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 lookToPos = this.gameObject.transform.position - m_TargetCam.transform.position;

        this.gameObject.transform.LookAt(lookToPos, this.m_TargetCam.transform.up);
    }
}
