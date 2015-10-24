using UnityEngine;
using System.Collections;

public class BaseballPhysics : MonoBehaviour {

    public GameObject baseball;
    public int timeoutDestructor; // TODO implement this
    public Vector3 position;
    public Vector3 velocity;
    public int frequency;

    private Rigidbody baseballRigid;

    // Use this for initialization
    void Start () {
        baseballRigid = baseball.GetComponent<Rigidbody>();
        StartCoroutine(BallFactory());
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    IEnumerator BallFactory() {
        for (int i = 0; i < 20; i += 1) { 
            GameObject clone;
            clone = Instantiate(baseball, position, transform.rotation) as GameObject;
            clone.GetComponent<Rigidbody>().velocity = transform.TransformDirection(velocity);
            Debug.Log("created" + i);
            Destroy(clone, 3000);
            yield return new WaitForSeconds(frequency);
        }
    }

    void FixedUpdate()
    {
        baseballRigid.AddForce(Physics.gravity, ForceMode.Acceleration);
    }
}
