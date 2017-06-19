
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if false
public class DWordTab
{
	public List<int>	values = new List<int>();
}

public class RelaxModData 
{
	public List<DWordTab> nbor = new List<DWordTab>();	// Array of neighbors for each vert.
	BitArray vis;	// visibility of edges on path to neighbors.
	int[] fnum = null;	//new List<int>();	//0;		// Number of faces for each vert.
	BitArray sel;		// Selection information.
	int vnum = 0;		// Size of above arrays

	void MaybeAppendNeighbor(int vert, int index, ref int max)
	{
		for ( int k1 = 0; k1 < max; k1++ )
		{
			if ( nbor[vert].values[k1] == index )
				return;
		}
		
		//DWORD dwi = (DWORD)index;
		nbor[vert].values.Add(index);	//Append(1, &dwi, 1);
		max++;
	}

	//RelaxModData()
	//{
		//nbor = NULL;
		//vis = NULL;
		//fnum = NULL;
		//vnum = 0;
	//}

	void Clear()
	{
		nbor.Clear();
		//vis.Clear();
		fnum = null;
	}

	void SetVNum(int num)
	{
		if ( num == vnum )
			return;
		Clear();
		vnum = num;
		if ( num < 1 )
			return;
		//nbor = new DWordTab[vnum];
		vis = new BitArray(vnum);
		fnum = new int[vnum];
		//sel.SetSize(vnum);
	}
}


[AddComponentMenu("Modifiers/Relax")]
public class MegaRelax : MegaModifier
{
	public float	Percent = 0.0f;
	public float	Decay = 0.0f;
	float			size;
	float			per;
	public MegaAxis axis;
	Matrix4x4		mat = new Matrix4x4();

	public override string ModName() { return "Relax"; }
	public override string GetHelpURL() { return "?page_id=166"; }

	static void FindVertexAngles(Mesh mm, float[] vang)
	{
		int i;
		for ( i = 0; i < mm.vertexCount; i++ )
			vang[i] = 0.0f;

		int[] tris = mm.triangles;

		int[] face = new int[3];
		Vector3[] verts = mm.vertices;

		for ( i = 0; i < tris.Length; i++ )
		{
			//int *vv = mm.f[i].vtx;
			int deg = 3;	//mm.f[i].deg;
			face[0] = tris[i];
			face[1] = tris[i + 1];
			face[2] = tris[i + 2];

			for ( int j = 0; j < 3; j++ )
			{
				Vector3 d1 = verts[face[(j + 1) % deg]] - verts[face[j]];
				Vector3 d2 = verts[face[(j + deg - 1) % deg]] - verts[face[j]];
				float len = Vector3.SqrMagnitude(d1);
				if ( len == 0.0f )
					continue;

				d1 /= Mathf.Sqrt(len);
				len = Vector3.SqrMagnitude(d2);
				if ( len == 0.0f )
					continue;

				d2 /= Mathf.Sqrt(len);
				float cs = Vector3.Dot(d1, d2);
				// STEVE: What about angles over PI?
				if ( cs >= 1.0f )
					continue;	// angle of 0
				if ( cs <= -1.0f )
					vang[face[j]] += Mathf.PI;
				else
					vang[face[j]] += Mathf.Acos(cs);
			}
		}
	}

	public float relax = 0.0f;
	public int	iter = 1;
	public bool	boundary = false;
	public bool	saddle = false;

	int		MAX_ITER = 999999999;
	int		MIN_ITER = 0;
	float	MAX_RELAX =  1.0f;
	float	MIN_RELAX = -1.0f;

