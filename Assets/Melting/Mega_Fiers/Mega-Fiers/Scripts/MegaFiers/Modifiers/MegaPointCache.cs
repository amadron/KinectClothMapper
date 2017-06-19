
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class MegaPCVert
{
	public int[]		indices;
	public Vector3[]	points;
}

public enum MegaInterpMethod
{
	None,
	Linear,
	Bez,
}

public enum MegaBlendAnimMode
{
	Replace,
	Additive,
}

[AddComponentMenu("Modifiers/Point Cache")]
public class MegaPointCache : MegaModifier
{
	public float			time		= 0.0f;
	public bool				animated	= false;
	public float			speed		= 1.0f;
	public float			maxtime		= 1.0f;
	public MegaRepeatMode	LoopMode	= MegaRepeatMode.PingPong;
	public MegaInterpMethod	interpMethod = MegaInterpMethod.Linear;
	public MegaPCVert[]		Verts;
	public float			weight = 1.0f;
	public bool				framedelay = true;

	public MegaBlendAnimMode	blendMode = MegaBlendAnimMode.Additive;	// local space

	int     numPoints;            // Number of points per sample
	float   startFrame;           // Corresponds to the UI value of the same name.
	float   sampleRate;           // Corresponds to the UI value of the same name.
	int     numSamples;           // Defines how many samples are stored in the file.
	float	t;
	float	alpha = 0.0f;
	float	dalpha = 0.0f;
	int		sindex;
	int		sindex1;

	public bool				showmapping = false;
	public float			mappingSize = 0.001f;
	public int				mapStart = 0;
	public int				mapEnd = 0;

	public bool				havemapping = false;
	public float			scl			= 1.0f;
	public bool				flipyz		= false;
	public bool				negx		= false;
	public bool				negz		= false;
	public float			adjustscl	= 1.0f;
	public Vector3			adjustoff	= Vector3.zero;

	public override string ModName()	{ return "Point Cache"; }
	public override string GetHelpURL() { return "?page_id=1335"; }

	void LinearAbs(MegaModifiers mc, int start, int end)
	{
		for ( int i = start; i < end; i++ )
		{
			Vector3 p = Verts[i].points[sindex];
			Vector3 p1 = Verts[i].points[sindex1];
			p.x = p.x + ((p1.x - p.x) * dalpha);
			p.y = p.y + ((p1.y - p.y) * dalpha);
			p.z = p.z + ((p1.z - p.z) * dalpha);

			for ( int v = 0; v < Verts[i].indices.Length; v++ )
				sverts[Verts[i].indices[v]] = p;
		}
	}

	void LinearAbsWeighted(MegaModifiers mc, int start, int end)
	{
		for ( int i = start; i < end; i++ )
		{
			Vector3 p = Verts[i].points[sindex];
			Vector3 p1 = Verts[i].points[sindex1];
			p.x = p.x + ((p1.x - p.x) * dalpha);
			p.y = p.y + ((p1.y - p.y) * dalpha);
			p.z = p.z + ((p1.z - p.z) * dalpha);

			float w = mc.selection[Verts[i].indices[0]];	//[wc];
			p1 = verts[Verts[i].indices[0]];

			p = p1 + ((p - p1) * w);
			for ( int v = 0; v < Verts[i].indices.Length; v++ )
				sverts[Verts[i].indices[v]] = p;
		}
	}

	void LinearRel(MegaModifiers mc, int start, int end)
	{
		for ( int i = start; i < end; i++ )
		{
			int ix = Verts[i].indices[0];

			Vector3 basep = mc.verts[ix];

			Vector3 p = Verts[i].points[sindex];
			Vector3 p1 = Verts[i].points[sindex1];
			p.x += (((p1.x - p.x) * dalpha) - basep.x);	// * weight;	//mc.verts[ix].x;
			p.y += (((p1.y - p.y) * dalpha) - basep.y);	// * weight;	//mc.verts[ix].y;
			p.z += (((p1.z - p.z) * dalpha) - basep.z);	// * weight;	//mc.verts[ix].z;

			p1 = verts[Verts[i].indices[0]];

			p.x = p1.x + (p.x * weight);
			p.y = p1.y + (p.y * weight);
			p.z = p1.z + (p.z * weight);

			for ( int v = 0; v < Verts[i].indices.Length; v++ )
			{
				int idx = Verts[i].indices[v];
				sverts[idx] = p;
			}
		}
	}

