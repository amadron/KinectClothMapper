
using UnityEngine;

public class MegaShapeRBodyPath : MonoBehaviour
{
	public MegaShape	target;				// The Shape that will attract the rigid body
	public int			curve = 0;			// The sub curve of that shape usually 0
	public float		force = 1.0f;		// The force that will applied if the rbody is 1 unit away from the curve
	public float		alpha = 0.0f;		// The alpha value to use is usealpha mode set, allows you to set the point on the curve to attract the rbody (0 - 1)
	public bool			usealpha = false;	// Set to true to use alpha value instead of finding the nearest point on the curve.

	Rigidbody rb = null;

	void Update()
	{
		if ( target )
		{
			target.selcurve = curve;
			Vector3 p;

			Vector3 pos = transform.position;

			if ( usealpha )
				p = target.transform.TransformPoint(target.InterpCurve3D(curve, alpha, true));
			else
			{
				Vector3 tangent = Vector3.zero;
				int kt = 0;
				p = target.FindNearestPointWorld(pos, 5, ref kt, ref tangent, ref alpha);
			}

			if ( rb == null )
				rb = GetComponent<Rigidbody>();

			if ( rb )
			{
				Vector3 dir = p - pos;

				rb.AddForce(dir * (force / dir.magnitude));
			}
		}
	}
}