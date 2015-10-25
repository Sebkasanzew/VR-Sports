using UnityEngine;
using System.Collections;

public class BaseballStickController : MonoBehaviour {

	public SixenseHands			Hand;
	public Vector3				Sensitivity = new Vector3( 0.01f, 0.01f, 0.01f );
	
	protected Quaternion		m_initialRotation;
	protected Vector3			m_initialPosition;
	protected Vector3			m_baseControllerPosition;

    private SixenseInput.Controller controller;
		
	// Use this for initialization
	protected virtual void Start() 
	{
		m_initialRotation = this.gameObject.transform.localRotation;
		m_initialPosition = this.gameObject.transform.localPosition;

        controller = SixenseInput.GetController(Hand);

        m_baseControllerPosition = new Vector3(controller.Position.x * Sensitivity.x,
                                               controller.Position.y * Sensitivity.y,
                                               controller.Position.z * Sensitivity.z);
    }
	
	// Update is called once per frame
	void Update () 
	{
		if ( Hand == SixenseHands.UNKNOWN )
		{
			return;
		}
		
		if ( controller != null && controller.Enabled )  
		{		
			UpdateObject(controller);
		}	
	}
		
	protected virtual void UpdateObject(  SixenseInput.Controller controller )
	{
		if (controller.GetButtonDown(SixenseButtons.TRIGGER))
		{
			
			// delta controller position is relative to this point
			
        }
		
		UpdatePosition( controller );
		UpdateRotation( controller );
	}
	
	
	protected void UpdatePosition( SixenseInput.Controller controller )
	{
        Vector3 controllerPosition = new Vector3(controller.Position.x * Sensitivity.x,
                                                  controller.Position.y * Sensitivity.y,
                                                  0);// controller.Position.z * Sensitivity.z );

        // distance controller has moved since enabling positional control
        Debug.Log(m_baseControllerPosition);
        Vector3 vDeltaControllerPos = controllerPosition - m_baseControllerPosition;
		
		// update the localposition of the object
		this.gameObject.transform.localPosition = m_initialPosition + vDeltaControllerPos;
	}
	
	
	protected void UpdateRotation( SixenseInput.Controller controller )
	{
		this.gameObject.transform.localRotation = controller.Rotation * m_initialRotation;
	}
}
