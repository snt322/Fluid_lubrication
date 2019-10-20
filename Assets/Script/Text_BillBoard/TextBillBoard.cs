using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class TextBillBoard : MonoBehaviour , IBillBoardSendMessage
{
    [SerializeField]
    private UnityEngine.Camera m_Camera = null;


    // Update is called once per frame
    void Update()
    {
        Vector3 lookToPos = this.gameObject.transform.position - m_Camera.transform.position;

        this.gameObject.transform.LookAt(lookToPos, this.m_Camera.transform.up);
    }

    void IBillBoardSendMessage.SetWorldPosition(Vector3 position)
    {
        this.gameObject.transform.position = position;
        Debug.Log("Called ISendMessage.TestSend(Vector3 position). Position is " + position);
    }

}

public interface IBillBoardSendMessage : IEventSystemHandler
{
    void SetWorldPosition(Vector3 position);
}
