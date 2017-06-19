
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class MegaAnimatedVert
{
	public int[]				indices;
	public Vector3				startVert;
	public MegaBezVector3KeyControl	con;
}

// Should add multiple channels or animation to save copys

[AddComponentMenu("Modifiers/Vertex Anim")]
public class MegaVertexAnim : MegaModifier
{
	public float	time		= 0.0f;
	public bool		animated	= false;
	public float	speed		= 1.0f;
	public float	maxtime		= 4.0f;
	public int[]	NoAnim;
	public float	weight = 1.0f;

	public MegaAnimatedVert[]	Verts;
	float t;

	public MegaBlendAnimMode	blendMode = MegaBlendAnimMode.Additive;

	public override string ModName()	{ return "AnimatedMesh"; }
	public override string GetHelpURL() { return "?page_id=1350"; }

	void Replace(MegaModifiers mc, int startvert, int endvert)
	{
		for ( int i = startvert; i < endvert; i++ )
		{
			MegaBezVector3KeyControl bc = (MegaBezVector3KeyControl)Verts[i].con;

			Vector3 off = bc.GetVector3(t);

			// ******* We must have duplicate verts in the indices array, so check that, if so same will apply to pc mod
			for ( int v = 0; v < Verts[i].indices.Length; v++ )
				sverts[Verts[i].indices[v]] = off;
		}
	}

	void ReplaceWeighted(MegaModifiers mc, int startvert, int endvert)
	{
		for ( int i = startvert; i < endvert; i++ )
		{
			MegaBezVector3KeyControl bc = (MegaBezVector3KeyControl)Verts[i].con;

			Vector3 off = bc.GetVector3(t);

			float w = mc.selection[Verts[i].indices[0]] * weight;	//[wc];

			Vector3 p1 = verts[Verts[i].indices[0]];

			off = p1 + ((off - p1) * w);

			for ( int v = 0; v < Verts[i].indices.Length; v++ )
				sverts[Verts[i].indices[v]] = off;
		}
	}

	void Additive(MegaModifiers mc, int startvert, int endvert)
	{
		for ( int i = startvert; i < endvert; i++ )
		{
			MegaBezVector3KeyControl bc = (MegaBezVector3KeyControl)Verts[i].con;

			Vector3 basep = mc.verts[Verts[i].indices[0]];
			Vector3 off = bc.GetVector3(t) - basep;

			off = verts[Verts[i].indices[0]] + (off * weight);

			for ( int v = 0; v < Verts[i].indices.Length; v++ )
			{
				int idx = Verts[i].indices[v];

				sverts[idx] = off;
			}
		}
	}

	void AdditiveWeighted(MegaModifiers mc, int startvert, int endvert)
	{
		for ( int i = startvert; i < endvert; i++ )
		{
			MegaBezVector3KeyControl bc = (MegaBezVector3KeyControl)Verts[i].con;

			Vector3 basep = mc.verts[Verts[i].indices[0]];
			Vector3 off = bc.GetVector3(t) - basep;

			float w = mc.selection[Verts[i].indices[0]] * weight;	//[wc];

			Vector3 p1 = verts[Verts[i].indices[0]];

			off = p1 + ((off - p1) * w);

			for ( int v = 0; v < Verts[i].indices.Length; v++ )
			{
				int idx = Verts[i].indices[v];

				sverts[idx] = off;
			}
		}
	}

	public override void Modify(MegaModifiers mc)
	{
		switch ( blendMode )
		{
			case MegaBlendAnimMode.Additive:	Additive(mc, 0, Verts.Length);	break;
			case MegaBlendAnimMode.Replace:		Replace(mc, 0, Verts.Length); break;
		}

		if ( NoAnim != null )
		{
			for ( int i = 0; i < NoAnim.Length; i++ )
			{
				int index = NoAnim[i];
				sverts[index] = verts[index];
			}
		}
	}

	public MegaRepeatMode LoopMode = MegaRepeatMode.PingPong;

	public override bool ModLateUpdate(MegaModContext mc)
	{
		if ( animated )
			time += Time.deltaTime * speed;

		switch ( LoopMode )
		{
			case MegaRepeatMode.Loop:		t = Mathf.Repeat(time, maxtime);		break;
			case MegaRepeatMode.PingPong:	t = Mathf.PingPong(time, maxtime);		break;
			case MegaRepeatMode.Clamp:		t = Mathf.Clamp(time, 0.0f, maxtime);	break;
		}

		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		return true;
	}

	public override void DoWork(MegaModifiers mc, int index, int start, int end, int cores)
	{
		ModifyCompressedMT(mc, index, cores);
	}

	public void ModifyCompressedMT(MegaModifiers mc, int tindex, int cores)
	{
		int step = NoAnim.Length / cores;
		int startvert = (tindex * step);
		int endvert = startvert + step;

		if ( tindex == cores - 1 )
			endvert = NoAnim.Length;

		if ( NoAnim != null )
		{
			for ( int i = startvert; i < endvert; i++ )
			{
				int index = NoAnim[i];
				sverts[index] = verts[index];
			}
		}

		step = Verts.Length / cores;
		startvert = (tindex * step);
		endvert = startvert + step;

		if ( tindex == cores - 1 )
			endvert = Verts.Length;

		switch ( blendMode )
		{
			case MegaBlendAnimMode.Additive:	Additive(mc, startvert, endvert); break;
			case MegaBlendAnimMode.Replace:		Replace(mc, startvert, endvert); break;
		}
	}
}
