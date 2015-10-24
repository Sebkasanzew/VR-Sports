using UnityEngine;
using System.Collections;

public class HitBaseBall : MonoBehaviour
{
	public float tumble;

	void OnTriggerEnter(Collider other)
	{
		GetComponent<Rigidbody>().velocity = -GetComponent<Rigidbody>().velocity;
	}
}
