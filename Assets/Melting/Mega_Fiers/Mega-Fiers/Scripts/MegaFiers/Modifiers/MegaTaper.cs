
using UnityEngine;

public enum MegaEffectAxis
{
	X = 0,
	Y = 1,
	XY = 2,
};

[AddComponentMenu("Modifiers/Taper")]
public class MegaTaper : MegaModifier
{
	public float			amount		= 0.0f;
	public bool				doRegion	= false;
	public float			to			= 0.0f;
	public float			from		= 0.0f;
	public float			dir			= 0.0f;
	public MegaAxis			axis		= MegaAxis.X;
	public MegaEffectAxis	EAxis		= MegaEffectAxis.X;
	Matrix4x4				mat			= new Matrix4x4();
	public float			crv			= 0.0f;
	public bool				sym			= false;
	bool	doX = false;
	bool	doY = false;
	float k1;
	float k2;
	float l;
	float ovl;

	void SetK(float K1, float K2)
	{
		k1 = K1;
		k2 = K2;
	}

	public override string ModName() { return "Taper"; }
	public override string GetHelpURL() { return "?page_id=338"; }


	public override Vector3 Map(int i, Vector3 p)
	{
		float z;

		if ( l == 0.0f )
			return p;

		p = tm.MultiplyPoint3x4(p);

		if ( doRegion )
		{
			if ( p.y < from )
				z = from * ovl;	// / l;
			else
			{
				if ( p.y > to )
					z = to * ovl;	// / l;
				else
					z = p.y * ovl;	// / l;
			}
		}
		else
			z = p.y * ovl;	// / l;

		if ( sym && z < 0.0f )
			z = -z;

		float f =  1.0f + z * k1 + k2 * z * (1.0f - z);	

		if ( doX )
			p.x *= f;

  		if ( doY )
			p.z *= f;

		return invtm.MultiplyPoint3x4(p);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		switch ( EAxis )
		{
			case MegaEffectAxis.X:	doX = true;		doY = false;	break;
			case MegaEffectAxis.Y:	doX = false;	doY = true;		break;
			case MegaEffectAxis.XY: doX = true;		doY = true;		break;
		}

		mat = Matrix4x4.identity;
		switch ( axis )
		{
			case MegaAxis.X: MegaMatrix.RotateZ(ref mat, Mathf.PI * 0.5f);	l = bbox.max[0] - bbox.min[0];	break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref mat, -Mathf.PI * 0.5f); l = bbox.max[2] - bbox.min[2];	break;
			case MegaAxis.Z: l = bbox.max[1] - bbox.min[1]; break;
		}

		if ( l != 0.0f )
			ovl = 1.0f / l;

		MegaMatrix.RotateY(ref mat, Mathf.Deg2Rad * dir);

		SetAxis(mat);
		SetK(amount, crv);

		return true;
	}

	public override void ExtraGizmo(MegaModContext mc)
	{
		if ( doRegion )
			DrawFromTo(axis, from , to, mc);
	}
}