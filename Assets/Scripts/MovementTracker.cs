using UnityEngine;
using System.Collections;

public class MovementTracker : MonoBehaviour {

    public Transform objectSpineBase;
    public Transform objectSpineMid;
    public Transform objectNeck;
    public Transform objectHead;
    public Transform objectShoulderLeft;
    public Transform objectElbowLeft;
    public Transform objectWristLeft;
    public Transform objectHandLeft;
    public Transform objectShoulderRight;
    public Transform objectElbowRight;
    public Transform objectWristRight;
    public Transform objectHandRight;
    public Transform objectHipLeft;
    public Transform objectKneeLeft;
    public Transform objectAnkleLeft;
    public Transform objectFootLeft;
    public Transform objectHipRight;
    public Transform objectKneeRight;
    public Transform objectAnkleRight;
    public Transform objectFootRight;
    public Transform objectSpineShoulder;
    public Transform objectHandTipLeft;
    public Transform objectThumbLeft;
    public Transform objectHandTipRight;
    public Transform objectThumbRight;

    public Windows.Kinect.JointType[] tracker = new Windows.Kinect.JointType[25];

    private Transform[] objects = new Transform[25];

    public float posFactor = 2;
    public float lowPassFactor = 0.1F;
    
    private Vector3 currentPos1;
    private Vector3 currentPos2;

    BodySourceManager bodySourceManager;

	// Use this for initialization
	void Start () {
        bodySourceManager = GetComponent<BodySourceManager>();

        objects[0] = objectSpineBase;
        objects[1] = objectSpineMid;
        objects[2] = objectNeck;
        objects[3] = objectHead;
        objects[4] = objectShoulderLeft;
        objects[5] = objectElbowLeft;
        objects[6] = objectWristLeft;
        objects[7] = objectHandLeft;
        objects[8] = objectShoulderRight;
        objects[9] = objectElbowRight;
        objects[10] = objectWristRight;
        objects[11] = objectHandRight;
        objects[12] = objectHipLeft;
        objects[13] = objectKneeLeft;
        objects[14] = objectAnkleLeft;
        objects[15] = objectFootLeft;
        objects[16] = objectHipRight;
        objects[17] = objectKneeRight;
        objects[18] = objectAnkleRight;
        objects[19] = objectFootRight;
        objects[20] = objectSpineShoulder;
        objects[21] = objectHandTipLeft;
        objects[22] = objectThumbLeft;
        objects[23] = objectHandTipRight;
        objects[24] = objectThumbRight;
	}

    void FixedUpdate()
    {
        if (bodySourceManager == null)
        {
            return;
        }

        Windows.Kinect.Body[] data = bodySourceManager.GetData();
        if (data == null)
        {
            return;
        }

        int counter = 0;
        foreach (var body in data){
            counter++;

            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
/*                var pos = body.Joints[track1].Position;
                CalcLowPassValues(new Vector3(pos.X, pos.Y, -pos.Z), ref currentPos1);
                object1.position = currentPos1 * posFactor;

                pos = body.Joints[track2].Position;
                CalcLowPassValues(new Vector3(pos.X, pos.Y, -pos.Z), ref currentPos2);
                object2.position = currentPos2 * posFactor;*/


                for (var i = 0; i < tracker.Length; i++)
                {
                    if (objects[i] != null)
                    {
                        var pos = body.Joints[tracker[i]].Position;
                        CalcLowPassValues(new Vector3(pos.X, pos.Y, -pos.Z), ref currentPos1);
                        objects[i].position = currentPos1 * posFactor;
                    }
                }
                break;
            }
        }
    }

	// Update is called once per frame
	void Update () {
	
	}

    void CalcLowPassValues(Vector3 newPos, ref Vector3 resultPos)
    {
        resultPos = Vector3.Lerp(resultPos, newPos, lowPassFactor);
    }
}
