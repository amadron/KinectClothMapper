
using UnityEngine;
using System.Collections.Generic;

// We only need do vertical movement for first draft
[AddComponentMenu("Modifiers/Rope Deform")]
public class MegaRopeDeform : MegaModifier
{
	public override string ModName()	{ return "RopeDeform"; }
	public override string GetHelpURL() { return "?page_id=1524"; }

	public float			floorOff		= 0.0f;	// floor offset
	public int				NumMasses		= 8;		// masses
	public MegaSoft2D		soft			= new MegaSoft2D();
	public float			timeStep		= 0.01f;
	public float			Mass			= 10.0f;	// Mass of system
	public MegaAxis			axis			= MegaAxis.Z;
	public AnimationCurve	stiffnessCrv	= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public float			stiffspring		= 1.0f;
	public float			stiffdamp		= 0.1f;
	public float			spring			= 1.0f;
	public float			damp			= 1.0f;
	public float			off				= 0.0f;
	public bool				init			= false;
	public float			SpringCompress	= 1.0f;
	public bool				BendSprings		= true;
	public bool				Constraints		= true;
	public float			DampingRatio	= 0.5f;
	public int				pconl;
	public int				pconr;
	public bool				DisplayDebug	= true;
	public int				drawsteps		= 20;
	public float			boxsize			= 0.01f;

	public Transform		left;
	public Transform		right;
	public float			weight			= 0.0f;
	public float			weightPos		= 0.5f;
	public Vector2[]		masspos;

	int		ax;
	float	minx;
	float	width;

	// We could add a rotate just lerp a normal etc
	// y is minor axis
	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		// We could precalc this
		float alpha = (p[ax] - minx) / width;

		// Cubic from rope for this
		Vector2 y = Interp1a(alpha);	//masses[m].p.y + ((masses[m + 1].p.y - masses[m].p.y) * a);
		p.y += y.y + (off * 0.01f);
		p[ax] = y.x;

		return invtm.MultiplyPoint3x4(p);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		ax = (int)axis;
		minx = bbox.min[ax];
		width = bbox.max[ax] - bbox.min[ax];

		if ( init || NumMasses != soft.masses.Count )
		{
			init = false;
			Init();
		}

