

using UnityEngine;

[AddComponentMenu("Modifiers/Warp/FFD 2x2x2")]
public class MegaFFD2x2x2Warp : MegaFFDWarp
{
	public override string WarpName() { return "FFD2x2x2"; }

	public override int GridSize()
	{
		return 2;
	}

	public override Vector3 Map(int ii, Vector3 p)
	{
		Vector3 q = Vector3.zero;

		Vector3 pp = tm.MultiplyPoint3x4(p);

		if ( inVol )
		{
			for ( int i = 0; i < 3; i++ )
			{
				if ( pp[i] < -EPSILON || pp[i] > 1.0f + EPSILON )
					return p;
			}

			//if ( pp.x < -hw || pp.x > hw || pp.y < -hh || pp.y > hh || pp.z < -hl || pp.z > hl )
			//if ( pp.x < 0.0f || pp.x > Width || pp.y < 0.0f || pp.y > Height || pp.z < 0.0f || pp.z > Length )
				//return p;
		}

		Vector3 ipp = pp;
		float dist = pp.magnitude;
		float dcy = Mathf.Exp(-totaldecay * Mathf.Abs(dist));

		float ip, jp, kp;
		for ( int i = 0; i < 2; i++ )
		{
			ip = i == 0 ? 1.0f - pp.x : pp.x;

			for ( int j = 0; j < 2; j++ )
			{
				jp = ip * (j == 0 ? 1.0f - pp.y : pp.y);

				for ( int k = 0; k < 2; k++ )
				{
					kp = jp * (k == 0 ? 1.0f - pp.z : pp.z);

					int ix = (i * 4) + (j * 2) + k;
					q.x += pt[ix].x * kp;
					q.y += pt[ix].y * kp;
					q.z += pt[ix].z * kp;
				}
			}
		}

		q = Vector3.Lerp(ipp, q, dcy);

		return invtm.MultiplyPoint3x4(q);
	}

	public override int GridIndex(int i, int j, int k)
	{
		return (i * 4) + (j * 2) + k;
	}
}