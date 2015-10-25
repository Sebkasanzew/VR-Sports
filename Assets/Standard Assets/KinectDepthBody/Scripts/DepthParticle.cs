using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Kinect;
using Assets.KinectDepthBody;
using UnityEngine;

public class DepthParticle : MonoBehaviour {

    private static HashSet<DepthParticle> _instances;

    //particle & shader specific
    public Material Material;
    private ComputeBuffer _particleBuffer;
    private ComputeBuffer _bodyIndexBuffer;
    private ComputeBuffer _colorBuffer;
    private ComputeBuffer _colorSpacePointBuffer;
    private ComputeBuffer _depthComputeBuffer;
    private ComputeBuffer _noiseColorBuffer;

    private int _particleCount;

    public GameObject ReferenceObject;

    //Kinect specifics
    private KinectSensor _sensor;
    public ComputeShader DepthComputeShader;

    public float PointSize = 1f;

    //private CoordinateMapperManager _coordinateMapperManager;

    private KinectDataManager _kinectDataManager;
    private float[] _bodyIndexPoints;

    public int Width = 512;
    public int Height = 424;

    private float[] _depthData;
    private ColorSpacePoint[] _colorSpacePoints;
    private Vector3[] _particleArray;

    public Vector3 PositionOffset;
    public Vector3 ScaleOffset;

    public bool MainParticleRepresentation = false;
    public bool ActivateNoiseColor = false;
    public int DifferentNoiseColorsCount = 32;

    private UnityEngine.Vector4[] _randomColorBuffer;
    private int _frameWidth;
    private int _frameHeight;

    private Texture2D _texture;

    public Vector3 RotateVector = new Vector3(0, 0, 0);

    public bool DevelopmentMode = false;

    private Action KinectDataReceivedCallback;

	private Vector3 _headPositionPoint;
	public float HeadParticleFilterDistance;
	public bool FilterHeadParticle;
	public float zDiffHeadFilter;

    public float[] GetDepthData()
    {
        return _depthData;
    }

    public Vector3 GetReferencePosition()
    {
        return ReferenceObject.transform.position;
    }

	public void SetHeadPositionPoint(Vector3 headPosition)
	{
		_headPositionPoint = headPosition;
	}

	// Use this for initialization
	void Start ()
	{
	    _particleCount = Width*Height;

	    //Init Buffers
        _particleBuffer = new ComputeBuffer(_particleCount, 12);
        _randomColorBuffer = new UnityEngine.Vector4[DifferentNoiseColorsCount];

        //Init Particles
        _particleArray = new Vector3[_particleCount];

        var c = 0;
        for (var i = 0; i < Height; ++i)
        {
            for (var j = 0; j < Width; ++j)
            {
               _particleArray[c++] = new Vector3(j, i);
            }
        }

	    _kinectDataManager = KinectDataManager.GetInstance();
        _depthData = _kinectDataManager.GetDepthPointBuffer();
        _bodyIndexPoints = _kinectDataManager.GetBodyIndexBuffer();
        _texture = _kinectDataManager.GetColorTexture();
        _colorSpacePoints = _kinectDataManager.GetColorSpacePointBuffer();


        Debug.Log((_kinectDataManager != null) + " - " + _depthData.Length);

        //Get Kincet Components
	    _sensor = _kinectDataManager.GetKinectSensor();
        KinectDataReceivedCallback = () =>
        {
            _texture = _kinectDataManager.GetColorTexture();
            _colorSpacePoints = _kinectDataManager.GetColorSpacePointBuffer();
        };

	    _kinectDataManager.DataReceivedAction.Add(KinectDataReceivedCallback);


        _colorSpacePoints = new ColorSpacePoint[_depthData.Length];

        //initialize Buffer
        _bodyIndexBuffer        = new ComputeBuffer(_bodyIndexPoints.Length, sizeof(float));
        _colorSpacePointBuffer  = new ComputeBuffer(_colorSpacePoints.Length, sizeof(float) * 2);
        _colorBuffer            = new ComputeBuffer(_depthData.Length, sizeof(float) * 3);
        _depthComputeBuffer     = new ComputeBuffer(_depthData.Length, sizeof(float));
        _noiseColorBuffer       = new ComputeBuffer(_randomColorBuffer.Length, sizeof(float) * 4);

	    //Set Buffer Data
        _particleBuffer.SetData(_particleArray);

        //Bind Buffer to Shader
        Material.SetBuffer("particleBuffer", _particleBuffer);
        Material.SetBuffer("bodyIndexBuffer", _bodyIndexBuffer);
        Material.SetBuffer("colorBuffer", _colorBuffer);

        DepthComputeShader.SetBuffer(0, "particleBuffer", _particleBuffer);
        DepthComputeShader.SetBuffer(0, "colorBuffer", _colorBuffer);
        DepthComputeShader.SetBuffer(0, "depthBuffer", _depthComputeBuffer);
        DepthComputeShader.SetBuffer(0, "colorSpacePoints", _colorSpacePointBuffer);
        DepthComputeShader.SetBuffer(0, "noiseColorBuffer", _noiseColorBuffer);




        //Setting the Instance for remote Render Calls
	    if (_instances == null)
	    {
	        _instances = new HashSet<DepthParticle>();
	    }
	    _instances.Add(this);

        var frameDesc = _sensor.DepthFrameSource.FrameDescription;
        _frameWidth = frameDesc.Width;
        _frameHeight = frameDesc.Height;

	    

	}

 

