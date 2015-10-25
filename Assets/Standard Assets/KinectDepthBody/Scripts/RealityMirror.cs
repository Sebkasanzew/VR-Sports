using Assets.KinectDepthBody;
using UnityEngine;
using System.Collections;

public class RealityMirror : MonoBehaviour
{

    public bool OverrideScale = false;
    private KinectDataManager _kinectDataManager;


	// Use this for initialization
	void Start ()
	{
        _kinectDataManager = KinectDataManager.GetInstance();
	    var texture = _kinectDataManager.GetColorTexture();

	    if (OverrideScale)
	    {
	        gameObject.transform.localScale = new Vector3(texture.width, texture.height, 1);
	    }

        gameObject.GetComponent<Renderer>().material.mainTexture = texture;
	}
}
