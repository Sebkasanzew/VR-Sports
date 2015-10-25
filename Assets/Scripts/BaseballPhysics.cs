using UnityEngine;
using System.Collections;

public class BaseballPhysics : MonoBehaviour {

    public GameObject baseball;
    public int timeoutDestructor; // TODO implement this
    public int xPosition;
    public int yPosition;
    public int zPosition;
    public int xVelocity;
    public int yVelocity;
    public int zVelocity;
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
        for (int i = 0; i < 100; i += 1) { 
            GameObject clone;
            clone = Instantiate(baseball, new Vector3(xPosition, yPosition, zPosition), transform.rotation) as GameObject;

            clone.GetComponent<Rigidbody>().velocity = transform.TransformDirection(new Vector3(xVelocity + Random.Range(-10.0F, 10.0F), 
                                                                                                yVelocity + Random.Range(-10.0F, 10.0F), 
                                                                                                zVelocity + Random.Range(-10.0F, 10.0F)));
            //Debug.Log("created" + i);
            Destroy(clone, 3000);
            yield return new WaitForSeconds(frequency);
        }
    }

    void FixedUpdate()
    {
        baseballRigid.AddForce(Physics.gravity, ForceMode.Acceleration);
    }
}
