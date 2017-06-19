
using UnityEngine;
using System.IO;

[AddComponentMenu("Modifiers/World Path Deform")]
public class MegaWorldPathDeform : MegaModifier
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
	public float			tangent		= 1.0f;
	[HideInInspector]
	public Matrix4x4		mat			= new Matrix4x4();

	public bool				UseTwistCurve	= false;
	public AnimationCurve	twistCurve		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
	public bool				UseStretchCurve = false;
	public AnimationCurve	stretchCurve	= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public override string	ModName() { return "WorldPathDeform"; }
	public override string	GetHelpURL() { return "?page_id=361"; }
	public Vector3			Up = Vector3.up;

	public int				curve = 0;
	public bool				usedist		= false;
	public float			distance	= 0.0f;

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

		Vector3 ps	= path.InterpCurve3D(curve, alpha, path.normalizedInterp, ref tws);	// - start;
		Vector3 ps1	= path.InterpCurve3D(curve, alpha + usetan, path.normalizedInterp);	// - start;

		if ( path.splines[curve].closed )
			alpha = Mathf.Repeat(alpha, 1.0f);
		else
			alpha = Mathf.Clamp01(alpha);

		Quaternion	tw = Quaternion.identity;

		if ( UseTwistCurve )
		{
			float twst = twistCurve.Evaluate(alpha) * twist;
			tw = Quaternion.AngleAxis(twst + tws, Vector3.forward);
		}
		else
			tw = Quaternion.AngleAxis((twist * alpha) + tws, Vector3.forward);

		ps1.x -= ps.x;
		ps1.y -= ps.y;
		ps1.z -= ps.z;
		Quaternion rotation = Quaternion.LookRotation(ps1, Up) * tw;

		Matrix4x4 wtm = Matrix4x4.identity;
		MegaMatrix.SetTR(ref wtm, ps, rotation);
		wtm = mat * wtm;

		ps.x = (p.x * wtm[0]) + (p.y * wtm[4]) + wtm[12];
		ps.y = (p.x * wtm[1]) + (p.y * wtm[5]) + wtm[13];
		ps.z = (p.x * wtm[2]) + (p.y * wtm[6]) + wtm[14];

		return ps;	//wtm.MultiplyPoint3x4(p);
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
			if ( usedist )
				percent = distance / path.splines[curve].length * 100.0f;

			if ( curve >= path.splines.Count )
				curve = 0;

			usepercent = percent / 100.0f;
			ovlen = (1.0f / path.splines[curve].length);	// * stretch;
			usetan = (tangent * 0.01f);

			mat = Matrix4x4.identity;

			switch ( axis )
			{
				case MegaAxis.X: MegaMatrix.RotateZ(ref mat, Mathf.PI * 0.5f); break;
				case MegaAxis.Y: MegaMatrix.RotateX(ref mat, -Mathf.PI * 0.5f); break;
				case MegaAxis.Z: break;
			}

			MegaMatrix.RotateZ(ref mat, Mathf.Deg2Rad * rotate);

			SetAxis(mat);

			mat = transform.localToWorldMatrix.inverse * path.transform.localToWorldMatrix;
			return true;
		}

		return false;
	}

	public override void DrawGizmo(MegaModContext context)
	{
		SetTM();

		if ( !Prepare(context) )
			return;

		Vector3 min = context.bbox.min;
		Vector3 max = context.bbox.max;

		if ( context.mod.sourceObj != null )
			Gizmos.matrix = context.mod.sourceObj.transform.localToWorldMatrix;
		else
			Gizmos.matrix = transform.localToWorldMatrix;

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
