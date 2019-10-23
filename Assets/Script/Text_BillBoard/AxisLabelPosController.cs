using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using AxisLabel;

public class AxisLabelPosController : MonoBehaviour, AxisLabel.IMessageSend
{
    private Vector3 m_AxisLabelPos;
    private Vector3 m_LabelPosOffset;

    private UnityAction MyAction;

    private void Awake()
    {
        string tag = this.gameObject.tag;

        AxisLabel.MyTagName.Axis axisEnum = (AxisLabel.MyTagName.Axis)System.Enum.Parse(typeof(AxisLabel.MyTagName.Axis), tag);

        float x, y, z;
        x = y = z = 0;
        switch(axisEnum)
        {
            case AxisLabel.MyTagName.Axis.AxisOrigine:
                x = - AxisLabel.ConstantValue.offset.x;
                y = 0;
                z = - AxisLabel.ConstantValue.offset.z;
                MyAction += AxisOrigineFunc;
                break;
            case AxisLabel.MyTagName.Axis.AxisX:
                x = AxisLabel.ConstantValue.offset.x;
                y = AxisLabel.ConstantValue.offset.y / 2.0f;
                MyAction += AxisXFunc;
                break;
            case AxisLabel.MyTagName.Axis.AxisY:
                y = AxisLabel.ConstantValue.offset.y;
                MyAction += AxisYFunc;
                break;
            case AxisLabel.MyTagName.Axis.AxisZ:
                z = AxisLabel.ConstantValue.offset.z;
                y = AxisLabel.ConstantValue.offset.y / 2.0f;
                MyAction += AxisZFunc;
                break;
        }

        m_LabelPosOffset = new Vector3(x,y,z);



    }

    void AxisOrigineFunc()
    {
        this.gameObject.transform.position = m_LabelPosOffset;
    }

    void AxisXFunc()
    {
        this.gameObject.transform.position = m_AxisLabelPos + m_LabelPosOffset;
    }

    void AxisYFunc()
    {
        this.gameObject.transform.position = m_AxisLabelPos + m_LabelPosOffset;
    }

    void AxisZFunc()
    {
        this.gameObject.transform.position = m_AxisLabelPos + m_LabelPosOffset;
    }

    void IMessageSend.UpdateLabelPos(Vector3 pos)
    {
        m_AxisLabelPos = pos;
        MyAction();
    }
}

namespace AxisLabel
{
    public interface IMessageSend : IEventSystemHandler
    {
        void UpdateLabelPos(Vector3 pos);
    }

    namespace MyTagName
    {
        enum Axis
        {
            AxisOrigine,
            AxisX,
            AxisY,
            AxisZ,
        }
    }

    public static class ConstantValue
    {
        public static readonly Vector3 offset = new Vector3(0.1f, 0.1f, 0.1f);
    }

}