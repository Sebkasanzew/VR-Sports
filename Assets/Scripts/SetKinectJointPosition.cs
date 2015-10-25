using UnityEngine;
using System.Collections;
using Windows.Kinect;
using Assets.KinectDepthBody;

public class SetKinectJointPosition : MonoBehaviour {

    private KinectSensor _sensor;
    private BodyFrameReader _bodyFrameReader;
    private CoordinateMapper _coordinateMapper;
    private Body[] _bodies;
    private KinectDataManager _kinectDataManager;

    public Vector3 StaticPositionOffset;
    public JointType joint;

    public Vector3 scaleOffset = new Vector3(1f, 1f, 1f);
    public int Smooth = 1;

    private float[] _depthData;

    Vector3 parentPosition;

    // Use this for initialization
    void Start() {
        //Get Kincet Components
        _sensor = KinectSensor.GetDefault();
        if (!_sensor.IsOpen)
        {
            _sensor.Open();
        }

        _coordinateMapper = _sensor.CoordinateMapper;
        _bodyFrameReader = _sensor.BodyFrameSource.OpenReader();

        _kinectDataManager = KinectDataManager.GetInstance();


        _bodyFrameReader.FrameArrived += _bodyFrameReader_FrameArrived;



    }

    private void _bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
    {
        BodyFrameReference frameReference = e.FrameReference;
        using (var frame = frameReference.AcquireFrame())
        {
            if (frame != null)
            {
                _bodies = new Body[frame.BodyFrameSource.BodyCount];
                frame.GetAndRefreshBodyData(_bodies);

                if (_bodies.Length == 0)
                {
                    Debug.Log("No Body found!");
                    return;
                }

                Body body = null;
                foreach (var b in _bodies)
                {
                    if (b != null && b.IsTracked)
                    {
                        body = b;
                        break;
                    }
                }

                if (body == null)
                {
                    Debug.Log("No body tracked");
                    return;
                }

                parentPosition = this.gameObject.transform.parent.transform.position;

                _depthData = _kinectDataManager.GetDepthPointBuffer();

                var bodyPart = body.Joints[joint];

                var depth = _coordinateMapper.MapCameraPointToDepthSpace(bodyPart.Position);
                var index = (int)depth.X + (int)depth.Y * 512;


                if (index >= _depthData.Length)
                {
                    return;
                }

                var posZ = _depthData[index];


                var posX = 1 - depth.X + 512;
                var posY = 1 - depth.Y + 424;

                posX *= -1;
                posZ *= -1;

                var scaledPosition = new Vector3(posX * scaleOffset.x, posY * scaleOffset.y, posZ * scaleOffset.z);

                scaledPosition += parentPosition;
                scaledPosition += StaticPositionOffset;

                //gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, scaledPosition, Smooth * Time.deltaTime);
                gameObject.transform.position = scaledPosition;
            }
        }
    }

}