    // Update is called once per frame
	void Update ()
	{
        Material.SetVector("ReferencePosition", ReferenceObject.transform.position);
        Material.SetVector("ScaleOffset", ScaleOffset);
        Material.SetVector("PositionOffset", PositionOffset);
        Material.SetVector("RotateVector", RotateVector);
        Material.SetVector("RotationPoint", ReferenceObject.transform.position);
        Material.SetInt("FilterBody", (DevelopmentMode) ? 0 : 1);
        Material.SetFloat("PointSize", PointSize);

        DepthComputeShader.SetInt("activatedNoiseColor", (ActivateNoiseColor) ? 1 : 0);
        DepthComputeShader.SetInt("noiseColorCount", (ActivateNoiseColor) ? 1 : 0);


        //Create Noise Colors
	    if (ActivateNoiseColor)
	    {
	        for (var i = 0; i < _randomColorBuffer.Length; ++i)
	        {
	            _randomColorBuffer[i] = new UnityEngine.Vector4
	            {
	                x = UnityEngine.Random.Range(0f, 1f),
	                y = UnityEngine.Random.Range(0f, 1f),
	                z = UnityEngine.Random.Range(0f, 1f),
	                w = 1
	            };
	        }
	        _noiseColorBuffer.SetData(_randomColorBuffer);
        }

        //Fill ComputeShader with Data
        _particleBuffer.SetData(_particleArray);
        _colorSpacePointBuffer.SetData(_colorSpacePoints);
        _depthComputeBuffer.SetData(_depthData);
        _bodyIndexBuffer.SetData(_bodyIndexPoints);

        DepthComputeShader.SetTexture(0, "res", _texture);

        DepthComputeShader.Dispatch(0, _frameWidth / 8, _frameHeight / 8, 1);
    }



    public void Render()
    {
        // Bind the pass to the pipeline then call a draw (this use the buffer bound in Start() instead of a VBO).
        Material.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, 1, _particleCount);
    }

    public static DepthParticle GetInstance(bool main = true)
    {
        if (_instances == null) _instances = new HashSet<DepthParticle>();
        return main ? _instances.FirstOrDefault(x => x.MainParticleRepresentation) : _instances.FirstOrDefault();
    }

    public static IEnumerable<DepthParticle> GetAllInstances()
    {
        if(_instances == null) _instances = new HashSet<DepthParticle>();
        return _instances;
    }

    private void OnDestroy()
    {
        _instances.Remove(this);
        _kinectDataManager.DataReceivedAction.Remove(KinectDataReceivedCallback);

        // Unity cry if the GPU buffer isn't manually cleaned
        _particleBuffer.Release();
        _bodyIndexBuffer.Release();
        _colorBuffer.Release();
        _colorSpacePointBuffer.Release();
        _depthComputeBuffer.Release();
        _noiseColorBuffer.Release();
    }
}
