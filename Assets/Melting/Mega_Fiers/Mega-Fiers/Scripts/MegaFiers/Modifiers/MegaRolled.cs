
using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Modifiers/Rolled")]
public class MegaRolled : MegaModifier
{
	public float		radius	= 1.0f;
	public Transform	roller;
	public float		splurge	= 1.0f;
	public MegaAxis		fwdaxis	= MegaAxis.Z;
	Matrix4x4			mat		= new Matrix4x4();
	Vector3[]			offsets;
	Plane				plane;
	float				height	= 0.0f;

	public override string ModName() { return "Rolled"; }
	public override string GetHelpURL() { return "?page_id=1292"; }

	public override Vector3 Map(int i, Vector3 p)
	{
		if ( i >= 0 )
		{
			p = tm.MultiplyPoint3x4(p);	// tm may have an offset gizmo etc

			if ( p.z > rpos.z )
			{
				p.y *= delta;	//height;

				p.x += (1.0f - delta) * splurge * p.x;
				p.z += (1.0f - delta) * splurge * (p.z - rpos.z);
			}

			p = invtm.MultiplyPoint3x4(p);
		}

		return p;
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	Vector3 rpos;
	public bool	clearoffsets = false;

	float delta = 0.0f;

	public override bool Prepare(MegaModContext mc)
	{
		if ( !roller )
			return false;

		rpos = transform.worldToLocalMatrix.MultiplyPoint3x4(roller.position);

		height = rpos.y - radius;

		if ( offsets == null || offsets.Length != mc.mod.verts.Length )
			offsets = new Vector3[mc.mod.verts.Length];

		mat = Matrix4x4.identity;

		SetAxis(mat);
		tm = Matrix4x4.identity;

		if ( clearoffsets )
		{
			clearoffsets = false;

			for ( int i = 0; i < offsets.Length; i++ )
			{
				offsets[i] = Vector3.zero;
			}
		}

		if ( height < mc.bbox.Size().y )
			delta = height / mc.bbox.Size().y;
		else
			delta = 1.0f;

		return true;
	}

	public override void PrepareMT(MegaModifiers mc, int cores)
	{
	}

	public override void DoWork(MegaModifiers mc, int index, int start, int end, int cores)
	{
		if ( index == 0 )
			Modify(mc);
	}
}