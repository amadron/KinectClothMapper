using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using Windows.Kinect;
namespace JointOrientationBasics
{
    public class BodySourceView : MonoBehaviour
    {
        public Material BoneMaterial;
        public BodySourceManager BodySourceManager;
        private static KinectSensor _Sensor;
        private static Camera cam;
        public GameObject headObj;
        public GameObject shirtObj;
        public string rigpref = "mixamorig_";
        public bool MirrorJoints;
        private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
        private BodySourceManager _BodyManager;
        private static float resFactorY;
        private static float resFactorX;

        private DoubleExponentialFilter jointSmoother;
        private DoubleExponentialFilter.TRANSFORM_SMOOTH_PARAMETERS smoothingParams;
        private DoubleExponentialFilter.Joint filteredJoint;

        private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
        {
            { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
            { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
            { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
            { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

            { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
            { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
            { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
            { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

            { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
            { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
            { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
            { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
            { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
            { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

            { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
            { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
            { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
            { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
            { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
            { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

            { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
            { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
            { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
            { Kinect.JointType.Neck, Kinect.JointType.Head },
        };

        private void Start()
        {
            _Sensor = Kinect.KinectSensor.GetDefault();
            if (null == this.BodySourceManager)
            {
                BodySourceManager[] objs = FindObjectsOfType(typeof(BodySourceManager)) as BodySourceManager[];
                if (objs.Length > 0)
                {
                    this.BodySourceManager = objs[0];
                }
            }
            //cam = GameObject.Find("OrtoCamera").GetComponent<Camera>();
            cam = GameObject.Find("Main Camera").GetComponent<Camera>();
            resFactorY = (float)Screen.height / 1080;
            resFactorX = (float)Screen.width / 1920;
        }


        void Update()
        {
            if (BodySourceManager == null)
            {
                return;
            }

            _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
            if (_BodyManager == null)
            {
                return;
            }

            Kinect.Body[] data = _BodyManager.Bodies;
            if (data == null)
            {
                return;
            }

            List<ulong> trackedIds = new List<ulong>();
            foreach (var body in data)
            {
                if (body == null)
                {
                    continue;
                }

                if (body.IsTracked)
                {
                    trackedIds.Add(body.TrackingId);
                }
            }

            List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

            // First delete untracked bodies
            foreach (ulong trackingId in knownIds)
            {
                if (!trackedIds.Contains(trackingId))
                {
                    Destroy(_Bodies[trackingId]);
                    _Bodies.Remove(trackingId);
                }
            }

            foreach (var body in data)
            {
                if (body == null)
                {
                    continue;
                }

                if (body.IsTracked)
                {
                    if (!_Bodies.ContainsKey(body.TrackingId))
                    {
                        _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                    }

                    RefreshBodyObject(body, _Bodies[body.TrackingId]);
                }
            }
        }

        string assembName(string name)
        {
            return rigpref + name;
        }

        private GameObject CreateBodyObject(ulong id)
        {
            GameObject body = new GameObject("Body:" + id);
            GameObject shirt = Instantiate<GameObject>(shirtObj);
            shirt.transform.parent = body.transform;
            shirt.transform.localPosition = Vector3.zero;
            shirt.SetActive(true);
            Transform shNeck = null;
            Transform shHips = null;
            Transform shSpine = null;
            Transform shLUpLeg = null;
            Transform shRUpLeg = null;
            Transform shLArm = null;
            Transform shRArm = null;
            Transform shLShould = null;
            Transform shRShould = null;
            Transform shSp1 = null;
            Transform shSp2 = null;
            Transform shRForeArm = null;
            Transform shLForeArm = null;
            Transform shLHand = null;
            Transform shRHand = null;
            Transform shHead = null;
            Transform shkneeRight = null;
            Transform shkneeLeft = null;
            Transform shAnkleLeft = null;
            Transform shAnkleRight = null;
            Transform shFootLeft = null;
            Transform shFootRight = null;
            Transform shHeadEnd = null;
            Transform shHandTipRight = null;
            Transform shHandTipLeft = null;

            if (shirt != null)
                foreach (Transform tr in shirt.transform)
                {
                    string name = tr.name;
                    if (name.Equals(assembName("Neck")))
                    {
                        shNeck = tr;
                    }
                    if (name.Equals(assembName("Hips")))
                    {
                        shHips = tr;
                    }
                    if (name.Equals(assembName("LeftArm")))
                    {
                        shLArm = tr;
                    }
                    if (name.Equals(assembName("LeftShoulder")))
                    {
                        shLShould = tr;
                    }
                    if (name.Equals(assembName("RightShoulder")))
                    {
                        shRShould = tr;
                    }
                    if (name.Equals(assembName("RightArm")))
                    {
                        shRArm = tr;
                    }
                    if (name.Equals(assembName("LeftUpLeg")))
                    {
                        shLUpLeg = tr;
                    }
                    if (name.Equals(assembName("RightUpLeg")))
                    {
                        shRUpLeg = tr;
                    }
                    if (name.Equals(assembName("Spine1")))
                    {
                        shSp1 = tr;
                    }
                    if (name.Equals(assembName("Spine2")))
                    {
                        shSp2 = tr;
                    }
                    if (name.Equals(assembName("Spine")))
                    {
                        shSpine = tr;
                    }
                    if (name.Equals(assembName("RightHand")))
                    {
                        shRHand = tr;
                    }
                    if (name.Equals(assembName("LeftHand")))
                    {
                        shLHand = tr;
                    }
                    if (name.Equals(assembName("RightForeArm")))
                    {
                        shRForeArm = tr;
                    }
                    if (name.Equals(assembName("LeftForeArm")))
                    {
                        shLForeArm = tr;
                    }
                    if (name.Equals(assembName("Head")))
                    {
                        shHead = tr;
                        foreach (Transform trCh in tr)
                        {


                        }
                    }
                    if (name.Equals(assembName("LeftLeg")))
                    {
                        shkneeLeft = tr;
                    }
                    if (name.Equals(assembName("RightLeg")))
                    {
                        shkneeRight = tr;
                    }
                    if (name.Equals(assembName("RightFoot")))
                    {
                        shFootRight = tr;
                    }
                    if (name.Equals(assembName("LeftFoot")))
                    {
                        shFootLeft = tr;
                    }
                }
            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            {
                GameObject jointObj;

                if (jt == Kinect.JointType.Head)
                {
                    jointObj = Instantiate<GameObject>(headObj);
                    Vector3 pos = Vector3.zero;
                    jointObj.transform.localPosition = Vector3.zero;
                    jointObj.SetActive(true);
                }
                else
                {
                    jointObj = new GameObject();
                    //jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //jointObj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                }
                Transform jointTr = jointObj.transform;
                Transform mapTr = null;
                if (jt == Kinect.JointType.Neck)
                {
                    mapTr = shNeck;
                    //mapTr = shHead;
                }
                else if (jt == Kinect.JointType.SpineShoulder)
                {
                    mapTr = shSp2;
                }
                else if (jt == Kinect.JointType.Head)
                {
                    // mapTr = shHead;
                }
                else if (jt == Kinect.JointType.HipLeft)
                {
                    mapTr = shLUpLeg;
                }
                else if (jt == Kinect.JointType.HipRight)
                {
                    mapTr = shRUpLeg;
                }
                else if (jt == Kinect.JointType.ShoulderLeft)
                {
                    mapTr = shRShould;
                }
                else if (jt == Kinect.JointType.ShoulderRight)
                {
                    mapTr = shLShould;
                }
                else if (jt == Kinect.JointType.ElbowLeft)
                {
                    mapTr = shRForeArm;
                }
                else if (jt == Kinect.JointType.ElbowRight)
                {
                    mapTr = shLForeArm;
                }
                else if (jt == Kinect.JointType.SpineBase)
                {
                    mapTr = shHips;
                }
                else if (jt == Kinect.JointType.HandLeft)
                {
                    mapTr = shRHand;
                }
                else if (jt == Kinect.JointType.HandRight)
                {
                    mapTr = shLHand;
                }
                else if (jt == Kinect.JointType.KneeLeft)
                {
                    mapTr = shkneeLeft;
                }
                else if (jt == Kinect.JointType.KneeRight)
                {
                    mapTr = shkneeRight;
                }
                else if (jt == Kinect.JointType.FootRight)
                {
                    mapTr = shFootRight;
                }
                else if (jt == Kinect.JointType.FootLeft)
                {
                    mapTr = shFootLeft;
                }

                if (mapTr != null)
                {
                    mapTr.parent = jointTr;
                    transformLocalPos(mapTr);
                }

                jointObj.name = jt.ToString();
                jointObj.transform.parent = body.transform;
            }

            return body;
        }

        private void transformLocalPos(Transform tr)
        {
            Vector3 rot = Vector3.zero;
            tr.localRotation = Quaternion.Euler(rot);
            Vector3 vec = tr.localPosition;
            vec.y = 0;
            tr.localPosition = Vector3.zero;
        }

        public Vector3 RForeArmOffset = new Vector3(90, 180, 0);
        public Vector3 LForeArmOffset = new Vector3(90, 180, 0);
        public Vector3 RShoulderOffset = Vector3.zero;
        public Vector3 LShoulderOffset = Vector3.zero;
        private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
        {
            GameObject leftShoulder = null;
            GameObject rightShoulder = null;
            GameObject leftElbow = null;
            GameObject rightElbow = null;
            GameObject hipLeft = null;
            GameObject hipRight = null;
            GameObject spineBase = null;
            GameObject leftKnee = null;
            GameObject rightKnee = null;
            GameObject handLeft = null;
            GameObject handRight = null;
            GameObject handTipLeft = null;
            GameObject handTipRight = null;
            foreach (Transform tr in bodyObject.transform)
            {
                if (tr.name.Equals("ShoulderLeft"))
                    leftShoulder = tr.gameObject;
                if (tr.name.Equals("ElbowLeft"))
                    leftElbow = tr.gameObject;
                if (tr.name.Equals("ShoulderRight"))
                    rightShoulder = tr.gameObject;
                if (tr.name.Equals("ElbowRight"))
                    rightElbow = tr.gameObject;
                if (tr.name.Equals("HipLeft"))
                    hipLeft = tr.gameObject;
                if (tr.name.Equals("HipRight"))
                    hipRight = tr.gameObject;
                if (tr.name.Equals("SpineBase"))
                    spineBase = tr.gameObject;
                if (tr.name.Equals("KneeLeft"))
                    leftKnee = tr.gameObject;
                if (tr.name.Equals("KneeRight"))
                    rightKnee = tr.gameObject;
                if (tr.name.Equals("HandLeft"))
                    handLeft = tr.gameObject;
                if (tr.name.Equals("HandRight"))
                    handRight = tr.gameObject;
                
            }

            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            {
                Kinect.Joint sourceJoint = body.Joints[jt];
                Kinect.Joint? targetJoint = null;



                if (_BoneMap.ContainsKey(jt))
                {
                    targetJoint = body.Joints[_BoneMap[jt]];
                }
                Transform jointObj = bodyObject.transform.Find(jt.ToString());

                //jointObj.localPosition = GetVector3FromJoint(sourceJoint);
                jointObj.localPosition = MapToColor(sourceJoint);
                Quaternion rotation = ConvertJointQuaternionToUnityQuaterion(body, jt, false);
                if (jt == Kinect.JointType.Head)
                    rotation = ConvertJointQuaternionToUnityQuaterion(body, JointType.Neck, false);
                Vector3 rot = rotation.eulerAngles;

                rotation.eulerAngles = rot;
                jointObj.rotation = rotation;
            }
            //Scale Shirt

            Vector3 upWidthVec = leftShoulder.transform.position - rightShoulder.transform.position;
            float upWidth = upWidthVec.magnitude;
            Vector3 bottomWidthVec = hipLeft.transform.position - hipRight.transform.position;
            float bottomWidth = bottomWidthVec.magnitude;
            Vector3 spineScale = spineBase.transform.localScale;
            spineScale.x = bottomWidth * 0.65f;
            spineBase.transform.localScale = spineScale;

        }

        internal static Vector3 ConvertJointPositionToUnityVector3(Kinect.Body body, Kinect.JointType type, bool mirror = true)
        {
            Vector3 position = new Vector3(body.Joints[type].Position.X,
                body.Joints[type].Position.Y,
                body.Joints[type].Position.Z);

            // translate -x
            if (mirror)
            {
                position.x *= -1;
            }
            return position;
        }

        Quaternion getLookRotation(Vector3 vec, Vector3 offset)
        {
            Quaternion lookRot = Quaternion.LookRotation(vec);
            lookRot.eulerAngles = new Vector3(lookRot.eulerAngles.x + offset.x, lookRot.eulerAngles.y + offset.y, lookRot.eulerAngles.z + offset.z);
            return lookRot;
        }

        static Quaternion ConvertJointQuaternionToUnityQuaterion(Kinect.Body body, Kinect.JointType jt, bool mirror = true)
        {
            Quaternion rotation = new Quaternion(body.JointOrientations[jt].Orientation.X,
                body.JointOrientations[jt].Orientation.Y,
                body.JointOrientations[jt].Orientation.Z,
                body.JointOrientations[jt].Orientation.W);

            // flip rotation
            if (mirror)
            {
                rotation = new Quaternion(rotation.x, -rotation.y, -rotation.z, rotation.w);
            }

            return rotation;
        }


        private static Color GetColorForState(Kinect.TrackingState state)
        {
            switch (state)
            {
                case Kinect.TrackingState.Tracked:
                    return Color.green;

                case Kinect.TrackingState.Inferred:
                    return Color.red;

                default:
                    return Color.black;
            }
        }

        public static Vector3 MapToColor(Kinect.Joint joint)
        {
            //Joint Pos to Color Coordinates
            ColorSpacePoint colorPoint = _Sensor.CoordinateMapper.MapCameraPointToColorSpace(joint.Position);
            if (float.IsInfinity(colorPoint.X))
            {
                colorPoint.X = 0;
            }
            if (float.IsInfinity(colorPoint.Y))
            {
                colorPoint.Y = 0;
            }

            Vector3 vec = new Vector3(colorPoint.X, colorPoint.Y, 0);
            //Color Coordinates to actual Screensize/ Pos
            vec.x *= resFactorX;
            vec.y *= resFactorY;
            //to do: actual ScreenPos to Viewport Pos
            vec.x = vec.x / Screen.width;
            vec.y = 1 - vec.y / Screen.height;
            // Viewport Pos to 3d Space
            vec.z = joint.Position.Z * 10;
            vec = cam.ViewportToWorldPoint(vec);
            return vec;
        }

        private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
        {
            return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
        }
    }
}
