
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaConformTarget
{
	public GameObject	target;
	public bool			children = false;
}

[AddComponentMenu("Modifiers/Conform Multi")]
public class MegaConformMulti : MegaModifier
{
	public List<MegaConformTarget>	targets = new List<MegaConformTarget>();
	public List<Collider>	conformColliders = new List<Collider>();
	public float[]		offsets;
	public Bounds		bounds;
	public float[]		last;
	public Vector3[]	conformedVerts;
	public float		conformAmount = 1.0f;
	public float		raystartoff = 0.0f;
	public float		offset = 0.0f;
	public float		raydist = 100.0f;
	public MegaAxis		axis = MegaAxis.Y;
	Matrix4x4	loctoworld;
	Matrix4x4	ctm;
	Matrix4x4	cinvtm;
	Ray			ray = new Ray();
	RaycastHit	hit;

	public override string ModName() { return "Conform Multi"; }
	public override string GetHelpURL() { return "?page_id=4547"; }

	public void BuildColliderList()
	{
		conformColliders.Clear();

		for ( int i = 0; i < targets.Count; i++ )
		{
			if ( targets[i].target )
			{
				if ( targets[i].children )
				{
					Collider[] cols = (Collider[])targets[i].target.GetComponentsInChildren<Collider>();

					for ( int c = 0; c < cols.Length; c++ )
						conformColliders.Add(cols[c]);
				}
				else
				{
					Collider col = targets[i].target.GetComponent<Collider>();

					if ( col )
						conformColliders.Add(col);
				}
			}
		}
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		return p;
	}

	bool DoRayCast(Ray ray, ref Vector3 pos, float raydist)
	{
		bool retval = false;
		float min = float.MaxValue;
		
		for ( int i = 0; i < conformColliders.Count; i++ )
		{
			if ( conformColliders[i].Raycast(ray, out hit, raydist) )
			{
				retval = true;
				if ( hit.distance < min )
				{
					min = hit.distance;
					pos = hit.point;
				}
			}
		}

		return retval;
	}

	public override void Modify(MegaModifiers mc)
	{
		if ( conformColliders.Count > 0 )
		{
			int ax = (int)axis;

			Vector3 hitpos = Vector3.zero;

			for ( int i = 0; i < verts.Length; i++ )
			{
				Vector3 origin = ctm.MultiplyPoint(verts[i]);
				origin.y += raystartoff;
				ray.origin = origin;
				ray.direction = Vector3.down;

				sverts[i] = verts[i];

				if ( DoRayCast(ray, ref hitpos, raydist) )
				{
					Vector3 lochit = cinvtm.MultiplyPoint(hitpos);

					sverts[i][ax] = Mathf.Lerp(verts[i][ax], lochit[ax] + offsets[i] + offset, conformAmount);
					last[i] = sverts[i][ax];
				}
				else
				{
					Vector3 ht = ray.origin;
					ht.y -= raydist;
					sverts[i][ax] = last[i];
				}
			}
		}
		else
			verts.CopyTo(sverts, 0);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		if ( targets.Count > 0 )
		{
			if ( conformColliders.Count == 0 )
				return false;

			if ( conformedVerts == null || conformedVerts.Length != mc.mod.verts.Length )
			{
				conformedVerts = new Vector3[mc.mod.verts.Length];
				// Need to run through all the source meshes and find the vertical offset from the base

				offsets = new float[mc.mod.verts.Length];
				last = new float[mc.mod.verts.Length];

				for ( int i = 0; i < mc.mod.verts.Length; i++ )
					offsets[i] = mc.mod.verts[i][(int)axis] - mc.bbox.min[(int)axis];
			}

			loctoworld = transform.localToWorldMatrix;

			ctm = loctoworld;
			cinvtm = transform.worldToLocalMatrix;	//ctm.inverse;
		}

		return true;
	}
}
