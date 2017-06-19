
using UnityEngine;

public enum MegaMeltMat
{
	Ice = 0,
	Glass,
	Jelly,
	Plastic,
	Custom,
}

[AddComponentMenu("Modifiers/Melt")]
public class MegaMelt : MegaModifier
{
	public float		Amount			= 0.0f;
	public float		Spread			= 19.0f;
	public MegaMeltMat	MaterialType	= MegaMeltMat.Ice;
	public float		Solidity		= 1.0f;
	public MegaAxis		axis			= MegaAxis.X;
	public bool			FlipAxis		= false;
	float				zba				= 0.0f;
	public float		flatness		= 0.0f;
	float				size			= 0.0f;
	float				bulger			= 0.0f;
	float				ybr,zbr,visvaluea;
	int					confiner,vistypea;
	float				cx,cy,cz;
	float				xsize,ysize,zsize;
	float				ooxsize,ooysize,oozsize;

	public override string ModName() { return "Melt"; }
	public override string GetHelpURL() { return "?page_id=225"; }

	float hypot(float x, float y)
	{
		return Mathf.Sqrt(x * x + y * y);
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		float x, y, z;
		float xw,yw,zw;
		float vdist,mfac,dx,dy;
		float defsinex = 0.0f, coldef = 0.0f, realmax = 0.0f;

		// Mult by mc
		p = tm.MultiplyPoint3x4(p);

		x = p.x; y = p.y; z = p.z;
		xw = x - cx; yw = y - cy; zw = z - cz;

		if ( xw == 0.0f && yw == 0.0f && zw == 0.0f ) xw = yw = zw = 1.0f;
		if ( x == 0.0f && y == 0.0f && z == 0.0f ) x = y = z = 1.0f;

		// Find distance from centre
		vdist = Mathf.Sqrt(xw * xw + yw * yw + zw * zw);

		mfac = size / vdist;

		if ( axis == MegaAxis.Y )
		{
			dx = xw + Mathf.Sign(xw) * ((Mathf.Abs(xw * mfac)) * (bulger * ybr));
			dy = yw + Mathf.Sign(yw) * ((Mathf.Abs(yw * mfac)) * (bulger * ybr));
			x = (dx + cx);
			y = (dy + cy);
		}

		if ( axis == MegaAxis.Z )	//Y )
		{
			dx = xw + Mathf.Sign(xw) * ((Mathf.Abs(xw * mfac)) * (bulger * ybr));
			dy = zw + Mathf.Sign(zw) * ((Mathf.Abs(zw * mfac)) * (bulger * ybr));
			x = (dx + cx);
			z = (dy + cz);
		}

		if ( axis == MegaAxis.X )	//Z )
		{
			dx = zw + Mathf.Sign(zw) * ((Mathf.Abs(zw * mfac)) * (bulger * ybr));
			dy = yw + Mathf.Sign(yw) * ((Mathf.Abs(yw * mfac)) * (bulger * ybr));
			z = (dx + cz);
			y = (dy + cy);
		}

		if ( axis == MegaAxis.Y ) if ( p.z < (bbox.min.z + zbr) ) goto skipmelt;
		if ( axis == MegaAxis.Z ) if ( p.y < (bbox.min.y + zbr) ) goto skipmelt;
		if ( axis == MegaAxis.X ) if ( p.x < (bbox.min.x + zbr) ) goto skipmelt;

		if ( axis == MegaAxis.Y ) realmax = hypot((bbox.max.x - cx), (bbox.max.y - cy));
		if ( axis == MegaAxis.Z ) realmax = hypot((bbox.max.x - cx), (bbox.max.z - cz));
		if ( axis == MegaAxis.X ) realmax = hypot((bbox.max.z - cz), (bbox.max.y - cy));

		if ( axis == MegaAxis.Y )
		{
			defsinex = hypot((x - cx), (y - cy));
			coldef = realmax - hypot((x - cx), (y - cy));
		}

		if ( axis == MegaAxis.Z )
		{
			defsinex = hypot((x - cx), (z - cz));
			coldef = realmax - hypot((x - cx), (z - cz));
		}

		if ( axis == MegaAxis.X )
		{
			defsinex = hypot((z - cz), (y - cy));
			coldef = realmax - hypot((z - cz), (y - cy));
		}

		if ( coldef < 0.0f )
			coldef = 0.0f;

		defsinex += (coldef / visvaluea);

		if ( axis == MegaAxis.Y )
		{
			if ( FlipAxis )
			{
				float nminz = bbox.min.z + (((z - bbox.min.z) * oozsize) * flatness);

				z -= (defsinex * bulger);
				if ( z <= nminz ) z = nminz;
				if ( z <= (nminz + zbr) ) z = (nminz + zbr);
			}
			else
			{
				float nmaxz = bbox.max.z - (((bbox.max.z - z) * oozsize) * flatness);

				z += (defsinex * bulger);
				if ( z >= nmaxz ) z = nmaxz;
				if ( z >= (nmaxz + zbr) ) z = (nmaxz + zbr);
			}
		}
		if ( axis == MegaAxis.Z )
		{
			if ( !FlipAxis )
			{
				float nminy = bbox.min.y + (((y - bbox.min.y) * ooysize) * flatness);
				y -= (defsinex * bulger);
				if ( y <= nminy ) y = nminy;
				if ( y <= (nminy + zbr) ) y = (nminy + zbr);
			}
			else
			{
				float nmaxy = bbox.max.y - (((bbox.max.y - y) * ooysize) * flatness);
				y += (defsinex * bulger);
				if ( y >= nmaxy ) y = nmaxy;
				if ( y >= (nmaxy + zbr) ) y = (nmaxy + zbr);
			}
		}
		if ( axis == MegaAxis.X )
		{
			if ( !FlipAxis )
			{
				float nminx = bbox.min.x + (((x - bbox.min.x) * ooxsize) * flatness);
				x -= (defsinex * bulger);
				if ( x <= nminx ) x = nminx;
				if ( x <= (nminx + zbr) ) x = (nminx + zbr);
			}
			else
			{
				float nmaxx = bbox.max.x - (((bbox.max.x - x) * ooxsize) * flatness);
				x += (defsinex * bulger);
				if ( x >= nmaxx ) x = nmaxx;
				if ( x >= (nmaxx + zbr) ) x = (nmaxx + zbr);
			}
		}

	// [jump point] don't melt this point...
	skipmelt:
		p.x = x; p.y = y; p.z = z;
		p = invtm.MultiplyPoint3x4(p);
		return p;
	}