	void LinearRelWeighted(MegaModifiers mc, int start, int end)
	{
		for ( int i = start; i < end; i++ )
		{
			int ix = Verts[i].indices[0];

			Vector3 basep = verts[ix];

			Vector3 p = Verts[i].points[sindex];
			Vector3 p1 = Verts[i].points[sindex1];
			p.x += (((p1.x - p.x) * dalpha) - basep.x);	// * weight;	//mc.verts[ix].x;
			p.y += (((p1.y - p.y) * dalpha) - basep.y);	// * weight;	//mc.verts[ix].y;
			p.z += (((p1.z - p.z) * dalpha) - basep.z);	// * weight;	//mc.verts[ix].z;

			float w = mc.selection[Verts[i].indices[0]] * weight;	//[wc];

			p1 = verts[Verts[i].indices[0]];

			p.x = p1.x + (p.x * w);
			p.y = p1.y + (p.y * w);
			p.z = p1.z + (p.z * w);

			for ( int v = 0; v < Verts[i].indices.Length; v++ )
			{
				int idx = Verts[i].indices[v];
				sverts[idx] = p;
			}
		}
	}

	void NoInterpAbs(MegaModifiers mc, int start, int end)
	{
		for ( int i = start; i < end; i++ )
		{
			Vector3 p = Verts[i].points[sindex];

			for ( int v = 0; v < Verts[i].indices.Length; v++ )
				sverts[Verts[i].indices[v]] = p;
		}
	}

	void NoInterpAbsWeighted(MegaModifiers mc, int start, int end)
	{
		for ( int i = start; i < end; i++ )
		{
			Vector3 p = Verts[i].points[sindex];

			float w = mc.selection[Verts[i].indices[0]] * weight;	//[wc];

			Vector3 p1 = verts[Verts[i].indices[0]];

			p = p1 + ((p - p1) * w);

			for ( int v = 0; v < Verts[i].indices.Length; v++ )
				sverts[Verts[i].indices[v]] = p;
		}
	}

	void NoInterpRel(MegaModifiers mc, int start, int end)
	{
		for ( int i = start; i < end; i++ )
		{
			int ix = Verts[i].indices[0];
			Vector3 p = Verts[i].points[sindex] - verts[ix];

			Vector3 p1 = verts[Verts[i].indices[0]];

			p.x = p1.x + (p.x * weight);
			p.y = p1.y + (p.y * weight);
			p.z = p1.z + (p.z * weight);

			for ( int v = 0; v < Verts[i].indices.Length; v++ )
			{
				int idx = Verts[i].indices[v];
				sverts[idx] = p;
			}
		}
	}

	void NoInterpRelWeighted(MegaModifiers mc, int start, int end)
	{
		for ( int i = start; i < end; i++ )
		{
			int ix = Verts[i].indices[0];
			Vector3 p = Verts[i].points[sindex] - verts[ix];

			float w = mc.selection[Verts[i].indices[0]] * weight;	//[wc];

			Vector3 p1 = verts[Verts[i].indices[0]];

			p = p1 + ((p - p1) * w);

			for ( int v = 0; v < Verts[i].indices.Length; v++ )
			{
				int idx = Verts[i].indices[v];
				sverts[idx] = p;
			}
		}
	}

	bool skipframe = true;

	// TODO: Option to lerp or even bez, depends on how many samples
	public override void Modify(MegaModifiers mc)
	{
		if ( Verts != null )
		{
			switch ( interpMethod )
			{
				case MegaInterpMethod.Linear:
					switch ( blendMode )
					{
						case MegaBlendAnimMode.Additive:	LinearRel(mc, 0, Verts.Length);	break;
						case MegaBlendAnimMode.Replace:		LinearAbs(mc, 0, Verts.Length);	break;
					}
					break;

				case MegaInterpMethod.Bez:
					switch ( blendMode )
					{
						case MegaBlendAnimMode.Additive:	LinearRel(mc, 0, Verts.Length); break;
						case MegaBlendAnimMode.Replace:		LinearAbs(mc, 0, Verts.Length); break;
					}
					break;

				case MegaInterpMethod.None:
					switch ( blendMode )
					{
						case MegaBlendAnimMode.Additive:	NoInterpRel(mc, 0, Verts.Length); break;
						case MegaBlendAnimMode.Replace:		NoInterpAbs(mc, 0, Verts.Length); break;
					}
					break;
			}
		}
		else
		{
			for ( int i = 0; i < verts.Length; i++ )
				sverts[i] = verts[i];
		}
	}

