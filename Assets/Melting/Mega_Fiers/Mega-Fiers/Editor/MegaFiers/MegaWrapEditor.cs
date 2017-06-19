
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CanEditMultipleObjects, CustomEditor(typeof(MegaWrap))]
public class MegaWrapEditor : Editor
{
	public override void OnInspectorGUI()
	{
		MegaWrap mod = (MegaWrap)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		mod.WrapEnabled = EditorGUILayout.Toggle("Enabled", mod.WrapEnabled);
		mod.target = (MegaModifyObject)EditorGUILayout.ObjectField("Target", mod.target, typeof(MegaModifyObject), true);

		float max = 1.0f;
		if ( mod.target )
			max = mod.target.bbox.size.magnitude;

		mod.maxdist = EditorGUILayout.Slider("Max Dist", mod.maxdist, 0.0f, max);	//2.0f);	//mod.maxdist);
		if ( mod.maxdist < 0.0f )
			mod.maxdist = 0.0f;

		mod.maxpoints = EditorGUILayout.IntField("Max Points", mod.maxpoints);	//mod.maxdist);
		if ( mod.maxpoints < 1 )
			mod.maxpoints = 1;

		Color col = GUI.backgroundColor;
		EditorGUILayout.BeginHorizontal();
		if ( mod.bindverts == null )
		{
			GUI.backgroundColor = Color.red;
			if ( GUILayout.Button("Map") )
				Attach(mod.target);
		}
		else
		{
			GUI.backgroundColor = Color.green;
			if ( GUILayout.Button("ReMap") )
				Attach(mod.target);
		}

		GUI.backgroundColor = col;
		if ( GUILayout.Button("Reset") )
			mod.ResetMesh();

		EditorGUILayout.EndHorizontal();

		if ( GUI.changed )
			EditorUtility.SetDirty(mod);

		mod.gap = EditorGUILayout.FloatField("Gap", mod.gap);
		mod.shrink = EditorGUILayout.Slider("Shrink", mod.shrink, 0.0f, 1.0f);
		mod.size = EditorGUILayout.Slider("Size", mod.size, 0.001f, 0.04f);
		if ( mod.bindverts != null )
			mod.vertindex = EditorGUILayout.IntSlider("Vert Index", mod.vertindex, 0, mod.bindverts.Length - 1);
		mod.offset = EditorGUILayout.Vector3Field("Offset", mod.offset);

		mod.NormalMethod = (MegaNormalMethod)EditorGUILayout.EnumPopup("Normal Method", mod.NormalMethod);
#if UNITY_5 || UNITY_6
		mod.UseBakedMesh = EditorGUILayout.Toggle("Use Baked Mesh", mod.UseBakedMesh);
#endif

		if ( mod.bindverts == null || mod.target == null )
			EditorGUILayout.LabelField("Object not wrapped");
		else
			EditorGUILayout.LabelField("UnMapped", mod.nomapcount.ToString());

		if ( GUI.changed )
			EditorUtility.SetDirty(mod);
	}

	public void OnSceneGUI()
	{
		DisplayDebug();
	}