	public override void ModStart(MegaModifiers mc)
	{
		cx = bbox.center.x;
		cy = bbox.center.y;
		cz = bbox.center.z;

		// Compute the size and center
		xsize = (bbox.max.x - bbox.min.x);
		ysize = (bbox.max.y - bbox.min.y);
		zsize = (bbox.max.z - bbox.min.z);

		size = (xsize > ysize) ? xsize : ysize;
		size = (zsize > size) ? zsize : size;
		size /= 2.0f;
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		cx = bbox.center.x;
		cy = bbox.center.y;
		cz = bbox.center.z;

		// Compute the size and center
		xsize = (bbox.max.x - bbox.min.x);
		ysize = (bbox.max.y - bbox.min.y);
		zsize = (bbox.max.z - bbox.min.z);

		ooxsize = 1.0f / xsize;
		ooysize = 1.0f / ysize;
		oozsize = 1.0f / zsize;

		size = (xsize > ysize) ? xsize : ysize;
		size = (zsize > size) ? zsize : size;
		size /= 2.0f;

		switch ( MaterialType )
		{
			case MegaMeltMat.Ice:		visvaluea = 2.0f;		break;
			case MegaMeltMat.Glass:		visvaluea = 12.0f;		break;
			case MegaMeltMat.Jelly:		visvaluea = 0.4f;		break;
			case MegaMeltMat.Plastic:	visvaluea = 0.7f;		break;
			case MegaMeltMat.Custom:	visvaluea = Solidity;	break;
		}

		if ( Amount < 0.0f )
			Amount = 0.0f;

		ybr = Spread / 100.0f;
		zbr = zba / 10.0f;
		bulger = Amount / 100.0f;

		return true;
	}
}
