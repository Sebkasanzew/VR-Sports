using UnityEngine;
using System.Collections;

public class RandomRotator : MonoBehaviour
{
	public float tumble;

	void Start()
	{
		GetComponent<Rigidbody>().velocity = transform.forward * tumble;
	}

}
