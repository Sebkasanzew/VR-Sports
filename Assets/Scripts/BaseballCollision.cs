using UnityEngine;
using System.Collections;

public class BaseballCollision : MonoBehaviour {
    Vector3 contact;
    // Use this for initialization

	void Start () {
        //Debug.Log(transform.GetComponent<Rigidbody>().velocity);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision c)
    {
        //Debug.Log("Wooohooo Collision!");
        //Debug.Log(c);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Bat")
        {

            AudioSource audio = GetComponent<AudioSource>();
            audio.Play();

            var impactVelocityX = transform.GetComponent<Rigidbody>().velocity.x - other.GetComponent<Rigidbody>().velocity.x;
            impactVelocityX *= Mathf.Sign(impactVelocityX);
            var impactVelocityY = transform.GetComponent<Rigidbody>().velocity.y - other.GetComponent<Rigidbody>().velocity.y;
            impactVelocityY *= Mathf.Sign(impactVelocityY);
            var impactVelocityZ = transform.GetComponent<Rigidbody>().velocity.z - other.GetComponent<Rigidbody>().velocity.z;
            impactVelocityZ *= Mathf.Sign(impactVelocityZ);

            var impactVelocity = impactVelocityX + impactVelocityY + impactVelocityZ;
            var impactForce = impactVelocity * transform.GetComponent<Rigidbody>().mass * other.GetComponent<Rigidbody>().mass * 0.2F;
            impactForce *= Mathf.Sign(impactForce);

            Debug.Log("impactForce: " + impactForce);

            transform.GetComponent<Rigidbody>().velocity = new Vector3(0, 10, impactForce);           
        }
        
    }

}
