using UnityEngine;
using System.Collections;

public class RandomRotator : MonoBehaviour
{
	public Vector3 tumble;

	void Start()
	{
		GetComponent<Rigidbody>().velocity = transform.forward + -tumble;
	}

}