		AddWeight();
		UpdateRope();
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		return true;
	}

	public void Build(MegaModContext mc)
	{
	}

	// Do physics

	public void UpdateRope()
	{
		if ( soft != null )
		{
			soft.Update();

			for ( int i = 0; i < soft.masses.Count; i++ )
			{
				masspos[i + 1] = soft.masses[i].pos;

				soft.masses[i].forcec = Vector2.zero;
			}

			masspos[0] = soft.masses[0].pos - (soft.masses[1].pos - soft.masses[0].pos);
			masspos[masspos.Length - 1] = soft.masses[soft.masses.Count - 1].pos + (soft.masses[soft.masses.Count - 1].pos - soft.masses[soft.masses.Count - 2].pos);

			if ( left != null )	//&& pconl != null )
			{
				Vector3 p = transform.worldToLocalMatrix.MultiplyPoint(left.position);
				soft.constraints[pconl].pos.x = p[ax];
				soft.constraints[pconl].pos.y = p.y;
			}

			if ( right != null )	//&& pconr != null )
			{
				Vector3 p = transform.worldToLocalMatrix.MultiplyPoint(right.position);
				soft.constraints[pconr].pos.x = p[ax];
				soft.constraints[pconr].pos.y = p.y;
			}
		}
	}

	public void Init()
	{
		if ( soft.masses == null )
			soft.masses = new List<Mass2D>();

		soft.masses.Clear();
		float ms = Mass / (float)(NumMasses);

		int ax = (int)axis;

		Vector2 pos = Vector2.zero;

		damp = (DampingRatio * 0.45f) * (2.0f * Mathf.Sqrt(ms * spring));

		for ( int i = 0; i < NumMasses; i++ )
		{
			float alpha = (float)i / (float)(NumMasses - 1);

			pos.x = Mathf.Lerp(bbox.min[ax], bbox.max[ax], alpha);

			Mass2D rm = new Mass2D(ms, pos);
			soft.masses.Add(rm);
		}

		masspos = new Vector2[soft.masses.Count + 2];

		for ( int i = 0; i < soft.masses.Count; i++ )
			masspos[i + 1] = soft.masses[i].pos;

		if ( soft.springs == null )
			soft.springs = new List<Spring2D>();

		soft.springs.Clear();

		if ( soft.constraints == null )
			soft.constraints = new List<Constraint2D>();

		soft.constraints.Clear();

		for ( int i = 0; i < soft.masses.Count - 1; i++ )
		{
			Spring2D spr = new Spring2D(i, i + 1, spring, damp, soft);

			//float len = spr.restLen;
			spr.restLen *= SpringCompress;
			soft.springs.Add(spr);

			if ( Constraints )
			{
				// Do we use restLen or len here?
				Constraint2D lcon = Constraint2D.CreateLenCon(i, i + 1, spr.restLen);
				soft.constraints.Add(lcon);
			}
		}

		if ( BendSprings )
		{
			int gap = 2;
			for ( int i = 0; i < soft.masses.Count - gap; i++ )
			{
				float alpha = (float)i / (float)soft.masses.Count;
				Spring2D spr = new Spring2D(i, i + gap, stiffspring * stiffnessCrv.Evaluate(alpha), stiffdamp * stiffnessCrv.Evaluate(alpha), soft);
				soft.springs.Add(spr);

				Constraint2D lcon = Constraint2D.CreateLenCon(i, i + gap, spr.restLen);
				soft.constraints.Add(lcon);
			}
		}
		// Apply fixed end constraints
		Constraint2D pcon;

		pos.x = bbox.min[ax];
		pos.y = 0.0f;
		pcon = Constraint2D.CreatePointCon(0, pos);
		pconl = soft.constraints.Count;
		soft.constraints.Add(pcon);

		pos.x = bbox.max[ax];
		pcon = Constraint2D.CreatePointCon(soft.masses.Count - 1, pos);
		pconr = soft.constraints.Count;
		soft.constraints.Add(pcon);

		soft.DoConstraints();
	}

	void DrawSpline(int steps)	//, float t)
	{
		if ( soft.masses != null && soft.masses.Count != 0 )
		{
			Vector3 prevPt = Interp1a(0.0f);

			if ( ax == 2 )
			{
				float x = prevPt.x;
				prevPt.x = prevPt.z;
				prevPt.z = x;
			}

			for ( int i = 1; i <= steps; i++ )
			{
				if ( (i & 1) == 1 )
					Gizmos.color = Color.white;
				else
					Gizmos.color = Color.black;

				float pm = (float)i / (float)steps;

				Vector3 currPt = Interp1a(pm);

				if ( ax == 2 )
				{
					float x = currPt.x;
					currPt.x = currPt.z;
					currPt.z = x;
				}
				Gizmos.DrawLine(prevPt, currPt);
				prevPt = currPt;
			}
		}
	}

	public void OnDrawGizmos()
	{
		Display();
	}

	// Mmm should be in gizmo code
	void Display()
	{
		Gizmos.matrix = transform.localToWorldMatrix;

		if ( DisplayDebug && soft != null && soft.masses != null )
		{
			DrawSpline(drawsteps);	//, vel * 0.0f);

			Vector3 p = Vector3.zero;

			Gizmos.color = Color.yellow;

			for ( int i = 0; i < soft.masses.Count; i++ )
			{
				if ( ax == 0 )
				{
					p.x = soft.masses[i].pos.x;
					p.y = soft.masses[i].pos.y;	// + (off * 0.01f);
					p.z = 0.0f;
				}
				else
				{
					p.z = soft.masses[i].pos.x;
					p.y = soft.masses[i].pos.y;	// + (off * 0.01f);
					p.x = 0.0f;
				}
				Gizmos.DrawCube(p, Vector3.one * boxsize * 0.1f);
			}

			if ( weightPos >= 0.0f && weightPos < 100.0f )
			{
				Gizmos.color = Color.blue;
				Vector2 pos = Interp1a(weightPos * 0.01f);

				if ( ax == 0 )
				{
					p.x = pos.x;
					p.y = pos.y;	// + (off * 0.01f);
					p.z = 0.0f;
				}
				else
				{
					p.z = pos.x;
					p.y = pos.y;	// + (off * 0.01f);
					p.x = 0.0f;
				}

				Gizmos.DrawCube(p, Vector3.one * boxsize * 0.2f);
			}
		}
		Gizmos.matrix = Matrix4x4.identity;
	}

	// Spline interp etc
	public Vector2 Interp1(float t)
	{
		int numSections = soft.masses.Count - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
		float u = t * (float)numSections - (float)currPt;

		Vector2 a = soft.masses[currPt].pos;
		Vector2 b = soft.masses[currPt + 1].pos;
		Vector2 c = soft.masses[currPt + 2].pos;
		Vector2 d = soft.masses[currPt + 3].pos;

		return 0.5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) + (-a + c) * u + 2f * b);
	}

	// Need to build coefs after sim then this becomes faster
	public Vector2 Interp1a(float t)
	{
		int numSections = masspos.Length - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
		float u = t * (float)numSections - (float)currPt;

		Vector2 a = masspos[currPt];
		Vector2 b = masspos[currPt + 1];
		Vector2 c = masspos[currPt + 2];
		Vector2 d = masspos[currPt + 3];

		return 0.5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) + (-a + c) * u + 2f * b);
	}

	void AddWeight()
	{
		if ( weightPos >= 0.0f && weightPos < 100.0f )
		{
			float num = (float)(soft.masses.Count - 1);
			int m1 = (int)(num * weightPos * 0.01f);
			int m2 = m1 + 1;

			float alpha = ((float)num * weightPos * 0.01f) - (float)m1;

			Vector3 frc = Vector2.zero;

			frc.y = weight * (1.0f - alpha);
			soft.masses[m1].forcec = frc;
			frc.y = weight * alpha;
			soft.masses[m2].forcec = frc;
		}
	}

	public float GetPos(float alpha)
	{
		Vector2 p = Interp1a(alpha);
		return p.y;
	}

	public Vector2 GetPos2(float alpha)
	{
		return Interp1a(alpha);
	}

	public Vector2 GetPos3(float v)
	{
		for ( int i = 1; i < masspos.Length - 1; i++ )
		{
			if ( v > masspos[i].x && v < masspos[i + 1].x )
			{
				float u = (v - masspos[i].x) / (masspos[i + 1].x - masspos[i].x);
				Vector2 a = masspos[i - 1];
				Vector2 b = masspos[i];
				Vector2 c = masspos[i + 1];
				Vector2 d = masspos[i + 2];

				return 0.5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) + (-a + c) * u + 2f * b);
			}
		}

		return Vector2.zero;
	}

	public Vector2 SetWeight(float v, float weight)
	{
		for ( int i = 1; i < masspos.Length - 2; i++ )
		{
			if ( v > masspos[i].x && v < masspos[i + 1].x )
			{
				float u = (v - masspos[i].x) / (masspos[i + 1].x - masspos[i].x);
				Vector2 a = masspos[i - 1];
				Vector2 b = masspos[i];
				Vector2 c = masspos[i + 1];
				Vector2 d = masspos[i + 2];

				Vector2 frc = Vector2.zero;
				frc.y = weight * (1.0f - u);

				soft.masses[i - 1].forcec = frc;

				frc.y = weight * u;
				soft.masses[i].forcec = frc;

				return 0.5f * ((-a + 3.0f * b - 3.0f * c + d) * (u * u * u) + (2.0f * a - 5.0f * b + 4.0f * c - d) * (u * u) + (-a + c) * u + 2.0f * b);
			}
		}

		return Vector2.zero;
	}
}