	void DisplayDebug()
	{
		MegaWrap mod = (MegaWrap)target;
		if ( mod.target )
		{
			if ( mod.bindverts != null && mod.bindverts.Length > 0 )
			{
				if ( mod.targetIsSkin && !mod.sourceIsSkin )
				{
					Color col = Color.black;
					Handles.matrix = Matrix4x4.identity;

					MegaBindVert bv = mod.bindverts[mod.vertindex];

					for ( int i = 0; i < bv.verts.Count; i++ )
					{
						MegaBindInf bi = bv.verts[i];
						float w = bv.verts[i].weight / bv.weight;

						if ( w > 0.5f )
							col = Color.Lerp(Color.green, Color.red, (w - 0.5f) * 2.0f);
						else
							col = Color.Lerp(Color.blue, Color.green, w * 2.0f);
						Handles.color = col;

						Vector3 p = (mod.skinnedVerts[bv.verts[i].i0] + mod.skinnedVerts[bv.verts[i].i1] + mod.skinnedVerts[bv.verts[i].i2]) / 3.0f;	//tm.MultiplyPoint(mod.vr[i].cpos);
						Handles.DotCap(i, p, Quaternion.identity, mod.size);	//0.01f);

						Vector3 p0 = mod.skinnedVerts[bi.i0];
						Vector3 p1 = mod.skinnedVerts[bi.i1];
						Vector3 p2 = mod.skinnedVerts[bi.i2];

						Vector3 cp = mod.GetCoordMine(p0, p1, p2, bi.bary);
						Handles.color = Color.gray;
						Handles.DrawLine(p, cp);

						Vector3 norm = mod.FaceNormal(p0, p1, p2);
						Vector3 cp1 = cp + (((bi.dist * mod.shrink) + mod.gap) * norm.normalized);
						Handles.color = Color.green;
						Handles.DrawLine(cp, cp1);
					}
				}
				else
				{
					Color col = Color.black;
					Matrix4x4 tm = mod.target.transform.localToWorldMatrix;
					Handles.matrix = tm;	//Matrix4x4.identity;

					MegaBindVert bv = mod.bindverts[mod.vertindex];

					for ( int i = 0; i < bv.verts.Count; i++ )
					{
						MegaBindInf bi = bv.verts[i];
						float w = bv.verts[i].weight / bv.weight;

						if ( w > 0.5f )
							col = Color.Lerp(Color.green, Color.red, (w - 0.5f) * 2.0f);
						else
							col = Color.Lerp(Color.blue, Color.green, w * 2.0f);
						Handles.color = col;

						Vector3 p = (mod.target.sverts[bv.verts[i].i0] + mod.target.sverts[bv.verts[i].i1] + mod.target.sverts[bv.verts[i].i2]) / 3.0f;	//tm.MultiplyPoint(mod.vr[i].cpos);
						Handles.DotCap(i, p, Quaternion.identity, mod.size);	//0.01f);

						Vector3 p0 = mod.target.sverts[bi.i0];
						Vector3 p1 = mod.target.sverts[bi.i1];
						Vector3 p2 = mod.target.sverts[bi.i2];

						Vector3 cp = mod.GetCoordMine(p0, p1, p2, bi.bary);
						Handles.color = Color.gray;
						Handles.DrawLine(p, cp);

						Vector3 norm = mod.FaceNormal(p0, p1, p2);
						Vector3 cp1 = cp + (((bi.dist * mod.shrink) + mod.gap) * norm.normalized);
						Handles.color = Color.green;
						Handles.DrawLine(cp, cp1);
					}
				}

				// Show unmapped verts
				Handles.color = Color.yellow;
				for ( int i = 0; i < mod.bindverts.Length; i++ )
				{
					if ( mod.bindverts[i].weight == 0.0f )
					{
						Vector3 pv1 = mod.freeverts[i];
						Handles.DotCap(0, pv1, Quaternion.identity, mod.size);	//0.01f);
					}
				}
			}

			if ( mod.verts != null && mod.verts.Length > mod.vertindex )
			{
				Handles.color = Color.red;
				Handles.matrix = mod.transform.localToWorldMatrix;
				Vector3 pv = mod.verts[mod.vertindex];
				Handles.DotCap(0, pv, Quaternion.identity, mod.size);	//0.01f);
			}
		}
	}

