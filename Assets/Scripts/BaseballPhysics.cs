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
        for (int i = 0; i < 100; i += 1) { 
            GameObject clone;
            clone = Instantiate(baseball, new Vector3(position.x, position.y, position.z), transform.rotation) as GameObject;

            clone.GetComponent<Rigidbody>().velocity = transform.TransformDirection(new Vector3(velocity.x + Random.Range(-10.0F, 10.0F),
                                                                                                velocity.y + Random.Range(-10.0F, 10.0F),
                                                                                                velocity.z + Random.Range(-10.0F, 10.0F)));
            Destroy(clone, 3000);
            yield return new WaitForSeconds(frequency);
        }
    }

    void FixedUpdate()
    {
        baseballRigid.AddForce(Physics.gravity, ForceMode.Acceleration);
    }
}
