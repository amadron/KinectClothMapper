
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaBindInf
{
	public float	dist;
	public int		face;
	public int		i0;
	public int		i1;
	public int		i2;
	public Vector3	bary;
	public float	weight;
	public float	area;
}

[System.Serializable]
public class MegaBindVert
{
	public float				weight;
	public List<MegaBindInf>	verts = new List<MegaBindInf>();
}

public struct MegaCloseFace
{
	public int face;
	public float dist;
}

[ExecuteInEditMode]
public class MegaWrap : MonoBehaviour
{
	public float			gap				= 0.0f;
	public float			shrink			= 1.0f;
	public List<int>		neededVerts		= new List<int>();
	public Vector3[]		skinnedVerts;
	public Mesh				mesh			= null;
	public Vector3			offset			= Vector3.zero;
	public bool				targetIsSkin	= false;
	public bool				sourceIsSkin	= false;
	public int				nomapcount		= 0;
	public Matrix4x4[]		bindposes;
	public BoneWeight[]		boneweights;
	public Transform[]		bones;
	public float			size			= 0.01f;
	public int				vertindex		= 0;
	public Vector3[]		freeverts;	// position for any vert with no attachments
	public Vector3[]		startverts;
	public Vector3[]		verts;
	public MegaBindVert[]	bindverts;
	public MegaModifyObject	target;
	public float			maxdist			= 0.25f;
	public int				maxpoints		= 4;
	public bool				WrapEnabled		= true;
	public MegaNormalMethod NormalMethod	= MegaNormalMethod.Unity;

#if UNITY_5 || UNITY_6
	public bool				UseBakedMesh	= true;
#endif

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=3709");
	}

	Vector4 Plane(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		Vector3 normal = Vector4.zero;
		normal.x = (v2.y - v1.y) * (v3.z - v1.z) - (v2.z - v1.z) * (v3.y - v1.y);
		normal.y = (v2.z - v1.z) * (v3.x - v1.x) - (v2.x - v1.x) * (v3.z - v1.z);
		normal.z = (v2.x - v1.x) * (v3.y - v1.y) - (v2.y - v1.y) * (v3.x - v1.x);

		normal = normal.normalized;
		return new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(v2, normal));
	}

	float PlaneDist(Vector3 p, Vector4 plane)
	{
		Vector3 n = plane;
		return Vector3.Dot(n, p) + plane.w;
	}

	public float GetDistance(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		return MegaNearestPointTest.DistPoint3Triangle3Dbl(p, p0, p1, p2);
	}

	public float GetPlaneDistance(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector4 pl = Plane(p0, p1, p2);
		return PlaneDist(p, pl);
	}

	public Vector3 MyBary(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector3 bary = Vector3.zero;
		Vector3 normal = FaceNormal(p0, p1, p2);

		float areaABC = Vector3.Dot(normal, Vector3.Cross((p1 - p0), (p2 - p0)));
		float areaPBC = Vector3.Dot(normal, Vector3.Cross((p1 - p), (p2 - p)));
		float areaPCA = Vector3.Dot(normal, Vector3.Cross((p2 - p), (p0 - p)));

		bary.x = areaPBC / areaABC; // alpha
		bary.y = areaPCA / areaABC; // beta
		bary.z = 1.0f - bary.x - bary.y; // gamma
		return bary;
	}

	public Vector3 MyBary1(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
		float d00 = Vector3.Dot(v0, v0);
		float d01 = Vector3.Dot(v0, v1);
		float d11 = Vector3.Dot(v1, v1);
		float d20 = Vector3.Dot(v2, v0);
		float d21 = Vector3.Dot(v2, v1);
		float denom = d00 * d11 - d01 * d01;

		float w = (d11 * d20 - d01 * d21) / denom;
		float v = (d00 * d21 - d01 * d20) / denom;
		float u = 1.0f - v - w;
		return new Vector3(u, v, w);
	}

	public Vector3 CalcBary(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		return MyBary(p, p0, p1, p2);
	}

	public float CalcArea(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector3 e1 = p1 - p0;
		Vector3 e2 = p2 - p0;
		Vector3 e3 = Vector3.Cross(e1, e2);
		return 0.5f * e3.magnitude;
	}

	Vector3 e11 = Vector3.zero;
	Vector3 e22 = Vector3.zero;
	Vector3 cr = Vector3.zero;

	public Vector3 FaceNormal(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		//Vector3 e1 = p1 - p0;
		//Vector3 e2 = p2 - p0;

		e11.x = p1.x - p0.x;
		e11.y = p1.y - p0.y;
		e11.z = p1.z - p0.z;

		e22.x = p2.x - p0.x;
		e22.y = p2.y - p0.y;
		e22.z = p2.z - p0.z;

		//Vector3 e2 = p2 - p0;

		cr.x = e11.y * e22.z - e22.y * e11.z;
		cr.y = -(e11.x * e22.z - e22.x * e11.z);	// * -1;
		cr.z = e11.x * e22.y - e22.x * e11.y;

		return cr;	//Vector3.Cross(e11, e22);
	}


	static void CopyBlendShapes(Mesh mesh1, Mesh clonemesh)
	{
#if UNITY_5_3 || UNITY_5_4 || UNITY_6
		int bcount = mesh1.blendShapeCount;	//GetBlendShapeFrameCount();

		Vector3[] deltaverts = new Vector3[mesh1.vertexCount];
		Vector3[] deltanorms = new Vector3[mesh1.vertexCount];
		Vector3[] deltatans = new Vector3[mesh1.vertexCount];

		for ( int j = 0; j < bcount; j++ )
		{
			int frames = mesh1.GetBlendShapeFrameCount(j);
			string bname = mesh1.GetBlendShapeName(j);

			for ( int f = 0; f < frames; f++ )
			{
				mesh1.GetBlendShapeFrameVertices(j, f, deltaverts, deltanorms, deltatans);
				float weight = mesh1.GetBlendShapeFrameWeight(j, f);

				clonemesh.AddBlendShapeFrame(bname, weight, deltaverts, deltanorms, deltatans);
			}
		}
#endif
	}


	public Mesh CloneMesh(Mesh m)
	{
		Mesh clonemesh = new Mesh();
		clonemesh.vertices = m.vertices;
#if UNITY_5_0 || UNITY_5_1 || UNITY_5
		clonemesh.uv2 = m.uv2;
		clonemesh.uv3 = m.uv3;
		clonemesh.uv4 = m.uv4;
#else
		clonemesh.uv1 = m.uv1;
		clonemesh.uv2 = m.uv2;
#endif
		clonemesh.uv = m.uv;
		clonemesh.normals = m.normals;
		clonemesh.tangents = m.tangents;
		clonemesh.colors = m.colors;

		clonemesh.subMeshCount = m.subMeshCount;

		for ( int s = 0; s < m.subMeshCount; s++ )
			clonemesh.SetTriangles(m.GetTriangles(s), s);

		CopyBlendShapes(m, clonemesh);

		clonemesh.boneWeights = m.boneWeights;
		clonemesh.bindposes = m.bindposes;
		clonemesh.name = m.name;	// + "_copy";
		clonemesh.RecalculateBounds();
		return clonemesh;
	}

	[ContextMenu("Reset Mesh")]
	public void ResetMesh()
	{
		if ( mesh )
		{
			mesh.vertices = startverts;
			mesh.RecalculateBounds();
			RecalcNormals();
			//mesh.RecalculateNormals();
		}

		target = null;
		bindverts = null;
	}

	public void SetMesh()
	{
		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh srcmesh = null;

		if ( mf != null )
			srcmesh = mf.sharedMesh;
		else
		{
			SkinnedMeshRenderer smesh = (SkinnedMeshRenderer)GetComponent(typeof(SkinnedMeshRenderer));

			if ( smesh != null )
				srcmesh = smesh.sharedMesh;
		}

		if ( srcmesh != null )
		{
			mesh = CloneMesh(srcmesh);

			if ( mf )
				mf.sharedMesh = mesh;
			else
			{
				SkinnedMeshRenderer smesh = (SkinnedMeshRenderer)GetComponent(typeof(SkinnedMeshRenderer));
				smesh.sharedMesh = mesh;
			}
		}
	}

	public void Attach(MegaModifyObject modobj)
	{
		targetIsSkin = false;
		sourceIsSkin = false;

		if ( mesh && startverts != null )
			mesh.vertices = startverts;

		if ( modobj == null )
		{
			bindverts = null;
			return;
		}

		nomapcount = 0;

		if ( mesh )
			mesh.vertices = startverts;

		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh srcmesh = null;

		if ( mf != null )
		{
			//skinned = false;
			srcmesh = mf.mesh;
		}
		else
		{
			SkinnedMeshRenderer smesh = (SkinnedMeshRenderer)GetComponent(typeof(SkinnedMeshRenderer));

			if ( smesh != null )
			{
				//skinned = true;
				srcmesh = smesh.sharedMesh;
				sourceIsSkin = true;
			}
		}

		if ( srcmesh == null )
		{
			Debug.LogWarning("No Mesh found on the target object, make sure target has a mesh and MegaFiers modifier attached!");
			return;
		}

		if ( mesh == null )
			mesh = CloneMesh(srcmesh);	//mf.mesh);

		if ( mf )
			mf.mesh = mesh;
		else
		{
			SkinnedMeshRenderer smesh = (SkinnedMeshRenderer)GetComponent(typeof(SkinnedMeshRenderer));
			smesh.sharedMesh = mesh;
		}

		if ( sourceIsSkin == false )
		{
			SkinnedMeshRenderer tmesh = (SkinnedMeshRenderer)modobj.GetComponent(typeof(SkinnedMeshRenderer));
			if ( tmesh != null )
			{
				targetIsSkin = true;

				if ( !sourceIsSkin )
				{
					Mesh sm = tmesh.sharedMesh;
					bindposes = sm.bindposes;
					boneweights = sm.boneWeights;
					bones = tmesh.bones;
					skinnedVerts = sm.vertices;	//new Vector3[sm.vertexCount];
				}
			}
		}

		if ( targetIsSkin )
		{
			if ( boneweights == null || boneweights.Length == 0  )
				targetIsSkin = false;
		}

		neededVerts.Clear();

		verts = mesh.vertices;
		startverts = mesh.vertices;
		freeverts = new Vector3[startverts.Length];
		Vector3[] baseverts = modobj.verts;	//basemesh.vertices;
		int[] basefaces = modobj.tris;	//basemesh.triangles;

		bindverts = new MegaBindVert[verts.Length];

		// matrix to get vertex into local space of target
		Matrix4x4 tm = transform.localToWorldMatrix * modobj.transform.worldToLocalMatrix;

		List<MegaCloseFace> closefaces = new List<MegaCloseFace>();

		Vector3 p0 = Vector3.zero;
		Vector3 p1 = Vector3.zero;
		Vector3 p2 = Vector3.zero;

		for ( int i = 0; i < verts.Length; i++ )
		{
			MegaBindVert bv = new MegaBindVert();
			bindverts[i] = bv;

			Vector3 p = tm.MultiplyPoint(verts[i]);

			p = transform.TransformPoint(verts[i]);
			p = modobj.transform.InverseTransformPoint(p);
			freeverts[i] = p;

			closefaces.Clear();

			for ( int t = 0; t < basefaces.Length; t += 3 )
			{
				if ( targetIsSkin && !sourceIsSkin )
				{
					p0 = modobj.transform.InverseTransformPoint(GetSkinPos(basefaces[t]));
					p1 = modobj.transform.InverseTransformPoint(GetSkinPos(basefaces[t + 1]));
					p2 = modobj.transform.InverseTransformPoint(GetSkinPos(basefaces[t + 2]));
				}
				else
				{
					p0 = baseverts[basefaces[t]];
					p1 = baseverts[basefaces[t + 1]];
					p2 = baseverts[basefaces[t + 2]];
				}

				float dist = GetDistance(p, p0, p1, p2);

				if ( Mathf.Abs(dist) < maxdist )
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
			int maxp = maxpoints;
			if ( maxp == 0 )
				maxp = closefaces.Count;

			for ( int j = 0; j < maxp; j++ )
			{
				if ( j < closefaces.Count )
				{
					int t = closefaces[j].face;

					if ( targetIsSkin && !sourceIsSkin )
					{
						p0 = modobj.transform.InverseTransformPoint(GetSkinPos(basefaces[t]));
						p1 = modobj.transform.InverseTransformPoint(GetSkinPos(basefaces[t + 1]));
						p2 = modobj.transform.InverseTransformPoint(GetSkinPos(basefaces[t + 2]));
					}
					else
					{
						p0 = baseverts[basefaces[t]];
						p1 = baseverts[basefaces[t + 1]];
						p2 = baseverts[basefaces[t + 2]];
					}

					Vector3 normal = FaceNormal(p0, p1, p2);

					float dist = closefaces[j].dist;	//GetDistance(p, p0, p1, p2);

					MegaBindInf bi = new MegaBindInf();
					bi.dist = GetPlaneDistance(p, p0, p1, p2);	//dist;
					bi.face = t;
					bi.i0 = basefaces[t];
					bi.i1 = basefaces[t + 1];
					bi.i2 = basefaces[t + 2];
					bi.bary = CalcBary(p, p0, p1, p2);
					bi.weight = 1.0f / (1.0f + dist);
					bi.area = normal.magnitude * 0.5f;	//CalcArea(baseverts[basefaces[t]], baseverts[basefaces[t + 1]], baseverts[basefaces[t + 2]]);	// Could calc once at start
					tweight += bi.weight;
					bv.verts.Add(bi);
				}
			}

			if ( maxpoints > 0 && maxpoints < bv.verts.Count )
				bv.verts.RemoveRange(maxpoints, bv.verts.Count - maxpoints);

			// Only want to calculate skin vertices we use
			if ( !sourceIsSkin && targetIsSkin )
			{
				for ( int fi = 0; fi < bv.verts.Count; fi++ )
				{
					if ( !neededVerts.Contains(bv.verts[fi].i0) )
						neededVerts.Add(bv.verts[fi].i0);

					if ( !neededVerts.Contains(bv.verts[fi].i1) )
						neededVerts.Add(bv.verts[fi].i1);

					if ( !neededVerts.Contains(bv.verts[fi].i2) )
						neededVerts.Add(bv.verts[fi].i2);
				}
			}

			if ( tweight == 0.0f )
				nomapcount++;

			bv.weight = tweight;
		}
	}

	void LateUpdate()
	{
		DoUpdate();
	}

	public Vector3 GetSkinPos(int i)
	{
		Vector3 pos = target.sverts[i];
		Vector3 bpos = bindposes[boneweights[i].boneIndex0].MultiplyPoint(pos);
		Vector3 p = bones[boneweights[i].boneIndex0].TransformPoint(bpos) * boneweights[i].weight0;

		bpos = bindposes[boneweights[i].boneIndex1].MultiplyPoint(pos);
		p += bones[boneweights[i].boneIndex1].TransformPoint(bpos) * boneweights[i].weight1;

		bpos = bindposes[boneweights[i].boneIndex2].MultiplyPoint(pos);
		p += bones[boneweights[i].boneIndex2].TransformPoint(bpos) * boneweights[i].weight2;

		bpos = bindposes[boneweights[i].boneIndex3].MultiplyPoint(pos);
		p += bones[boneweights[i].boneIndex3].TransformPoint(bpos) * boneweights[i].weight3;

		return p;
	}

	Vector3 gcp = Vector3.zero;

	public Vector3 GetCoordMine(Vector3 A, Vector3 B, Vector3 C, Vector3 bary)
	{
		//Vector3 p = Vector3.zero;
		gcp.x = (bary.x * A.x) + (bary.y * B.x) + (bary.z * C.x);
		gcp.y = (bary.x * A.y) + (bary.y * B.y) + (bary.z * C.y);
		gcp.z = (bary.x * A.z) + (bary.y * B.z) + (bary.z * C.z);

		return gcp;
	}

	SkinnedMeshRenderer	tmesh;
#if UNITY_5 || UNITY_6
	Mesh	bakedmesh = null;
#endif

	void DoUpdate()
	{
		if ( WrapEnabled == false || target == null || bindverts == null )	//|| bindposes == null )
			return;

		if ( mesh == null )
			SetMesh();

		if ( mesh == null )
			return;

		if ( targetIsSkin && neededVerts != null && neededVerts.Count > 0 ) //|| (targetIsSkin && boneweights == null) )
		{
			if ( boneweights == null || tmesh == null )
			{
				tmesh = (SkinnedMeshRenderer)target.GetComponent(typeof(SkinnedMeshRenderer));
				if ( tmesh != null )
				{
					if ( !sourceIsSkin )
					{
						Mesh sm = tmesh.sharedMesh;
						bindposes = sm.bindposes;
						bones = tmesh.bones;
						boneweights = sm.boneWeights;
					}
				}
			}

#if UNITY_5 || UNITY_6
			if ( tmesh == null )
				tmesh = (SkinnedMeshRenderer)target.GetComponent(typeof(SkinnedMeshRenderer));

			if ( UseBakedMesh )
			{
				if ( bakedmesh == null )
					bakedmesh = new Mesh();

				tmesh.BakeMesh(bakedmesh);
				skinnedVerts = bakedmesh.vertices;
			}
			else
			{
				for ( int i = 0; i < neededVerts.Count; i++ )
					skinnedVerts[neededVerts[i]] = GetSkinPos(neededVerts[i]);
			}
#else
			for ( int i = 0; i < neededVerts.Count; i++ )
				skinnedVerts[neededVerts[i]] = GetSkinPos(neededVerts[i]);
#endif
		}

		Matrix4x4 stm = Matrix4x4.identity;

		Vector3 p = Vector3.zero;
		if ( targetIsSkin && !sourceIsSkin )
		{
#if UNITY_5 || UNITY_6
			if ( UseBakedMesh )
				stm = transform.worldToLocalMatrix * target.transform.localToWorldMatrix;	// * transform.worldToLocalMatrix;
			else
				stm = transform.worldToLocalMatrix;	// * target.transform.localToWorldMatrix;	// * transform.worldToLocalMatrix;
#else
			stm = transform.worldToLocalMatrix;	// * target.transform.localToWorldMatrix;	// * transform.worldToLocalMatrix;
#endif

			for ( int i = 0; i < bindverts.Length; i++ )
			{
				if ( bindverts[i].verts.Count > 0 )
				{
					p = Vector3.zero;
					float oow = 1.0f / bindverts[i].weight;

					int cnt = bindverts[i].verts.Count;

					for ( int j = 0; j < cnt; j++ )
					{
						MegaBindInf bi = bindverts[i].verts[j];

						Vector3 p0 = skinnedVerts[bi.i0];
						Vector3 p1 = skinnedVerts[bi.i1];
						Vector3 p2 = skinnedVerts[bi.i2];

						Vector3 cp = GetCoordMine(p0, p1, p2, bi.bary);
						Vector3 norm = FaceNormal(p0, p1, p2);

						float sq = 1.0f / Mathf.Sqrt(norm.x * norm.x + norm.y * norm.y + norm.z * norm.z);

						float d = (bi.dist * shrink) + gap;

						//cp += d * norm.x;
						cp.x += d * norm.x * sq;
						cp.y += d * norm.y * sq;
						cp.z += d * norm.z * sq;

						float bw = bi.weight * oow;

						if ( j == 0 )
						{
							p.x = cp.x * bw;
							p.y = cp.y * bw;
							p.z = cp.z * bw;
						}
						else
						{
							p.x += cp.x * bw;
							p.y += cp.y * bw;
							p.z += cp.z * bw;
						}
						//cp += ((bi.dist * shrink) + gap) * norm.normalized;
						//p += cp * (bi.weight / bindverts[i].weight);
					}

					Vector3 pp = stm.MultiplyPoint3x4(p);

					verts[i].x = pp.x + offset.x;
					verts[i].y = pp.y + offset.y;
					verts[i].z = pp.z + offset.z;
					//verts[i] = transform.InverseTransformPoint(p) + offset;
				}
			}
		}
		else
		{
			stm = transform.worldToLocalMatrix;	// * target.transform.localToWorldMatrix;	// * transform.worldToLocalMatrix;

			for ( int i = 0; i < bindverts.Length; i++ )
			{
				if ( bindverts[i].verts.Count > 0 )
				{
					p = Vector3.zero;
					float oow = 1.0f / bindverts[i].weight;

					for ( int j = 0; j < bindverts[i].verts.Count; j++ )
					{
						MegaBindInf bi = bindverts[i].verts[j];

						Vector3 p0 = target.sverts[bi.i0];
						Vector3 p1 = target.sverts[bi.i1];
						Vector3 p2 = target.sverts[bi.i2];

						Vector3 cp = GetCoordMine(p0, p1, p2, bi.bary);
						Vector3 norm = FaceNormal(p0, p1, p2);

						float sq = 1.0f / Mathf.Sqrt(norm.x * norm.x + norm.y * norm.y + norm.z * norm.z);

						float d = (bi.dist * shrink) + gap;

						//cp += d * norm.x;
						cp.x += d * norm.x * sq;
						cp.y += d * norm.y * sq;
						cp.z += d * norm.z * sq;

						float bw = bi.weight * oow;

						if ( j == 0 )
						{
							p.x = cp.x * bw;
							p.y = cp.y * bw;
							p.z = cp.z * bw;
						}
						else
						{
							p.x += cp.x * bw;
							p.y += cp.y * bw;
							p.z += cp.z * bw;
						}

						//cp += ((bi.dist * shrink) + gap) * norm.normalized;
						//p += cp * (bi.weight / bindverts[i].weight);
					}
				}
				else
					p = freeverts[i];	//startverts[i];

				Vector3 pp = stm.MultiplyPoint3x4(p);

				verts[i].x = pp.x + offset.x;
				verts[i].y = pp.y + offset.y;
				verts[i].z = pp.z + offset.z;

				//p = target.transform.TransformPoint(p);
				//verts[i] = transform.InverseTransformPoint(p) + offset;
			}
		}

		mesh.vertices = verts;
		RecalcNormals();
		mesh.RecalculateBounds();
	}

	public MegaNormMap[] mapping;
	public int[] tris;
	public Vector3[] facenorms;
	public Vector3[] norms;

	int[] FindFacesUsing(Vector3 p, Vector3 n)
	{
		List<int> faces = new List<int>();
		Vector3 v = Vector3.zero;

		for ( int i = 0; i < tris.Length; i += 3 )
		{
			v = verts[tris[i]];
			if ( v.x == p.x && v.y == p.y && v.z == p.z )
			{
				if ( n.Equals(norms[tris[i]]) )
					faces.Add(i / 3);
			}
			else
			{
				v = verts[tris[i + 1]];
				if ( v.x == p.x && v.y == p.y && v.z == p.z )
				{
					if ( n.Equals(norms[tris[i + 1]]) )
						faces.Add(i / 3);
				}
				else
				{
					v = verts[tris[i + 2]];
					if ( v.x == p.x && v.y == p.y && v.z == p.z )
					{
						if ( n.Equals(norms[tris[i + 2]]) )
							faces.Add(i / 3);
					}
				}
			}
		}

		return faces.ToArray();
	}

	// Should call this from inspector when we change to mega
	public void BuildNormalMapping(Mesh mesh, bool force)
	{
		if ( mapping == null || mapping.Length == 0 || force )
		{
			// so for each normal we have a vertex, so find all faces that share that vertex
			tris = mesh.triangles;
			norms = mesh.normals;
			facenorms = new Vector3[tris.Length / 3];
			mapping = new MegaNormMap[verts.Length];

			for ( int i = 0; i < verts.Length; i++ )
			{
				mapping[i] = new MegaNormMap();
				mapping[i].faces = FindFacesUsing(verts[i], norms[i]);
			}
		}
	}

	public void RecalcNormals()
	{
		if ( NormalMethod == MegaNormalMethod.Unity )	//|| mapping == null )
			mesh.RecalculateNormals();
		else
		{
			if ( mapping == null )
				BuildNormalMapping(mesh, false);

			RecalcNormals(mesh, verts);
		}
	}

	public void RecalcNormals(Mesh ms, Vector3[] _verts)
	{
		int index = 0;
		Vector3 v30 = Vector3.zero;
		Vector3 v31 = Vector3.zero;
		Vector3 v32 = Vector3.zero;
		Vector3 va = Vector3.zero;
		Vector3 vb = Vector3.zero;

		for ( int f = 0; f < tris.Length; f += 3 )
		{
			v30 = _verts[tris[f]];
			v31 = _verts[tris[f + 1]];
			v32 = _verts[tris[f + 2]];

			va.x = v31.x - v30.x;
			va.y = v31.y - v30.y;
			va.z = v31.z - v30.z;

			vb.x = v32.x - v31.x;
			vb.y = v32.y - v31.y;
			vb.z = v32.z - v31.z;

			v30.x = va.y * vb.z - va.z * vb.y;
			v30.y = va.z * vb.x - va.x * vb.z;
			v30.z = va.x * vb.y - va.y * vb.x;

			// Uncomment this if you dont want normals weighted by poly size
			//float l = v30.x * v30.x + v30.y * v30.y + v30.z * v30.z;
			//l = 1.0f / Mathf.Sqrt(l);
			//v30.x *= l;
			//v30.y *= l;
			//v30.z *= l;

			facenorms[index++] = v30;
		}

		for ( int n = 0; n < norms.Length; n++ )
		{
			if ( mapping[n].faces.Length > 0 )
			{
				Vector3 norm = facenorms[mapping[n].faces[0]];

				for ( int i = 1; i < mapping[n].faces.Length; i++ )
				{
					v30 = facenorms[mapping[n].faces[i]];
					norm.x += v30.x;
					norm.y += v30.y;
					norm.z += v30.z;
				}

				float l = norm.x * norm.x + norm.y * norm.y + norm.z * norm.z;
				l = 1.0f / Mathf.Sqrt(l);
				norm.x *= l;
				norm.y *= l;
				norm.z *= l;
				norms[n] = norm;
			}
			else
				norms[n] = Vector3.up;
		}

		ms.normals = norms;
	}
}