	void Attach(MegaModifyObject modobj)
	{
		MegaWrap mod = (MegaWrap)target;

		mod.targetIsSkin = false;
		mod.sourceIsSkin = false;

		if ( mod.mesh && mod.startverts != null )
			mod.mesh.vertices = mod.startverts;

		if ( modobj == null )
		{
			mod.bindverts = null;
			return;
		}

		mod.nomapcount = 0;

		if ( mod.mesh )
			mod.mesh.vertices = mod.startverts;

		MeshFilter mf = mod.GetComponent<MeshFilter>();
		Mesh srcmesh = null;

		if ( mf != null )
		{
			//skinned = false;
			srcmesh = mf.sharedMesh;
		}
		else
		{
			SkinnedMeshRenderer smesh = (SkinnedMeshRenderer)mod.GetComponent(typeof(SkinnedMeshRenderer));

			if ( smesh != null )
			{
				//skinned = true;
				srcmesh = smesh.sharedMesh;
				mod.sourceIsSkin = true;
			}
		}

		if ( srcmesh == null )
		{
			Debug.LogWarning("No Mesh found on the target object, make sure target has a mesh and MegaFiers modifier attached!");
			return;
		}

		if ( mod.mesh == null )
			mod.mesh = mod.CloneMesh(srcmesh);	//mf.mesh);

		if ( mf )
			mf.mesh = mod.mesh;
		else
		{
			SkinnedMeshRenderer smesh = (SkinnedMeshRenderer)mod.GetComponent(typeof(SkinnedMeshRenderer));
			smesh.sharedMesh = mod.mesh;
		}

		if ( mod.sourceIsSkin == false )
		{
			SkinnedMeshRenderer tmesh = (SkinnedMeshRenderer)modobj.GetComponent(typeof(SkinnedMeshRenderer));
			if ( tmesh != null )
			{
				mod.targetIsSkin = true;

				if ( !mod.sourceIsSkin )
				{
					Mesh sm = tmesh.sharedMesh;
					mod.bindposes = sm.bindposes;
					mod.boneweights = sm.boneWeights;
					mod.bones = tmesh.bones;
					mod.skinnedVerts = sm.vertices;	//new Vector3[sm.vertexCount];
				}
			}
		}

		if ( mod.targetIsSkin )
		{
			if ( mod.boneweights == null || mod.boneweights.Length == 0 )
				mod.targetIsSkin = false;
		}

		mod.neededVerts.Clear();

		mod.verts = mod.mesh.vertices;
		mod.startverts = mod.mesh.vertices;
		mod.freeverts = new Vector3[mod.startverts.Length];
		Vector3[] baseverts = modobj.verts;	//basemesh.vertices;
		int[] basefaces = modobj.tris;	//basemesh.triangles;

		mod.bindverts = new MegaBindVert[mod.verts.Length];

		// matrix to get vertex into local space of target
		Matrix4x4 tm = mod.transform.localToWorldMatrix * modobj.transform.worldToLocalMatrix;

		List<MegaCloseFace> closefaces = new List<MegaCloseFace>();

		Vector3 p0 = Vector3.zero;
		Vector3 p1 = Vector3.zero;
		Vector3 p2 = Vector3.zero;

		Vector3[] tverts = new Vector3[mod.target.sverts.Length];

		int tcount = 10;
		for ( int i = 0; i < tverts.Length; i++ )
		{
			tcount--;
			if ( tcount < 0 )
			{
				tcount = 10;
				EditorUtility.DisplayProgressBar("Calc vertex positions", "Vertex " + i + " of " + tverts.Length, (float)i / (float)tverts.Length);
			}

			if ( mod.targetIsSkin && !mod.sourceIsSkin )
				tverts[i] = modobj.transform.InverseTransformPoint(mod.GetSkinPos(i));
			else
				tverts[i] = baseverts[i];
		}

		EditorUtility.ClearProgressBar();

		for ( int i = 0; i < mod.verts.Length; i++ )
		{
			if ( EditorUtility.DisplayCancelableProgressBar("Wrap Mapping", "Mapping Vertex " + i + " of " + mod.verts.Length, (float)i / (float)mod.verts.Length) )
			{
				mod.bindverts = null;
				break;	
			}

			MegaBindVert bv = new MegaBindVert();
			mod.bindverts[i] = bv;

			Vector3 p = tm.MultiplyPoint(mod.verts[i]);

			p = mod.transform.TransformPoint(mod.verts[i]);
			p = modobj.transform.InverseTransformPoint(p);
			mod.freeverts[i] = p;

			closefaces.Clear();

			for ( int t = 0; t < basefaces.Length; t += 3 )
			{
				p0 = tverts[basefaces[t]];
				p1 = tverts[basefaces[t + 1]];
				p2 = tverts[basefaces[t + 2]];

				//if ( mod.targetIsSkin && !mod.sourceIsSkin )
				//{
					//p0 = modobj.transform.InverseTransformPoint(mod.GetSkinPos(basefaces[t]));
					//p1 = modobj.transform.InverseTransformPoint(mod.GetSkinPos(basefaces[t + 1]));
					//p2 = modobj.transform.InverseTransformPoint(mod.GetSkinPos(basefaces[t + 2]));
				//}
				//else
				//{
					//p0 = baseverts[basefaces[t]];
				//	p1 = baseverts[basefaces[t + 1]];
					//p2 = baseverts[basefaces[t + 2]];
				//}

				float dist = mod.GetDistance(p, p0, p1, p2);

				if ( Mathf.Abs(dist) < mod.maxdist )
				{
					MegaCloseFace cf = new MegaCloseFace();
					cf.dist = Mathf.Abs(dist);
					cf.face = t;

					bool inserted = false;
					for ( int k = 0; k < closefaces.Count; k++ )
					{
						if ( cf.dist < closefaces[k].dist )
						{
							closefaces.Insert(k, cf);
							inserted = true;
							break;
						}
					}

					if ( !inserted )
						closefaces.Add(cf);
				}
			}

			float tweight = 0.0f;
			int maxp = mod.maxpoints;
			if ( maxp == 0 )
				maxp = closefaces.Count;

			for ( int j = 0; j < maxp; j++ )
			{
				if ( j < closefaces.Count )
				{
					int t = closefaces[j].face;

					p0 = tverts[basefaces[t]];
					p1 = tverts[basefaces[t + 1]];
					p2 = tverts[basefaces[t + 2]];

					//if ( mod.targetIsSkin && !mod.sourceIsSkin )
					//{
						//p0 = modobj.transform.InverseTransformPoint(mod.GetSkinPos(basefaces[t]));
						//p1 = modobj.transform.InverseTransformPoint(mod.GetSkinPos(basefaces[t + 1]));
						//p2 = modobj.transform.InverseTransformPoint(mod.GetSkinPos(basefaces[t + 2]));
					//}
					//else
					//{
						//p0 = baseverts[basefaces[t]];
						//p1 = baseverts[basefaces[t + 1]];
						//p2 = baseverts[basefaces[t + 2]];
					//}

					Vector3 normal = mod.FaceNormal(p0, p1, p2);

					float dist = closefaces[j].dist;	//GetDistance(p, p0, p1, p2);

					MegaBindInf bi = new MegaBindInf();
					bi.dist = mod.GetPlaneDistance(p, p0, p1, p2);	//dist;
					bi.face = t;
					bi.i0 = basefaces[t];
					bi.i1 = basefaces[t + 1];
					bi.i2 = basefaces[t + 2];
					bi.bary = mod.CalcBary(p, p0, p1, p2);
					bi.weight = 1.0f / (1.0f + dist);
					bi.area = normal.magnitude * 0.5f;	//CalcArea(baseverts[basefaces[t]], baseverts[basefaces[t + 1]], baseverts[basefaces[t + 2]]);	// Could calc once at start
					tweight += bi.weight;
					bv.verts.Add(bi);
				}
			}

			if ( mod.maxpoints > 0 && mod.maxpoints < bv.verts.Count )
				bv.verts.RemoveRange(mod.maxpoints, bv.verts.Count - mod.maxpoints);

			// Only want to calculate skin vertices we use
			if ( !mod.sourceIsSkin && mod.targetIsSkin )
			{
				for ( int fi = 0; fi < bv.verts.Count; fi++ )
				{
					if ( !mod.neededVerts.Contains(bv.verts[fi].i0) )
						mod.neededVerts.Add(bv.verts[fi].i0);

					if ( !mod.neededVerts.Contains(bv.verts[fi].i1) )
						mod.neededVerts.Add(bv.verts[fi].i1);

					if ( !mod.neededVerts.Contains(bv.verts[fi].i2) )
						mod.neededVerts.Add(bv.verts[fi].i2);
				}
			}

			if ( tweight == 0.0f )
			{
				mod.nomapcount++;
				break;
			}

			bv.weight = tweight;
		}

		EditorUtility.ClearProgressBar();
	}
}