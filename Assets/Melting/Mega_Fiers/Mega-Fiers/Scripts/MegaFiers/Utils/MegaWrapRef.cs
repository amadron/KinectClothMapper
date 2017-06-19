
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MegaWrapRef : MonoBehaviour
{
	public float			gap				= 0.0f;
	public float			shrink			= 1.0f;
	public Vector3[]		skinnedVerts;
	public Mesh				mesh			= null;
	public Vector3			offset			= Vector3.zero;
	public bool				targetIsSkin	= false;
	public bool				sourceIsSkin	= false;
	public int				nomapcount		= 0;
	public Matrix4x4[]		bindposes;
	public Transform[]		bones;
	public float			size			= 0.01f;
	public int				vertindex		= 0;
	public Vector3[]		verts;
	public MegaModifyObject	target;
	public float			maxdist			= 0.25f;
	public int				maxpoints		= 4;
	public bool				WrapEnabled		= true;

	public MegaWrap			source;
	public MegaNormalMethod	NormalMethod = MegaNormalMethod.Unity;

	struct MegaCloseFace
	{
		public int		face;
		public float	dist;
	}

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

	float GetDistance(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		return MegaNearestPointTest.DistPoint3Triangle3Dbl(p, p0, p1, p2);
	}

	float GetPlaneDistance(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
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

	public Vector3 FaceNormal(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector3 e1 = p1 - p0;
		Vector3 e2 = p2 - p0;
		return Vector3.Cross(e1, e2);
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

	Mesh CloneMesh(Mesh m)
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
		if ( mesh && source )
		{
			mesh.vertices = source.startverts;
			mesh.RecalculateBounds();
			RecalcNormals();
		}

		target = null;
	}

	public void Attach(MegaModifyObject modobj)
	{
		targetIsSkin = false;
		sourceIsSkin = false;

		nomapcount = 0;

		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh srcmesh = null;

		if ( mf != null )
			srcmesh = mf.mesh;
		else
		{
			SkinnedMeshRenderer smesh = (SkinnedMeshRenderer)GetComponent(typeof(SkinnedMeshRenderer));

			if ( smesh != null )
			{
				srcmesh = smesh.sharedMesh;
				sourceIsSkin = true;
			}
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
					bones = tmesh.bones;
					skinnedVerts = sm.vertices;	//new Vector3[sm.vertexCount];
				}
			}
		}

		verts = mesh.vertices;
	}

	void LateUpdate()
	{
		DoUpdate();
	}

	Vector3 GetSkinPos(MegaWrap src, int i)
	{
		Vector3 pos = target.sverts[i];
		Vector3 bpos = bindposes[src.boneweights[i].boneIndex0].MultiplyPoint(pos);
		Vector3 p = bones[src.boneweights[i].boneIndex0].TransformPoint(bpos) * src.boneweights[i].weight0;

		bpos = bindposes[src.boneweights[i].boneIndex1].MultiplyPoint(pos);
		p += bones[src.boneweights[i].boneIndex1].TransformPoint(bpos) * src.boneweights[i].weight1;

		bpos = bindposes[src.boneweights[i].boneIndex2].MultiplyPoint(pos);
		p += bones[src.boneweights[i].boneIndex2].TransformPoint(bpos) * src.boneweights[i].weight2;

		bpos = bindposes[src.boneweights[i].boneIndex3].MultiplyPoint(pos);
		p += bones[src.boneweights[i].boneIndex3].TransformPoint(bpos) * src.boneweights[i].weight3;

		return p;
	}

	public Vector3 GetCoordMine(Vector3 A, Vector3 B, Vector3 C, Vector3 bary)
	{
		Vector3 p = Vector3.zero;
		p.x = (bary.x * A.x) + (bary.y * B.x) + (bary.z * C.x);
		p.y = (bary.x * A.y) + (bary.y * B.y) + (bary.z * C.y);
		p.z = (bary.x * A.z) + (bary.y * B.z) + (bary.z * C.z);

		return p;
	}

	void DoUpdate()
	{
		if ( source == null || WrapEnabled == false || target == null || source.bindverts == null )	//|| bindposes == null )
			return;

		if ( targetIsSkin && source.neededVerts != null && source.neededVerts.Count > 0 )
		{
			if ( source.boneweights == null )
			{
				SkinnedMeshRenderer tmesh = (SkinnedMeshRenderer)target.GetComponent(typeof(SkinnedMeshRenderer));
				if ( tmesh != null )
				{
					if ( !sourceIsSkin )
					{
						Mesh sm = tmesh.sharedMesh;
						bindposes = sm.bindposes;
						source.boneweights = sm.boneWeights;
					}
				}
			}

			for ( int i = 0; i < source.neededVerts.Count; i++ )
				skinnedVerts[source.neededVerts[i]] = GetSkinPos(source, source.neededVerts[i]);
		}

		Vector3 p = Vector3.zero;
		if ( targetIsSkin && !sourceIsSkin )
		{
			for ( int i = 0; i < source.bindverts.Length; i++ )
			{
				if ( source.bindverts[i].verts.Count > 0 )
				{
					p = Vector3.zero;

					for ( int j = 0; j < source.bindverts[i].verts.Count; j++ )
					{
						MegaBindInf bi = source.bindverts[i].verts[j];

						Vector3 p0 = skinnedVerts[bi.i0];
						Vector3 p1 = skinnedVerts[bi.i1];
						Vector3 p2 = skinnedVerts[bi.i2];

						Vector3 cp = GetCoordMine(p0, p1, p2, bi.bary);
						Vector3 norm = FaceNormal(p0, p1, p2);
						cp += ((bi.dist * shrink) + gap) * norm.normalized;
						p += cp * (bi.weight / source.bindverts[i].weight);
					}

					verts[i] = transform.InverseTransformPoint(p) + offset;
				}
			}
		}
		else
		{
			for ( int i = 0; i < source.bindverts.Length; i++ )
			{
				if ( source.bindverts[i].verts.Count > 0 )
				{
					p = Vector3.zero;

					for ( int j = 0; j < source.bindverts[i].verts.Count; j++ )
					{
						MegaBindInf bi = source.bindverts[i].verts[j];

						Vector3 p0 = target.sverts[bi.i0];
						Vector3 p1 = target.sverts[bi.i1];
						Vector3 p2 = target.sverts[bi.i2];

						Vector3 cp = GetCoordMine(p0, p1, p2, bi.bary);
						Vector3 norm = FaceNormal(p0, p1, p2);
						cp += ((bi.dist * shrink) + gap) * norm.normalized;
						p += cp * (bi.weight / source.bindverts[i].weight);
					}
				}
				else
					p = source.freeverts[i];	//startverts[i];

				p = target.transform.TransformPoint(p);
				verts[i] = transform.InverseTransformPoint(p) + offset;
			}
		}

		mesh.vertices = verts;
		RecalcNormals();
		mesh.RecalculateBounds();
	}

	[HideInInspector]
	public MegaNormMap[] mapping;
	[HideInInspector]
	public int[] tris;
	[HideInInspector]
	public Vector3[] facenorms;
	[HideInInspector]
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