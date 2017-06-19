
using UnityEngine;

[AddComponentMenu("Modifiers/Warps/ZStretch")]
public class MegaZStretchWarp : MegaWarp
{
	public float	amount		= 0.0f;
	public bool		doRegion	= false;
	public float	to			= 0.0f;
	public float	from		= 0.0f;
	public float	amplify		= 0.0f;
	public MegaAxis	axis		= MegaAxis.X;
	float			heightMax	= 0.0f;
	float			heightMin	= 0.0f;
	float			amplifier	= 0.0f;
	Matrix4x4		mat			= new Matrix4x4();

	public override string WarpName() { return "ZStretch"; }
	public override string GetIcon() { return "MegaStretch icon.png"; }
	public override string GetHelpURL() { return "?page_id=2560"; }

	void CalcBulge(MegaAxis axis, float stretch, float amplify)
	{
		amount = stretch;
		amplifier = (amplify >= 0.0f) ? amplify + 1.0f : 1.0f / (-amplify + 1.0f);

		if ( !doRegion )
		{
			switch ( axis )
			{
				case MegaAxis.X:
					heightMin = -Width * 0.5f;
					heightMax = Width * 0.5f;
					break;

				case MegaAxis.Z:
					heightMin = 0.0f;
					heightMax = Height;
					break;

				case MegaAxis.Y:
					heightMin = -Length * 0.5f;
					heightMax = Length * 0.5f;
					break;
			}
		}
		else
		{
			heightMin = from;
			heightMax = to;
		}
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		float normHeight;
		float xyScale, zScale;

		if ( amount == 0.0f || (heightMax - heightMin == 0) )
			return p;

		if ( (doRegion) && (to - from == 0.0f) )
			return p;

		p = tm.MultiplyPoint3x4(p);

		Vector3 ip = p;
		float dist = p.magnitude;
		float dcy = Mathf.Exp(-totaldecay * Mathf.Abs(dist));

		if ( doRegion && p.z > to )
			normHeight = (to - heightMin) / (heightMax - heightMin);
		else if ( doRegion && p.z < from )
			normHeight = (from - heightMin) / (heightMax - heightMin);
		else
			normHeight = (p.z - heightMin) / (heightMax - heightMin);

		if ( amount < 0.0f )
		{
			xyScale = (amplifier * -amount + 1.0F);
			zScale = (-1.0f / (amount - 1.0F));
		}
		else
		{
			xyScale = 1.0f / (amplifier * amount + 1.0f);
			zScale = amount + 1.0f;
		}

		float a = 4.0f * (1.0f - xyScale);
		float b = -4.0f * (1.0f - xyScale);
		float c = 1.0f;
		float fraction = (((a * normHeight) + b) * normHeight) + c;
		p.x *= fraction;
		p.y *= fraction;

		if ( doRegion && p.z < from )
			p.z += (zScale - 1.0f) * from;
		else if ( doRegion && p.z <= to )
			p.z *= zScale;
		else if ( doRegion && p.z > to )
			p.z += (zScale - 1.0f) * to;
		else
			p.z *= zScale;

		p = Vector3.Lerp(ip, p, dcy);

		p = invtm.MultiplyPoint3x4(p);

		return p;
	}

	public override bool Prepare(float decay)
	{
		tm = transform.worldToLocalMatrix;
		invtm = tm.inverse;

		mat = Matrix4x4.identity;

		switch ( axis )
		{
			case MegaAxis.X: MegaMatrix.RotateZ(ref mat, Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref mat, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		SetAxis(mat);
		CalcBulge(axis, amount, amplify);

		totaldecay = Decay + decay;
		if ( totaldecay < 0.0f )
			totaldecay = 0.0f;

		return true;
	}

	public override void ExtraGizmo()
	{
		if ( doRegion )
		{
			//DrawFromTo(axis, from, to);
		}
	}
}