﻿
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaCarriage
{
	//public GameObject	coupling;
	public GameObject	carriage;
	public GameObject	bogey1;
	public GameObject	bogey2;
	public Vector3		bogey1Offset = Vector3.zero;
	public Vector3		bogey2Offset = Vector3.zero;
	public Vector3		carriageOffset = Vector3.zero;

	public float		bogeyoff = 0.0f;
	public float		length = 0.0f;
	public Vector3		rot = Vector3.zero;
	public Vector3		bogey1Rot = Vector3.zero;
	public Vector3		bogey2Rot = Vector3.zero;
	//public float		aheadDist;
	public Vector3		b1;
	public Vector3		b2;
	public Vector3		cp;
	public Vector3		bp1;
	public Vector3		bp2;
}

[ExecuteInEditMode]
public class MegaTrainFollow : MonoBehaviour
{
	public MegaShape			path;
	public int					curve = 0;
	public List<MegaCarriage>	carriages = new List<MegaCarriage>();
	public float				distance;
	public float				speed = 0.0f;
	public bool					showrays = false;

	void Update()
	{
		distance += speed * Time.deltaTime;
		if ( path )
		{
			float cdist = distance;

			MegaSpline spline = path.splines[curve];

			// Start at front of train and work backwards, first obj in list is head of train
			for ( int i = 0; i < carriages.Count; i++ )
			{
				float alpha = cdist / spline.length;

				MegaCarriage car = carriages[i];

				float tw = 0.0f;
				float tw1 = 0.0f;

				car.b1 = path.transform.TransformPoint(path.InterpCurve3D(curve, alpha, true, ref tw));

				float d = cdist - car.length;	//bogey2Offset.z;
				float alpha1 = d / spline.length;

				car.b2 = path.transform.TransformPoint(path.InterpCurve3D(curve, alpha1, true, ref tw1));

				car.cp = (car.b1 + car.b2) * 0.5f;

				//if ( showrays )
				//	Debug.DrawLine(b1, b2);

				// Carriage is positioned half way between the bogeys
				if ( car.carriage )
				{
					// Offset can adjust this, so lerp from b1 to b2 based on offset
					//Vector3 cp = (b1 + b2) * 0.5f;

					//float twist = (tw + tw1) * 0.5f;
					//Quaternion trot = Quaternion.Euler(0.0f, twist, 0.0f);
					car.carriage.transform.position = car.cp + car.carriageOffset;
					Quaternion erot = Quaternion.Euler(car.rot);

					Quaternion rot = Quaternion.LookRotation(car.b1 - car.b2);
					car.carriage.transform.rotation = rot * erot;	// * trot;
				}

				if ( car.bogey1 && car.carriage )
				{
					car.bogey1.transform.position = car.carriage.transform.localToWorldMatrix.MultiplyPoint(car.bogey1Offset);	//b1;

					Quaternion erot = Quaternion.Euler(car.bogey1Rot);

					float a = alpha - (car.bogeyoff / spline.length);
					car.bp1 = path.transform.TransformPoint(path.InterpCurve3D(curve, a, true));
					Vector3 p2 = path.transform.TransformPoint(path.InterpCurve3D(curve, a + 0.0001f, true));
					Quaternion rot = Quaternion.LookRotation(p2 - car.bp1);
					car.bogey1.transform.rotation = rot * erot;

					//if ( showrays )
					//	Debug.DrawLine(car.cp, car.bp1, Color.red);
				}

				if ( car.bogey2 && car.carriage )
				{
					car.bogey2.transform.position = car.carriage.transform.localToWorldMatrix.MultiplyPoint(car.bogey2Offset);	//b1;

					Quaternion erot = Quaternion.Euler(car.bogey2Rot);

					//float tw = 0.0f;
					float a = alpha1 + (car.bogeyoff / spline.length);
					car.bp2 = path.transform.TransformPoint(path.InterpCurve3D(curve, a, true, ref tw));
					Vector3 p2 = path.transform.TransformPoint(path.InterpCurve3D(curve, a + 0.0001f, true));
					Quaternion rot = Quaternion.LookRotation(p2 - car.bp2);
					//Quaternion trot = Quaternion.Euler(-tw, 0.0f, 0.0f);
					car.bogey2.transform.rotation = rot * erot;// * trot;

					//if ( showrays )
					//	Debug.DrawLine(cp, p1, Color.blue);
				}

				// Then move back a bit for next car
				cdist -= car.length;
			}
		}
	}
}