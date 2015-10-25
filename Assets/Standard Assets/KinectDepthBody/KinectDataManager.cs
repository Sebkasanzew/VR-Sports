using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Kinect;
using UnityEngine;

namespace Assets.KinectDepthBody
{
    public class KinectDataManager
    {
        private static KinectDataManager _instance;
        private KinectSensor m_pKinectSensor;
        private CoordinateMapper m_pCoordinateMapper;
        private MultiSourceFrameReader m_pMultiSourceFrameReader;
        private DepthSpacePoint[] m_pDepthCoordinates;

        UnityThreading.ActionThread thread;

        private byte[] pColorBuffer;
        private byte[] pBodyIndexBuffer;
        private ColorSpacePoint[] m_pColorSpacePoints;
        private ushort[] pDepthBuffer;

        private float[] floatDepthBuffer;
        private float[] floatBodyIndexBuffer;

        public List<Action> DataReceivedAction = new List<Action>();

        const int        cDepthWidth  = 512;
        const int        cDepthHeight = 424;
        const int        cColorWidth  = 1920;
        const int        cColorHeight = 1080;

        long frameCount = 0;

        double elapsedCounter = 0.0;
        double fps = 0.0;
	
        Texture2D m_pColorRGBX;

        bool nullFrame = false;

        private KinectDataManager()
        {
            pColorBuffer = new byte[cColorWidth * cColorHeight * 4];
            pBodyIndexBuffer = new byte[cDepthWidth * cDepthHeight];
            pDepthBuffer = new ushort[cDepthWidth * cDepthHeight];
            m_pColorSpacePoints = new ColorSpacePoint[pDepthBuffer.Length];
            m_pColorRGBX = new Texture2D (cColorWidth, cColorHeight, TextureFormat.RGBA32, false);
            m_pDepthCoordinates = new DepthSpacePoint[cColorWidth * cColorHeight];

            floatDepthBuffer = new float[pDepthBuffer.Length];
            floatBodyIndexBuffer = new float[pDepthBuffer.Length];

            InitializeDefaultSensor ();

            m_pMultiSourceFrameReader.MultiSourceFrameArrived += m_pMultiSourceFrameReader_MultiSourceFrameArrived;
        }

        public static KinectDataManager GetInstance()
        {
            return _instance ?? (_instance = new KinectDataManager());
        }

        public KinectSensor GetKinectSensor()
        {
            return m_pKinectSensor;
        }

        public Texture2D GetColorTexture()
        {
            return m_pColorRGBX;
        }

        public float[] GetBodyIndexBuffer()
        {
            return floatBodyIndexBuffer;
        }

        public DepthSpacePoint[] GetDepthCoordinates()
        {
            return m_pDepthCoordinates;
        }

        public float[] GetDepthPointBuffer()
        {
            return floatDepthBuffer;
        }

        public ColorSpacePoint[] GetColorSpacePointBuffer()
        {
            return m_pColorSpacePoints;
        }

        void InitializeDefaultSensor()
        {	
            m_pKinectSensor = KinectSensor.GetDefault();
		
            if (m_pKinectSensor != null)
            {
                // Initialize the Kinect and get coordinate mapper and the frame reader
                m_pCoordinateMapper = m_pKinectSensor.CoordinateMapper;
			
                m_pKinectSensor.Open();
                if (m_pKinectSensor.IsOpen)
                {
                    m_pMultiSourceFrameReader = m_pKinectSensor.OpenMultiSourceFrameReader(
                        FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex);
                }
            }
		
            if (m_pKinectSensor == null)
            {
                UnityEngine.Debug.LogError("No ready Kinect found!");
            }
        }

