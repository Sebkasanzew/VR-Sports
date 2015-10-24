using UnityEngine;
using System.Collections;

public class HitBaseBall : MonoBehaviour
{
	public Vector3 tumble;
    public GameObject bat;

    void OnTriggerEnter(Collider other)
    {

        var r = GetComponent<Rigidbody>();

        //r.velocity = new Vector3(0f, 0f, 0f); //-GetComponent<Rigidbody>().velocity + transform.forward * tumble;
        //r.useGravity = true;
        //r.AddForce(-r.velocity + tumble, ForceMode.Impulse);
        r.AddForceAtPosition(-r.velocity, bat.transform.position, ForceMode.Impulse);
        r.useGravity = true;
    }
}