static class AABB_Triangle_Intersection
{
	static void FINDMINMAX(float x0, float x1, float x2, out float min, out float max)
	{
		min = max = x0;

		if ( x1 < min ) min = x1;
		if ( x1 > max ) max = x1;
		if ( x2 < min ) min = x2;
		if ( x2 > max ) max = x2;
	}

	static bool planeBoxOverlap(Vector3 normal, Vector3 vert, Vector3 maxbox)
	{
		Vector3 vmin, vmax;

		float v = vert.x;
		if ( normal.x > 0.0f )
		{
			vmin.x = -maxbox.x - v;
			vmax.x = maxbox.x - v;
		}
		else
		{
			vmin.x = maxbox.x - v;
			vmax.x = -maxbox.x - v;
		}

		v = vert.y;
		if ( normal.y > 0.0f )
		{
			vmin.y = -maxbox.y - v;
			vmax.y = maxbox.y - v;
		}
		else
		{
			vmin.y = maxbox.y - v;
			vmax.y = -maxbox.y - v;
		}

		v = vert.z;
		if ( normal.z > 0.0f )
		{
			vmin.z = -maxbox.z - v;
			vmax.z = maxbox.z - v;
		}
		else
		{
			vmin.z = maxbox.z - v;
			vmax.z = -maxbox.z - v;
		}

		if ( Vector3.Dot(normal, vmin) > 0.0f ) return false;

		if ( Vector3.Dot(normal, vmax) >= 0.0f ) return true;

		return false;
	}