        void ProcessFrame()
        {
            var pDepthData = GCHandle.Alloc(pDepthBuffer, GCHandleType.Pinned);
            var pDepthCoordinatesData = GCHandle.Alloc(m_pDepthCoordinates, GCHandleType.Pinned);
            var pColorData = GCHandle.Alloc(m_pColorSpacePoints, GCHandleType.Pinned);

            m_pCoordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
                pDepthData.AddrOfPinnedObject(), 
                (uint)pDepthBuffer.Length * sizeof(ushort),
                pDepthCoordinatesData.AddrOfPinnedObject(), 
                (uint)m_pDepthCoordinates.Length);

        
            m_pCoordinateMapper.MapDepthFrameToColorSpaceUsingIntPtr(
                pDepthData.AddrOfPinnedObject(),
                pDepthBuffer.Length * sizeof(ushort),
                pColorData.AddrOfPinnedObject(),
                (uint)m_pColorSpacePoints.Length);
        
            pColorData.Free();
            pDepthCoordinatesData.Free();
            pDepthData.Free();

            UnityThreadHelper.Dispatcher.Dispatch(() => 
            {
                m_pColorRGBX.LoadRawTextureData(pColorBuffer);
                m_pColorRGBX.Apply();
            });

            for (var i = 0; i < pDepthBuffer.Length; i++)
            {
                floatDepthBuffer[i] = (float)pDepthBuffer[i];
                floatBodyIndexBuffer[i] = (float)pBodyIndexBuffer[i];
            }
        }

        private void m_pMultiSourceFrameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            thread = UnityThreadHelper.CreateThread(() => GetKinectData(e.FrameReference.AcquireFrame()));
           
        }

        private void GetKinectData(MultiSourceFrame pMultiSourceFrame)
        {
            if (pMultiSourceFrame != null) 
            {
                frameCount++;
                nullFrame = false;

                using(var pDepthFrame = pMultiSourceFrame.DepthFrameReference.AcquireFrame())
                {
                    using(var pColorFrame = pMultiSourceFrame.ColorFrameReference.AcquireFrame())
                    {
                        using(var pBodyIndexFrame = pMultiSourceFrame.BodyIndexFrameReference.AcquireFrame())
                        {
                            // Get Depth Frame Data.
                            if (pDepthFrame != null)
                            {
                                var pDepthData = GCHandle.Alloc (pDepthBuffer, GCHandleType.Pinned);
                                pDepthFrame.CopyFrameDataToIntPtr(pDepthData.AddrOfPinnedObject(), (uint)pDepthBuffer.Length * sizeof(ushort));
                                pDepthData.Free();
                            }
						
                            // Get Color Frame Data
                            if (pColorFrame != null)
                            {
                                var pColorData = GCHandle.Alloc (pColorBuffer, GCHandleType.Pinned);
                                pColorFrame.CopyConvertedFrameDataToIntPtr(pColorData.AddrOfPinnedObject(), (uint)pColorBuffer.Length, ColorImageFormat.Rgba);
                                pColorData.Free();
                            }
                        
                            // Get BodyIndex Frame Data.
                            if (pBodyIndexFrame != null)
                            {
                                var pBodyIndexData = GCHandle.Alloc (pBodyIndexBuffer, GCHandleType.Pinned);
                                pBodyIndexFrame.CopyFrameDataToIntPtr(pBodyIndexData.AddrOfPinnedObject(), (uint)pBodyIndexBuffer.Length);
                                pBodyIndexData.Free();
                            }
                        }
                    }
                }

                ProcessFrame();
                foreach (var callback in DataReceivedAction)
                {
                    callback.Invoke();
                }
            }
            else
            {
                nullFrame = true;
            }
        }



        void OnApplicationQuit()
        {
            pDepthBuffer = null;
            pColorBuffer = null;
            pBodyIndexBuffer = null;

            if (m_pDepthCoordinates != null)
            {
                m_pDepthCoordinates = null;
            }

            if (m_pMultiSourceFrameReader != null)
            {
                m_pMultiSourceFrameReader.Dispose();
                m_pMultiSourceFrameReader = null;
            }
		
            if (m_pKinectSensor != null)
            {
                m_pKinectSensor.Close();
                m_pKinectSensor = null;
            }
        }
    }
}