	void ModifyObject()
	{	
		Matrix4x4 modmat,minv;
	
		float wtdRelax; // mjm - 4.8.99

		relax = Mathf.Clamp(relax, MIN_RELAX, MAX_RELAX);
		iter = Mathf.Clamp(iter, MIN_ITER, MAX_ITER);

		int i, j, max;
		DWORD selLevel = mesh->selLevel;
		float *vsw = (selLevel!=MESH_OBJECT) ? mesh->getVSelectionWeights() : NULL;

		rd->SetVNum (mesh->numVerts);
		for ( i = 0; i < rd->vnum; i++ )
		{
			rd->fnum[i] = 0;
			rd->nbor[i].ZeroCount();
		}

		rd->sel.ClearAll();
		DWORD *v;
		int k1, k2, origmax;
		for ( i = 0; i < mesh->numFaces; i++ )
		{
			v = mesh->faces[i].v;
			for ( j = 0; j < 3; j++ )
			{
				if ( (selLevel == MESH_FACE) && mesh->faceSel[i] )
					rd->sel.Set(v[j]);

				if ( (selLevel == MESH_EDGE) && mesh->edgeSel[i * 3 + j] )
					rd->sel.Set(v[j]);

				if ( (selLevel == MESH_EDGE) && mesh->edgeSel[i * 3 + (j + 2) % 3] )
					rd->sel.Set(v[j]);

				origmax = max = rd->nbor[v[j]].Count();
				rd->fnum[v[j]]++;
				for ( k1 = 0; k1 < max; k1++ )
					if ( rd->nbor[v[j]][k1] == v[(j + 1) % 3] )
						break;

				if ( k1 == max )
				{
					rd->nbor[v[j]].Append(1, v + (j + 1) % 3, 1);
					max++;
				}
					
				for ( k2 = 0; k2 < max; k2++ )
					if ( rd->nbor[v[j]][k2] == v[(j + 2) % 3] )
						break;

				if ( k2 == max )
				{
					rd->nbor[v[j]].Append(1, v + (j + 2) % 3, 1);
					max++;
				}
					
				if ( max > origmax )
					rd->vis[v[j]].SetSize(max, TRUE);

				if ( mesh->faces[i].getEdgeVis(j) )
					rd->vis[v[j]].Set(k1);
				else
				{
					if ( k1 >= origmax )
						rd->vis[v[j]].Clear(k1);
				}

				if ( mesh->faces[i].getEdgeVis((j + 2) % 3) )
					rd->vis[v[j]].Set(k2);
				else
				{
					if ( k2 >= origmax )
						rd->vis[v[j]].Clear(k2);
				}
			}
		}

		if ( selLevel == MESH_VERTEX )
			rd->sel = mesh->vertSel;
		else
		{
			if ( selLevel == MESH_OBJECT )
				rd->sel.SetAll();
		}

		Tab<float> vangles;
		if ( saddle )
			vangles.SetCount(rd->vnum);

		Vector3[] hold = new Vector3[rd->vnum];
		int act;

		for ( int k = 0; k < iter; k++ )
		{
			for ( i = 0; i < rd->vnum; i++ )
				hold[i] = verts[i];

			if ( saddle )
				mesh->FindVertexAngles(vangles.Addr(0));

			for ( i = 0; i < rd->vnum; i++ )
			{
				if ( (!rd->sel[i] ) && (!vsw || vsw[i] == 0) )
					continue;

				if ( saddle && (vangles[i] <= 2.0f * Mathf.PI * 0.99999f) )
					continue;

				max = rd->nbor[i].Count();
				if ( boundary && (rd->fnum[i] < max) )
					continue;

				if ( max < 1 )
					continue;

				Vector3 avg = Vector3.zero;

				for ( j = 0, act = 0; j < max; j++ )
				{
					if ( !rd->vis[i][j] )
						continue;
					act++;
					avg += hold[rd->nbor[i][j]];
				}

				if ( act < 1 )
					continue;

				wtdRelax = (!rd->sel[i]) ? relax * vsw[i] : relax;
				sverts[i] = hold[i] * (1.0f - wtdRelax) + avg * wtdRelax / ((float)act);
				//triObj->SetPoint(i, hold[i] * (1.0f - wtdRelax) + avg * wtdRelax / ((float)act));
			}
		}
	}
// 514

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		float dcy = Mathf.Exp(-Decay * p.magnitude);

		float k = ((size / Mathf.Sqrt(p.x * p.x + p.z * p.z) / 2.0f - 1.0f) * per * dcy) + 1.0f;
		p.x *= k;
		p.z *= k;
		return invtm.MultiplyPoint3x4(p);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public void SetTM1()
	{
		tm = Matrix4x4.identity;

		MegaMatrix.RotateZ(ref tm, -gizmoRot.z * Mathf.Deg2Rad);
		MegaMatrix.RotateY(ref tm, -gizmoRot.y * Mathf.Deg2Rad);
		MegaMatrix.RotateX(ref tm, -gizmoRot.x * Mathf.Deg2Rad);

		MegaMatrix.SetTrans(ref tm, gizmoPos + Offset);

		invtm = tm.inverse;
	}

	public override bool Prepare(MegaModContext mc)
	{
		mat = Matrix4x4.identity;

		switch ( axis )
		{
			case MegaAxis.X: MegaMatrix.RotateZ(ref mat, Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref mat, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		SetAxis(mat);

		float xsize = bbox.max.x - bbox.min.x;
		float zsize = bbox.max.z - bbox.min.z;
		size = (xsize > zsize) ? xsize : zsize;

		// Get the percentage to spherify at this time
		per = Percent / 100.0f;

		return true;
	}
}
#endif