	public static bool TriangleBoxOverlap(Vector3 A, Vector3 B, Vector3 C, Bounds Box)
	{
		return triBoxOverlap(Box.center, Box.extents, new Vector3[] { A, B, C });
	}

	static bool triBoxOverlap(Vector3 boxcenter, Vector3 boxhalfsize, Vector3[] triverts)
	{
		float min, max, p0, p1, p2, rad;

		Vector3 v0 = triverts[0] - boxcenter;
		Vector3 v1 = triverts[1] - boxcenter;
		Vector3 v2 = triverts[2] - boxcenter;

		Vector3 e0 = v1 - v0;
		Vector3 e1 = v2 - v1;
		Vector3 e2 = v0 - v2;

		float fex = Mathf.Abs(e0.x);
		float fey = Mathf.Abs(e0.y);
		float fez = Mathf.Abs(e0.z);

		#region AXISTEST_X01(e0.z, e0.y, fez, fey);
		{
			p0 = e0.z * v0.y - e0.y * v0.z;

			p2 = e0.z * v2.y - e0.y * v2.z;

			if ( p0 < p2 ) { min = p0; max = p2; } else { min = p2; max = p0; }

			rad = fez * boxhalfsize.y + fey * boxhalfsize.z;

			if ( min > rad || max < -rad ) return false;
		}
		#endregion
		//Debug.Log("post axis");

		#region AXISTEST_Y02(e0.z, e0.x, fez, fex);
		{
			p0 = -e0.z * v0.x + e0.x * v0.z;

			p2 = -e0.z * v2.x + e0.x * v2.z;

			if ( p0 < p2 ) { min = p0; max = p2; } else { min = p2; max = p0; }

			rad = fez * boxhalfsize.x + fex * boxhalfsize.z;

			if ( min > rad || max < -rad ) return false;
		}
		#endregion
		#region AXISTEST_Z12(e0.y, e0.x, fey, fex);
		{
			p1 = e0.y * v1.x - e0.x * v1.y;

			p2 = e0.y * v2.x - e0.x * v2.y;

			if ( p2 < p1 ) { min = p2; max = p1; } else { min = p1; max = p2; }

			rad = fey * boxhalfsize.x + fex * boxhalfsize.y;

			if ( min > rad || max < -rad ) return false;
		}
		#endregion

		fex = Mathf.Abs(e1.x);
		fey = Mathf.Abs(e1.y);
		fez = Mathf.Abs(e1.z);

		#region AXISTEST_X01(e1.z, e1.y, fez, fey);
		{
			p0 = e1.z * v0.y - e1.y * v0.z;

			p2 = e1.z * v2.y - e1.y * v2.z;

			if ( p0 < p2 ) { min = p0; max = p2; } else { min = p2; max = p0; }

			rad = fez * boxhalfsize.y + fey * boxhalfsize.z;

			if ( min > rad || max < -rad ) return false;
		}
		#endregion
		#region AXISTEST_Y02(e1.z, e1.x, fez, fex);
		{
			p0 = -e1.z * v0.x + e1.x * v0.z;

			p2 = -e1.z * v2.x + e1.x * v2.z;

			if ( p0 < p2 ) { min = p0; max = p2; } else { min = p2; max = p0; }

			rad = fez * boxhalfsize.x + fex * boxhalfsize.z;

			if ( min > rad || max < -rad ) return false;
		}
		#endregion
		#region AXISTEST_Z0(e1.y, e1.x, fey, fex)
		{
			p0 = e1.y * v0.x - e1.x * v0.y;

			p1 = e1.y * v1.x - e1.x * v1.y;

			if ( p0 < p1 ) { min = p0; max = p1; } else { min = p1; max = p0; }

			rad = fey * boxhalfsize.x + fex * boxhalfsize.y;

			if ( min > rad || max < -rad ) return false;
		}
		#endregion

		fex = Mathf.Abs(e2.x);
		fey = Mathf.Abs(e2.y);
		fez = Mathf.Abs(e2.z);

		#region AXISTEST_X2(e2.z, e2.y, fez, fey);
		{
			p0 = e2.z * v0.y - e2.y * v0.z;

			p1 = e2.z * v1.y - e2.y * v1.z;

			if ( p0 < p1 ) { min = p0; max = p1; } else { min = p1; max = p0; }

			rad = fez * boxhalfsize.y + fey * boxhalfsize.z;

			if ( min > rad || max < -rad ) return false;
		}
		#endregion
		#region AXISTEST_Y1(e2.z, e2.x, fez, fex);
		{
			p0 = -e2.z * v0.x + e2.x * v0.z;

			p1 = -e2.z * v1.x + e2.x * v1.z;

			if ( p0 < p1 ) { min = p0; max = p1; } else { min = p1; max = p0; }

			rad = fez * boxhalfsize.z + fex * boxhalfsize.z;

			if ( min > rad || max < -rad ) return false;
		}
		#endregion
		#region AXISTEST_Z12(e2.y, e2.x, fey, fex);
		{
			p1 = e2.y * v1.x - e2.x * v1.y;

			p2 = e2.y * v2.x - e2.x * v2.y;

			if ( p2 < p1 ) { min = p2; max = p1; } else { min = p1; max = p2; }

			rad = fey * boxhalfsize.x + fex * boxhalfsize.y;

			if ( min > rad || max < -rad ) return false;
		}
		#endregion

		FINDMINMAX(v0.x, v1.x, v2.x, out min, out max);
		if ( min > boxhalfsize.x || max < -boxhalfsize.x ) return false;

		FINDMINMAX(v0.y, v1.y, v2.y, out min, out max);
		if ( min > boxhalfsize.y || max < -boxhalfsize.y ) return false;

		FINDMINMAX(v0.z, v1.z, v2.z, out min, out max);
		if ( min > boxhalfsize.z || max < -boxhalfsize.z ) return false;

		Vector3 normal = Vector3.Cross(e0, e1);
		if ( !planeBoxOverlap(normal, v0, boxhalfsize) ) return false;  // -NJMP-

		return true;
	}
}
