using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using Windows.Kinect;

public class BodySourceView : MonoBehaviour 
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;
    private static KinectSensor _Sensor;
    private static Camera cam;
    public GameObject headObj;
    public GameObject shirtObj;
    private string rigpref = "mixamorig:";

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    

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
        cam = GameObject.Find("OrtoCamera").GetComponent<Camera>();
        
    }


    void Update () 
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
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
              }
                
            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
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


        if (shirt != null)
            foreach (Transform tr in shirt.transform)
            {
                string name = tr.name;
                if(name.Equals(assembName("Neck")))
                {
                    shNeck = tr;
                }
                if(name.Equals(assembName("Hips")))
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
                    shSpine= tr;
                }
                if(name.Equals(assembName("RightHand")))
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
                //jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
            Transform jointTr = jointObj.transform;
            if(jt == Kinect.JointType.Head)
            {
                shHead.parent = jointTr;
                transformLocalPos(shHead);
            }
            else if(jt == Kinect.JointType.SpineShoulder)
            {
                shNeck.parent = jointTr;
                transformLocalPos(shNeck);
            }
            else if(jt == Kinect.JointType.HipLeft)
            {
                shLUpLeg.parent = jointTr;
                transformLocalPos(shLUpLeg);
            }
            else if (jt == Kinect.JointType.HipRight)
            {
                shRUpLeg.parent = jointTr;
                transformLocalPos(shRUpLeg);
            }
            else if (jt == Kinect.JointType.ShoulderLeft)
            {
                shRShould.parent = jointTr;
                transformLocalPos(shRShould);
            }
            else if (jt == Kinect.JointType.ShoulderRight)
            {
                shLShould.parent = jointTr;
                transformLocalPos(shLShould);
            }
            else if(jt == Kinect.JointType.ElbowLeft)
            {
                shRForeArm.parent = jointTr;
                transformLocalPos(shRForeArm);
            }
            else if(jt == Kinect.JointType.ElbowRight)
            {
                shLForeArm.parent = jointTr;
                transformLocalPos(shLForeArm);
            }
            else if (jt == Kinect.JointType.SpineBase)
            {
                shHips.parent = jointTr;
                transformLocalPos(shHips);
            }
            else if(jt == Kinect.JointType.HandLeft)
            {
                shRHand.parent = jointTr;
                transformLocalPos(shRHand);
            }
            else if(jt == Kinect.JointType.HandRight)
            {
                shLHand.parent = jointTr;
                transformLocalPos(shLHand);
            }
            else if(jt == Kinect.JointType.KneeLeft)
            {
                shkneeLeft.parent = jointTr;
                transformLocalPos(shkneeLeft);
            }
            else if(jt == Kinect.JointType.KneeRight)
            {
                shkneeRight.parent = jointTr;
                transformLocalPos(shkneeRight);
            }
            else if(jt == Kinect.JointType.AnkleLeft)
            {
                shFootLeft.parent = jointTr;
                transformLocalPos(shFootLeft);
            }
            else if(jt == Kinect.JointType.AnkleRight)
            {
                shFootRight.parent = jointTr;
                transformLocalPos(shFootRight);
            }
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }
        
        return body;
    }
    
    private void transformLocalPos(Transform tr)
    {
        Vector3 vec = tr.localPosition;
        vec.y = 0;
        tr.localPosition = Vector3.zero;
    }

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        GameObject leftShoulder = null;
        GameObject rightShoulder = null;
        GameObject leftElbow = null;
        GameObject rightElbow = null;
        GameObject hipLeft = null;
        GameObject hipRight = null;
        GameObject spineBase = null;
        foreach(Transform tr in bodyObject.transform)
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
        }

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            /*
            if(jt == Kinect.JointType.SpineBase)
            {
                Vector3 pos = bodyObject.transform.position;
                pos.z = -sourceJoint.Position.Z;
                bodyObject.transform.position = pos;
            }
            */
            

            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            /*
            Vector3 pos = mapToColor(targetJoint.Value);
            Debug.Log("Pos: " + pos);
            pos.z = targetJoint.Value.Position.Z;
            //pos.z = 0;
            pos.x *= viewFactorX;
            pos.y *= viewFactorY;

            pos = cam.ScreenToViewportPoint(pos);
            jointObj.localPosition = pos; */

            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            Quaternion rotation = ConvertJointQuaternionToUnityQuaterion(body, jt, false);
            if (jt == Kinect.JointType.Head)
                rotation = ConvertJointQuaternionToUnityQuaterion(body, JointType.Neck, false);
            Vector3 rot = rotation.eulerAngles;
            if (jt == Kinect.JointType.HipRight || jt == Kinect.JointType.HipLeft)
                rot.z += 180;
            rotation.eulerAngles = rot;
            jointObj.rotation = rotation;
            

            //LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                
                
                
                //lr.SetPosition(0, GetVector3FromJoint(targetJoint.Value));
                //lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                //lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                //lr.enabled = false;
            }
        }
            Transform childL = leftShoulder.transform.GetChild(0);
            Transform childLchild = childL.GetChild(0);
            Transform childR = rightShoulder.transform.GetChild(0);
            Transform childRchild = childR.GetChild(0);
        //childLchild.LookAt(leftElbow.transform);
        //childRchild.LookAt(rightElbow.transform);
        Vector3 vecL = leftShoulder.transform.position - leftElbow.transform.position;
        Quaternion lRot = Quaternion.LookRotation(vecL);
        lRot.eulerAngles = new Vector3(360 - lRot.eulerAngles.x + 90, lRot.eulerAngles.y + 180, lRot.eulerAngles.z);
        childLchild.rotation = lRot;
        Vector3 vecR = rightShoulder.transform.position - rightElbow.transform.position;
        Quaternion rRot = Quaternion.LookRotation(vecR);
        rRot.eulerAngles = new Vector3(360  - rRot.eulerAngles.x + 90, rRot.eulerAngles.y + 180, rRot.eulerAngles.z);
        childRchild.rotation = rRot;
        Vector3 upWidthVec = leftShoulder.transform.position - rightShoulder.transform.position;
        float upWidth = upWidthVec.magnitude;
        Vector3 bottomWidthVec = hipLeft.transform.position - hipRight.transform.position;
        float bottomWidth = bottomWidthVec.magnitude;
        Vector3 spineScale = spineBase.transform.localScale;
        spineScale.x = bottomWidth;
        spineBase.transform.localScale = spineScale;
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
    
    private static Vector3 mapToColor(Kinect.Joint joint)
    {
        ColorSpacePoint colorPoint = _Sensor.CoordinateMapper.MapCameraPointToColorSpace(joint.Position);
        return new Vector3(colorPoint.X, colorPoint.Y);
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
