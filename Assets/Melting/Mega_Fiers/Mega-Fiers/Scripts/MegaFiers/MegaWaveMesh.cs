
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MegaWaveMesh : MonoBehaviour
{
	[HideInInspector]
	public float offset = 0.0f;

	public float Width = 1.0f;
	public float Height = 1.0f;
	public float Length = 0.0f;

	public int	 WidthSegs = 1;

	public bool	GenUVs = true;
	public bool	recalcBounds = false;
	public bool recalcNormals = false;
	public bool recalcCollider = false;

	public float mspeed = 1.0f;

	public float flex = 1.0f;
	public float amp = 0.0f;
	public float wave = 1.0f;
	public float phase = 0.0f;
	public float mtime = 0.0f;
	public float speed = 1.0f;
	float dist = 0.0f;
	float time = 0.0f;

	public float flex1 = 1.0f;
	public float amp1 = 0.0f;
	public float wave1 = 1.0f;
	public float phase1 = 0.0f;
	public float mtime1 = 0.0f;
	public float speed1 = 1.0f;
	float dist1 = 0.0f;
	float time1 = 0.0f;

	public float flex2 = 1.0f;
	public float amp2 = 0.0f;
	public float wave2 = 1.0f;
	public float phase2 = 0.0f;
	public float mtime2 = 0.0f;
	public float speed2 = 1.0f;
	float dist2 = 0.0f;
	float time2 = 0.0f;

	public float amount = 1.0f;
	[HideInInspector]
	public int surfacestart = 0;
	[HideInInspector]
	public int surfaceend = 1;

	[HideInInspector]
	public Vector3[]	verts;
	[HideInInspector]
	public Vector2[]	uvs;
	[HideInInspector]
	public int[]		tris;

	[HideInInspector]
	public float surface = 0.0f;	// top value for first map
	public bool	linkOffset = false;

	[HideInInspector]
	public bool rebuild = true;

	Material mat;

	public Vector2	UVOffset = Vector2.zero;
	public Vector2	UVScale = Vector2.one;

	[HideInInspector]
	public Mesh mesh;

	void Reset()
	{
		Rebuild();
	}

	public void Rebuild()
	{
		MeshFilter mf = GetComponent<MeshFilter>();

		if ( mf != null )
		{
			Mesh mesh1 = mf.sharedMesh;	//Utils.GetMesh(gameObject);

			if ( mesh1 == null )
			{
				mesh1 = new Mesh();
				mf.sharedMesh = mesh1;
			}
			mesh = mesh1;

			if ( mesh != null )
			{
				BuildMesh(mesh);
				MegaModifyObject mo = GetComponent<MegaModifyObject>();
				if ( mo != null )
				{
					mo.MeshUpdated();
				}
			}
		}
	}

	public MeshCollider meshCol;
	public Mesh			colmesh;
	public bool			smooth = true;

	void Update()
	{
		if ( mesh == null )
			Rebuild();

		if ( linkOffset )
			offset = transform.position.x;

		if ( mat == null )
		{
			MeshRenderer mr = GetComponent<MeshRenderer>();

			if ( mr )
				mat = mr.sharedMaterial;
		}

		if ( mat )
		{
			//float a = Width / mat.mainTexture.width;
			Vector3 off = mat.mainTextureOffset;
			off.x = offset / Width;	// * 2.0f);
			mat.mainTextureOffset = off;
		}

		if ( wave == 0.0f )
			wave = 0.0000001f;

		if ( wave1 == 0.0f )
			wave1 = 0.0000001f;

		if ( wave2 == 0.0f )
			wave2 = 0.0000001f;

		if ( rebuild )
		{
			BuildMesh(mesh);
		}
		else
		{
			UpdateSurface();
			mesh.vertices = verts;

			if ( recalcNormals )
				mesh.RecalculateNormals();

			if ( recalcBounds )
				mesh.RecalculateBounds();
		}

		if ( recalcCollider )
		{
			Rigidbody rb = GetComponent<Rigidbody>();
			if ( rb )
			{
				rb.inertiaTensor = Vector3.one;
				rb.inertiaTensorRotation = Quaternion.identity;
			}

			if ( meshCol == null )
			{
				meshCol = GetComponent<MeshCollider>();
				if ( meshCol == null )
				{
					meshCol = gameObject.AddComponent<MeshCollider>();
				}
			}

			if ( meshCol != null )
			{
				if ( colmesh == null )
				{
					colmesh = new Mesh();
					colmesh.Clear();
				}

				BuildCollider(colmesh);
				meshCol.sharedMesh = null;
				meshCol.sharedMesh = colmesh;
			}
		}

		mtime += Time.deltaTime * speed * mspeed;
		mtime1 += Time.deltaTime * speed1 * mspeed;
		mtime2 += Time.deltaTime * speed2 * mspeed;
	}

	Vector3[] colverts;

	public float colwidth = 1.0f;

	void BuildCollider(Mesh cmesh)
	{
		bool setris = false;
		if ( colverts == null || colverts.Length != verts.Length )
		{
			colverts = new Vector3[verts.Length];

			setris = true;
		}

		for ( int i = 0; i < surfaceend; i++ )
		{
			Vector3 p = verts[i];
			p.z += colwidth;
			colverts[i] = p;
			p.z -= 2.0f * colwidth;
			colverts[i + surfaceend] = p;
		}

		colmesh.vertices = colverts;
		if ( setris )
		{
			colmesh.triangles = tris;	//coltris;
		}
	}

	void MakeQuad1(int f, int a, int b, int c, int d)
	{
		tris[f++] = c;
		tris[f++] = b;
		tris[f++] = a;

		tris[f++] = a;
		tris[f++] = d;
		tris[f++] = c;
	}

	// Put in utils
	int MaxComponent(Vector3 v)
	{
		if ( Mathf.Abs(v.x) > Mathf.Abs(v.y) )
		{
			if ( Mathf.Abs(v.x) > Mathf.Abs(v.z) )
				return 0;
			else
				return 2;
		}
		else
		{
			if ( Mathf.Abs(v.y) > Mathf.Abs(v.z) )
				return 1;
			else
				return 2;
		}
	}

	static public float WaveFunc(float radius, float t, float amp, float waveLen, float phase)	//, float decay)
	{
		float ang = Mathf.PI * 2.0f * (radius / waveLen + phase);
		return amp * Mathf.Sin(ang);	// * Mathf.Exp(-decay * Mathf.Abs(radius));
	}

	static public float WaveFunc1(float radius, float t, float amp, float waveLen, float phase)	//, float decay)
	{
		float ang = Mathf.Repeat(Mathf.PI * 2.0f * (radius / waveLen + phase), Mathf.PI * 2.0f);
		if ( ang < Mathf.PI )
			return -amp * Mathf.Sin(ang);	// * Mathf.Exp(-decay * Mathf.Abs(radius));

		return amp * Mathf.Sin(ang);	// * Mathf.Exp(-decay * Mathf.Abs(radius));
	}

	public float Map(Vector3 p)
	{
		float u = Mathf.Abs(2.0f * p.y / dist);
		u = u * u;

		p.y = 0.0f;

		p.y += amount * flex * WaveFunc(p.x + offset, time, amp, wave, phase + mtime);	//, dy);
		p.y += amount * flex1 * WaveFunc(p.x + offset, time1, amp1, wave1, phase1 + mtime1);	//, dy1);
		p.y += amount * flex2 * WaveFunc(p.x + offset, time2, amp2, wave2, phase2 + mtime2);	//, dy2);

		return p.y + surface;
	}

	// Update for just the top verts to make quicker
	void UpdateSurface()
	{
		dist = (wave / 10.0f) * 4.0f * 5.0f;	//float(numSides);

		if ( dist == 0.0f )
			dist = 1.0f;

		dist1 = (wave1 / 10.0f) * 4.0f * 5.0f;	//float(numSides);

		if ( dist1 == 0.0f )
			dist1 = 1.0f;

		dist2 = (wave2 / 10.0f) * 4.0f * 5.0f;	//float(numSides);

		if ( dist2 == 0.0f )
			dist2 = 1.0f;

		for ( int i = surfacestart; i < surfaceend; i++ )
			verts[i].y = Map(verts[i]);
	}

	// Only call this on size or seg change
	void BuildMesh(Mesh mesh)
	{
		Width = Mathf.Clamp(Width, 0.0f, float.MaxValue);
		Length = Mathf.Clamp(Length, 0.0f, float.MaxValue);
		Height = Mathf.Clamp(Height, 0.0f, float.MaxValue);

		WidthSegs = Mathf.Clamp(WidthSegs, 1, 200);

		Vector3 vb = new Vector3(Width, Height, Length) / 2.0f;
		Vector3 va = Vector3.zero;
		va.x = -vb.x;
		va.y = vb.y;
		va.z = vb.z;

		float mdx = Width / (float)WidthSegs;
		float mdy = Height;	// / (float)HeightSegs;

		Vector3 p = va;

		int numverts = 2 * (WidthSegs + 1);

		surfacestart = 0;
		surfaceend = WidthSegs + 1;

		verts = new Vector3[numverts];
		uvs = new Vector2[numverts];

		tris = new int[WidthSegs * 2 * 3];

		Vector2 uv = Vector2.zero;

		int index = 0;
		surface = va.y;

		p.z = va.z;
		p.y = va.y;
		p.x = va.x;

		for ( int ix = 0; ix <= WidthSegs; ix++ )
		{
			verts[index] = p;
			if ( GenUVs )
			{
				uv.x = ((p.x + vb.x + UVOffset.x) / Width) * UVScale.x;
				uv.y = ((p.y + vb.y + UVOffset.y) / Height) * UVScale.y;
				uvs[index] = uv;
			}
			index++;
			p.x += mdx;
		}

		p.y -= mdy;
		p.x = va.x;

		for ( int ix = 0; ix <= WidthSegs; ix++ )
		{
			verts[index] = p;
			if ( GenUVs )
			{
				uv.x = ((p.x + vb.x + UVOffset.x) / Width) * UVScale.x;
				uv.y = ((p.y + vb.y + UVOffset.y) / Height) * UVScale.y;
				uvs[index] = uv;
			}
			p.x += mdx;
			index++;
		}

		int f = 0;
		int kv = 0;	//iz * (WidthSegs + 1) + index;
		for ( int ix = 0; ix < WidthSegs; ix++ )
		{
			MakeQuad1(f, kv, kv + WidthSegs + 1, kv + WidthSegs + 2, kv + 1);
			f += 6;
			kv++;
		}

		UpdateSurface();
		mesh.Clear();
		mesh.subMeshCount = 1;
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.SetTriangles(tris, 0);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
}