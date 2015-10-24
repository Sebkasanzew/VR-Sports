using UnityEngine;
using System.Collections;

public class Stock : MonoBehaviour {

    Quaternion start;
    float sensivity = 0.5f;
    SixenseInput.Controller controller;
    Quaternion m_initialRotation;

    // Use this for initialization
    void Start () {

        m_initialRotation = this.gameObject.transform.localRotation;
    }
    
    // Update is called once per frame
    void Update () {

        //var rot = controller.Rotation;
        if(controller == null)
        {
            controller = SixenseInput.Controllers[0];
            start = controller.RotationRaw * transform.localRotation;
        }

        UpdateRotation(controller);
    }

    protected void UpdateRotation(SixenseInput.Controller controller)
    {
        this.gameObject.transform.localRotation = controller.Rotation * m_initialRotation;
    }
}
