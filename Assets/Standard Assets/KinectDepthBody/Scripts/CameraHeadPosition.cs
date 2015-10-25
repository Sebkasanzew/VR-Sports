using System;
using System.Linq;
using Windows.Kinect;
using UnityEngine;
using System.Collections;

public class CameraHeadPosition : MonoBehaviour {


    private KinectSensor _sensor;
    private BodyFrameReader _bodyFrameReader;
    private CoordinateMapper _coordinateMapper;
    private Body[] _bodies;

    public Vector3 StaticPositionOffset;
    public float Smooth;


    private DepthParticle _depthparticle;

    private Vector3 _positionOffset = new Vector3(0, 0, 0);
    private Vector3 _scaleOffset = new Vector3(0, 0, 0);

    private float _lastHeadZ = 0;
    public float ZDiffLimit = 2f;

	public float minDiffLimit = 0;

    private Vector3 _lastRotationVector;
    private Vector3 _lastRotationPivotVector;
    private Matrix4x4 _rotationMatrix_x = new Matrix4x4();
    private Matrix4x4 _rotationMatrix_y = new Matrix4x4();
    private Matrix4x4 _rotationMatrix_z = new Matrix4x4();
	private Vector3 _lastHeadPosition;


    private Matrix4x4 _emptyMatrix = new Matrix4x4();

	// Use this for initialization
	void Start () 
    {
        //Get Kincet Components
        _sensor = KinectSensor.GetDefault();
        if (!_sensor.IsOpen)
        {
            _sensor.Open();
        }

	    _coordinateMapper = _sensor.CoordinateMapper;
	    _bodyFrameReader = _sensor.BodyFrameSource.OpenReader();

        
    }

    public Matrix4x4 Rotate(Vector3 r, Vector3 d)
    {
        float cx, cy, cz, sx, sy, sz;

        sx = Mathf.Sin(r.x);
        cx = Mathf.Cos(r.x);
        sy = Mathf.Sin(r.y);
        cy = Mathf.Cos(r.y);
        sz = Mathf.Sin(r.z);
        cz = Mathf.Cos(r.z);

        var matrix = new Matrix4x4();
        matrix.SetRow(0, new UnityEngine.Vector4(cy * cz, -cy * sz, sy, d.x));
        matrix.SetRow(1, new UnityEngine.Vector4(cx * sz + cz * sx * sy, cx * cz - sx * sy * sz, -cy * sx, d.y));
        matrix.SetRow(2, new UnityEngine.Vector4(sx * sz - cx * cz * sy, cx * sy * sz + cz * sx, cx * cy, d.z));
        matrix.SetRow(3, new UnityEngine.Vector4( 0, 0, 0, 1));

        return matrix;
    }
	
	// Update is called once per frame
	void Update ()
	{
		if (DepthParticle.GetInstance() == null)
	    {
            Debug.Log("DepthParticle could not be loaded");
	        return;
	    }

		if (Input.GetKeyDown("r"))
		{
			_lastHeadZ = 0;
		}

	    _depthparticle = DepthParticle.GetInstance();
        _scaleOffset = _depthparticle.ScaleOffset;
        _positionOffset = _depthparticle.PositionOffset;

	    using (var frame = _bodyFrameReader.AcquireLatestFrame())
	    {
	        if (frame != null)
	        {
	            _bodies = new Body[frame.BodyFrameSource.BodyCount];
                frame.GetAndRefreshBodyData(_bodies);

	            foreach (var body in _bodies)
	            {
	                if (body != null && body.IsTracked)
	                {
	                    var head = body.Joints[JointType.Head];

	                    var headDepth = _coordinateMapper.MapCameraPointToDepthSpace(head.Position);
	                    var depthPostion = _coordinateMapper.MapCameraPointToDepthSpace(head.Position);

	                    var index = (int) headDepth.X + (int) headDepth.Y*512;

	                    float headZ;

	                    if (index >= _depthparticle.GetDepthData().Length || index < 0)
	                    {
	                        headZ = _lastHeadZ;
	                    }
	                    else
	                    {
                            headZ = _depthparticle.GetDepthData()[index];

                            //filter for abnormal position changes
	                        var zDiff = Math.Abs(headZ - _lastHeadZ);
                            if (zDiff > ZDiffLimit && Math.Abs(_lastHeadZ) > 0)
                            {
                                headZ = _lastHeadZ;
                            }
                            else
                            {
                                _lastHeadZ = headZ;
                            }
	                    }

	                    var headX = 1 - depthPostion.X + 512;
	                    var headY = 1 - depthPostion.Y + 424;

                        var scaledPosition = new Vector3(headX * _scaleOffset.x, headY * _scaleOffset.y, headZ * _scaleOffset.z);


	                    if (_lastRotationPivotVector != _depthparticle.GetReferencePosition() ||
	                        _lastRotationVector != _depthparticle.RotateVector ||
                            _rotationMatrix_x == _emptyMatrix)
	                    {
	                        _lastRotationPivotVector = _depthparticle.GetReferencePosition();
	                        _lastRotationVector = _depthparticle.RotateVector;

                            _rotationMatrix_x = Rotate(new Vector3(_lastRotationVector.x, 0, 0), _lastRotationPivotVector);
                            _rotationMatrix_y = Rotate(new Vector3(0,_lastRotationVector.y, 0), _lastRotationPivotVector);
                            _rotationMatrix_z = Rotate(new Vector3(0, 0, _lastRotationVector.z), _lastRotationPivotVector);
	                    }

                        var rotatedPosition = _rotationMatrix_x.MultiplyPoint3x4(scaledPosition);
	                    rotatedPosition = _rotationMatrix_y.MultiplyPoint3x4(rotatedPosition);
	                    rotatedPosition = _rotationMatrix_z.MultiplyPoint3x4(rotatedPosition);

	                    rotatedPosition.x = rotatedPosition.x + _positionOffset.x + StaticPositionOffset.x + _depthparticle.GetReferencePosition().x;
	                    rotatedPosition.y = rotatedPosition.y + _positionOffset.y + StaticPositionOffset.y + _depthparticle.GetReferencePosition().y;
	                    rotatedPosition.z = rotatedPosition.z + _positionOffset.z + StaticPositionOffset.z + _depthparticle.GetReferencePosition().z;


						var xDiff = Math.Abs(rotatedPosition.x - _lastHeadPosition.x);
						var yDiff = Math.Abs(rotatedPosition.y - _lastHeadPosition.y);
						var zDiff2 = Math.Abs(rotatedPosition.z - _lastHeadPosition.z);

						if(xDiff > minDiffLimit && yDiff > minDiffLimit && zDiff2 > minDiffLimit)
						{

	                        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, rotatedPosition, Smooth * Time.deltaTime);
							_depthparticle.SetHeadPositionPoint(rotatedPosition);
							_lastHeadPosition = rotatedPosition;
						}
	                    //Skip all other Bodies
                        break;
	                }
	            }
	        }
	    }

       
	}
}