	public void ModifyInstance(MegaModifiers mc, float itime)
	{
		if ( Verts != null )
		{
			switch ( LoopMode )
			{
				case MegaRepeatMode.Loop:		t = Mathf.Repeat(itime, maxtime); break;
				case MegaRepeatMode.PingPong:	t = Mathf.PingPong(itime, maxtime); break;
				case MegaRepeatMode.Clamp:		t = Mathf.Clamp(itime, 0.0f, maxtime); break;
			}

			alpha = t / maxtime;

			float val = (float)(Verts[0].points.Length - 1) * alpha;

			sindex = (int)val;
			dalpha = val - sindex;
			if ( sindex == Verts[0].points.Length - 1 )
			{
				sindex1 = sindex;
				dalpha = 0.0f;
			}
			else
				sindex1 = sindex + 1;

			switch ( interpMethod )
			{
				case MegaInterpMethod.Linear:
					switch ( blendMode )
					{
						case MegaBlendAnimMode.Additive: LinearRel(mc, 0, Verts.Length); break;
						case MegaBlendAnimMode.Replace: LinearAbs(mc, 0, Verts.Length); break;
					}
					break;

				case MegaInterpMethod.Bez:
					switch ( blendMode )
					{
						case MegaBlendAnimMode.Additive: LinearRel(mc, 0, Verts.Length); break;
						case MegaBlendAnimMode.Replace: LinearAbs(mc, 0, Verts.Length); break;
					}
					break;

				case MegaInterpMethod.None:
					switch ( blendMode )
					{
						case MegaBlendAnimMode.Additive: NoInterpRel(mc, 0, Verts.Length); break;
						case MegaBlendAnimMode.Replace: NoInterpAbs(mc, 0, Verts.Length); break;
					}
					break;
			}
		}
		else
		{
			for ( int i = 0; i < verts.Length; i++ )
				sverts[i] = verts[i];
		}
	}

	public void SetAnim(float _t)
	{
		time = _t;
		t = _t;
		skipframe = true;
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		if ( !Prepare(mc) )
			return false;

		if ( animated )	//&& !lateanimupdate )
		{
			if ( framedelay && skipframe )
				skipframe = false;
			else
				time += Time.deltaTime * speed;
		}

		switch ( LoopMode )
		{
			case MegaRepeatMode.Loop:		t = Mathf.Repeat(time, maxtime); break;
			case MegaRepeatMode.PingPong:	t = Mathf.PingPong(time, maxtime); break;
			case MegaRepeatMode.Clamp:		t = Mathf.Clamp(time, 0.0f, maxtime); break;
		}

		alpha = t / maxtime;

		float val = (float)(Verts[0].points.Length - 1) * alpha;

		sindex = (int)val;
		dalpha = val - sindex;

		if ( sindex == Verts[0].points.Length - 1 )
		{
			sindex1 = sindex;
			dalpha = 0.0f;
		}
		else
			sindex1 = sindex + 1;

		return true;
	}

	public override bool Prepare(MegaModContext mc)
	{
		if ( Verts != null && Verts.Length > 0 && Verts[0].indices != null && Verts[0].indices.Length > 0 )
			return true;

		return false;
	}

	public override void DoWork(MegaModifiers mc, int index, int start, int end, int cores)
	{
		ModifyCompressedMT(mc, index, cores);
	}

	public void ModifyCompressedMT(MegaModifiers mc, int tindex, int cores)
	{
		if ( Verts != null )
		{
			int step = Verts.Length / cores;
			int startvert = (tindex * step);
			int endvert = startvert + step;

			if ( tindex == cores - 1 )
				endvert = Verts.Length;

			switch ( interpMethod )
			{
				case MegaInterpMethod.Linear:
					switch ( blendMode )
					{
						case MegaBlendAnimMode.Additive:	LinearRel(mc, startvert, endvert); break;
						case MegaBlendAnimMode.Replace:		LinearAbs(mc, startvert, endvert); break;
					}
					break;

				case MegaInterpMethod.Bez:
					switch ( blendMode )
					{
						case MegaBlendAnimMode.Additive:	LinearRel(mc, startvert, endvert); break;
						case MegaBlendAnimMode.Replace:		LinearAbs(mc, startvert, endvert); break;
					}
					break;

				case MegaInterpMethod.None:
					switch ( blendMode )
					{
						case MegaBlendAnimMode.Additive:	NoInterpRel(mc, startvert, endvert); break;
						case MegaBlendAnimMode.Replace:		NoInterpAbs(mc, startvert, endvert); break;
					}
					break;
			}
		}
	}
}
