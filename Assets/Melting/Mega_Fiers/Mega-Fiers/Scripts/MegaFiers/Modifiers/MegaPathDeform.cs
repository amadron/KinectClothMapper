
using UnityEngine;
using System.IO;

[AddComponentMenu("Modifiers/Path Deform")]
public class MegaPathDeform : MegaModifier
{
	public float			percent		= 0.0f;
	public float			stretch		= 1.0f;
	public float			twist		= 0.0f;
	public float			rotate		= 0.0f;
	public MegaAxis			axis		= MegaAxis.X;
	public bool				flip		= false;
	public MegaShape		path		= null;
	public bool				animate		= false;
	public float			speed		= 1.0f;
	public bool				drawpath	= false;
	public float			tangent		= 1.0f;
	[HideInInspector]
	public Matrix4x4		mat			= new Matrix4x4();

	public bool				UseTwistCurve	= false;
	public AnimationCurve	twistCurve		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
	public bool				UseStretchCurve	= false;
	public AnimationCurve	stretchCurve	= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public override string ModName()	{ return "PathDeform"; }
	public override string GetHelpURL() { return "?page_id=273"; }

	public Vector3 Up = Vector3.up;
	public int curve = 0;
	public bool usedist = false;
	public float distance = 0.0f;
	Vector3			start;
	Quaternion		tw = Quaternion.identity;
	float usepercent;
	float usetan;
	float ovlen;

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);	// Dont need either, so saving 3 vector mat mults but gaining a mat mult

		float alpha;
		float tws = 0.0f;

		if ( UseStretchCurve )
		{
			float str = stretchCurve.Evaluate(Mathf.Repeat(p.z * ovlen + usepercent, 1.0f)) * stretch;
			alpha = (p.z * ovlen * str) + usepercent;	//(percent / 100.0f);	// can precalc this
		}
		else
			alpha = (p.z * ovlen * stretch) + usepercent;	//(percent / 100.0f);	// can precalc this

		Vector3 ps	= path.InterpCurve3D(curve, alpha, path.normalizedInterp, ref tws) - start;
		Vector3 ps1	= path.InterpCurve3D(curve, alpha + usetan, path.normalizedInterp) - start;

		if ( path.splines[curve].closed )
			alpha = Mathf.Repeat(alpha, 1.0f);
		else
			alpha = Mathf.Clamp01(alpha);

		if ( UseTwistCurve )
		{
			float twst = twistCurve.Evaluate(alpha) * twist;
			tw = Quaternion.AngleAxis(twst + tws, Vector3.forward);
		}
		else
			tw = Quaternion.AngleAxis(tws + (twist * alpha), Vector3.forward);

		Vector3 relativePos = ps1 - ps;
		Quaternion rotation = Quaternion.LookRotation(relativePos, Up) * tw;
		//wtm.SetTRS(ps, rotation, Vector3.one);
		Matrix4x4 wtm = Matrix4x4.identity;
		MegaMatrix.SetTR(ref wtm, ps, rotation);

		wtm = mat * wtm;
		p.z = 0.0f;
		return wtm.MultiplyPoint3x4(p);
	}

	public override void ModStart(MegaModifiers mc)
	{
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		if ( animate )
			percent += speed * Time.deltaTime;

		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		if ( path != null )
		{
			if ( curve >= path.splines.Count )
				curve = 0;

			if ( usedist )
				percent = distance / path.splines[curve].length * 100.0f;

			usepercent = percent / 100.0f;
			ovlen = (1.0f / path.splines[curve].length);	// * stretch;
			usetan = (tangent * 0.01f);

			mat = Matrix4x4.identity;
			switch ( axis )
			{
				case MegaAxis.Z: MegaMatrix.RotateX(ref mat, -Mathf.PI * 0.5f); break;
			}
			MegaMatrix.RotateZ(ref mat, Mathf.Deg2Rad * rotate);

			SetAxis(mat);

			start = path.splines[curve].knots[0].p;

			Vector3 p1 = path.InterpCurve3D(0, 0.01f, path.normalizedInterp);

			Vector3 up = Vector3.zero;

			switch ( axis )
			{
				case MegaAxis.X: up = Vector3.left; break;
				case MegaAxis.Y: up = Vector3.back; break;
				case MegaAxis.Z: up = Vector3.up; break;
			}

			Quaternion lrot = Quaternion.identity;

			if ( flip )
				up = -up;

			lrot = Quaternion.FromToRotation(p1 - start, up);

			mat.SetTRS(Vector3.zero, lrot, Vector3.one);
			return true;
		}

		return false;
	}

	public void OnDrawGizmos()
	{
		if ( drawpath )
			Display(this);
	}

	// Mmm should be in gizmo code
	void Display(MegaPathDeform pd)
	{
		if ( pd.path != null )
		{
			// Need to do a lookat on first point to get the direction
			pd.mat = Matrix4x4.identity;

			Vector3 p = pd.path.splines[curve].knots[0].p;

			Vector3 p1 = pd.path.InterpCurve3D(curve, 0.01f, pd.path.normalizedInterp);
			Vector3 up = Vector3.zero;

			switch ( axis )
			{
				case MegaAxis.X: up = Vector3.left; break;
				case MegaAxis.Y: up = Vector3.back; break;
				case MegaAxis.Z: up = Vector3.up; break;
			}

			Quaternion lrot = Quaternion.identity;

			if ( flip )
				up = -up;

			lrot = Quaternion.FromToRotation(p1 - p, up);

			pd.mat.SetTRS(Vector3.zero, lrot, Vector3.one);

			Matrix4x4 mat = pd.transform.localToWorldMatrix * pd.mat;

			for ( int s = 0; s < pd.path.splines.Count; s++ )
			{
				float ldist = pd.path.stepdist * 0.1f;
				if ( ldist < 0.01f )
					ldist = 0.01f;

				float ds = pd.path.splines[s].length / (pd.path.splines[s].length / ldist);

				int c	= 0;
				int k	= -1;
				int lk	= -1;

				Vector3 first = pd.path.splines[s].Interpolate(0.0f, pd.path.normalizedInterp, ref lk) - p;

				for ( float dist = ds; dist < pd.path.splines[s].length; dist += ds )
				{
					float alpha = dist / pd.path.splines[s].length;
					Vector3 pos = pd.path.splines[s].Interpolate(alpha, pd.path.normalizedInterp, ref k) - p;

					if ( (c & 1) == 1 )
						Gizmos.color = pd.path.col1;
					else
						Gizmos.color = pd.path.col2;

					if ( k != lk )
					{
						for ( lk = lk + 1; lk <= k; lk++ )
						{
							Gizmos.DrawLine(mat.MultiplyPoint(first), mat.MultiplyPoint(pd.path.splines[s].knots[lk].p - p));
							first = pd.path.splines[s].knots[lk].p - p;
						}
					}

					lk = k;

					Gizmos.DrawLine(mat.MultiplyPoint(first), mat.MultiplyPoint(pos));

					c++;

					first = pos;
				}

				if ( (c & 1) == 1 )
					Gizmos.color = pd.path.col1;
				else
					Gizmos.color = pd.path.col2;

				if ( pd.path.splines[s].closed )
				{
					Vector3 pos = pd.path.splines[s].Interpolate(0.0f, pd.path.normalizedInterp, ref k) - p;
					Gizmos.DrawLine(mat.MultiplyPoint(first), mat.MultiplyPoint(pos));
				}
			}

			Vector3 p0 = pd.path.InterpCurve3D(curve, (percent / 100.0f), pd.path.normalizedInterp) - p;
			p1 = pd.path.InterpCurve3D(curve, (percent / 100.0f) + (tangent * 0.01f), pd.path.normalizedInterp) - p;

			Gizmos.color = Color.blue;
			Vector3 sz = new Vector3(pd.path.KnotSize * 0.01f, pd.path.KnotSize * 0.01f, pd.path.KnotSize * 0.01f);
			Gizmos.DrawCube(mat.MultiplyPoint(p0), sz);
			Gizmos.DrawCube(mat.MultiplyPoint(p1), sz);
		}
	}

	public override void DrawGizmo(MegaModContext context)
	{
		if ( !Prepare(context) )
			return;

		Vector3 min = context.bbox.min;
		Vector3 max = context.bbox.max;

		if ( context.mod.sourceObj != null )
			Gizmos.matrix = context.mod.sourceObj.transform.localToWorldMatrix;	// * gtm;
		else
			Gizmos.matrix = transform.localToWorldMatrix;	// * gtm;

		corners[0] = new Vector3(min.x, min.y, min.z);
		corners[1] = new Vector3(min.x, max.y, min.z);
		corners[2] = new Vector3(max.x, max.y, min.z);
		corners[3] = new Vector3(max.x, min.y, min.z);

		corners[4] = new Vector3(min.x, min.y, max.z);
		corners[5] = new Vector3(min.x, max.y, max.z);
		corners[6] = new Vector3(max.x, max.y, max.z);
		corners[7] = new Vector3(max.x, min.y, max.z);

		DrawEdge(corners[0], corners[1]);
		DrawEdge(corners[1], corners[2]);
		DrawEdge(corners[2], corners[3]);
		DrawEdge(corners[3], corners[0]);

		DrawEdge(corners[4], corners[5]);
		DrawEdge(corners[5], corners[6]);
		DrawEdge(corners[6], corners[7]);
		DrawEdge(corners[7], corners[4]);

		DrawEdge(corners[0], corners[4]);
		DrawEdge(corners[1], corners[5]);
		DrawEdge(corners[2], corners[6]);
		DrawEdge(corners[3], corners[7]);

		ExtraGizmo(context);
	}
}