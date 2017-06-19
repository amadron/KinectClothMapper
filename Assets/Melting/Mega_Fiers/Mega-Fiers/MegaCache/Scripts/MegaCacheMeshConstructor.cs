
using UnityEngine;
using System.Collections.Generic;

public class MegaCacheMatFaces
{
	public List<int>	tris = new List<int>();
}

public class MegaCacheFace
{
	public MegaCacheFace(Vector3 _v0, Vector3 _v1, Vector3 _v2, Vector3 _n0, Vector3 _n1, Vector3 _n2, Vector2 _uv0, Vector2 _uv1, Vector2 _uv2, int _sg, int _mid)
	{
		v30 = _v0;
		v31 = _v1;
		v32 = _v2;
		uv0 = _uv0;
		uv1 = _uv1;
		uv2 = _uv2;
		n0 = _n0;
		n1 = _n1;
		n2 = _n2;
		smthgrp = _sg;
		mtlid = _mid;
	}

	public int		v0,v1,v2;
	public Vector3	v30;
	public Vector3	v31;
	public Vector3	v32;
	public Vector3	n0 = Vector3.zero;
	public Vector3	n1 = Vector3.zero;
	public Vector3	n2 = Vector3.zero;
	public Vector2	uv0;
	public Vector2	uv1;
	public Vector2	uv2;
	public Vector2	uv10;
	public Vector2	uv11;
	public Vector2	uv12;
	public Color	col1;
	public Color	col2;
	public Color	col3;
	public int		smthgrp;
	public int		mtlid;
	public Vector3	faceNormal = Vector3.zero;
	public int		t0,t1,t2;
}

public class MegaCacheMeshConstructor
{
	static public List<Vector3>				verts	= new List<Vector3>();
	static public List<Vector3>				norms	= new List<Vector3>();
	static public List<Vector2>				uvs		= new List<Vector2>();
	static public List<int>					tris	= new List<int>();
	static public List<MegaCacheMatFaces>	matfaces = new List<MegaCacheMatFaces>();

	public class MegaCacheFaceGrid
	{
		public List<int>	verts = new List<int>();
	}

	static public MegaCacheFaceGrid[,,] checkgrid;
	static public Vector3 min;
	static public  Vector3 max;
	static public Vector3 size;
	static public int subdivs = 16;	//16;

	static public void BuildGrid(Vector3[] verts)
	{
		checkgrid = new MegaCacheFaceGrid[subdivs, subdivs, subdivs];

		min = verts[0];
		max = verts[0];

		for ( int i = 1; i < verts.Length; i++ )
		{
			Vector3 p = verts[i];

			if ( p.x < min.x )
				min.x = p.x;

			if ( p.x > max.x )
				max.x = p.x;

			if ( p.y < min.y )
				min.y = p.y;

			if ( p.y > max.y )
				max.y = p.y;

			if ( p.z < min.z )
				min.z = p.z;

			if ( p.z > max.z )
				max.z = p.z;
		}

		size = max - min;
	}

	static public void BuildTangents(Mesh mesh)
	{
		int triangleCount = mesh.triangles.Length;
		int vertexCount = mesh.vertices.Length;

		Vector3[]	tan1		= new Vector3[vertexCount];
		Vector3[]	tan2		= new Vector3[vertexCount];
		Vector4[]	tangents	= new Vector4[vertexCount];
		Vector3[]	verts		= mesh.vertices;
		Vector2[]	uvs			= mesh.uv;
		Vector3[]	norms		= mesh.normals;
		int[]		tris		= mesh.triangles;

		if ( uvs.Length > 0 )
		{
			for ( int a = 0; a < triangleCount; a += 3 )
			{
				long i1 = tris[a];
				long i2 = tris[a + 1];
				long i3 = tris[a + 2];

				Vector3 v1 = verts[i1];
				Vector3 v2 = verts[i2];
				Vector3 v3 = verts[i3];

				Vector2 w1 = uvs[i1];
				Vector2 w2 = uvs[i2];
				Vector2 w3 = uvs[i3];

				float x1 = v2.x - v1.x;
				float x2 = v3.x - v1.x;
				float y1 = v2.y - v1.y;
				float y2 = v3.y - v1.y;
				float z1 = v2.z - v1.z;
				float z2 = v3.z - v1.z;

				float s1 = w2.x - w1.x;
				float s2 = w3.x - w1.x;
				float t1 = w2.y - w1.y;
				float t2 = w3.y - w1.y;

				float r = 1.0f / (s1 * t2 - s2 * t1);

				Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
				Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

				tan1[i1] += sdir;
				tan1[i2] += sdir;
				tan1[i3] += sdir;

				tan2[i1] += tdir;
				tan2[i2] += tdir;
				tan2[i3] += tdir;
			}

			for ( int a = 0; a < vertexCount; a++ )
			{
				Vector3 n = norms[a].normalized;
				Vector3 t = tan1[a].normalized;

				Vector3.OrthoNormalize(ref n, ref t);
				tangents[a].x = t.x;
				tangents[a].y = t.y;
				tangents[a].z = t.z;
				tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
				tangents[a] = tangents[a].normalized;
			}

			mesh.tangents = tangents;
		}
	}
}

public class MegaCacheMeshConstructorOBJ : MegaCacheMeshConstructor
{
	static int FindVertGrid(Vector3 p, Vector3 n, Vector2 uv)
	{
		int sd = subdivs - 1;

		int gx = 0;
		int gy = 0;
		int gz = 0;

		if ( size.x > 0.0f )
			gx = (int)(sd * ((p.x - min.x) / size.x));

		if ( size.y > 0.0f )
			gy = (int)(sd * ((p.y - min.y) / size.y));

		if ( size.z > 0.0f )
			gz = (int)(sd * ((p.z - min.z) / size.z));

		MegaCacheFaceGrid fg = checkgrid[gx, gy, gz];

		if ( fg == null )
		{
			fg = new MegaCacheFaceGrid();
			checkgrid[gx, gy, gz] = fg;
		}

		for ( int i = 0; i < fg.verts.Count; i++ )
		{
			int ix = fg.verts[i];
			if ( verts[ix].x == p.x && verts[ix].y == p.y && verts[ix].z == p.z )
			{
				if ( norms[ix].x == n.x && norms[ix].y == n.y && norms[ix].z == n.z )
				{
					if ( uvs[ix].x == uv.x && uvs[ix].y == uv.y )
						return ix;
				}
			}
		}

		fg.verts.Add(verts.Count);

		verts.Add(p);
		norms.Add(n);
		uvs.Add(uv);

		return verts.Count - 1;
	}

	static public void Construct(List<MegaCacheFace> faces, Mesh mesh, Vector3[] meshverts, bool optimize, bool recalc, bool tangents)
	{
		mesh.Clear();

		if ( meshverts == null || meshverts.Length == 0 || faces.Count == 0 )
			return;

		verts.Clear();
		norms.Clear();
		uvs.Clear();
		tris.Clear();
		matfaces.Clear();

		BuildGrid(meshverts);

		int maxmat = 0;
		for ( int i = 0; i < faces.Count; i++ )
		{
			if ( faces[i].mtlid > maxmat )
				maxmat = faces[i].mtlid;
		}
		maxmat++;

		for ( int i = 0; i < maxmat; i++ )
			matfaces.Add(new MegaCacheMatFaces());

		for ( int i = 0; i < faces.Count; i++ )
		{
			int mtlid = faces[i].mtlid;
			int v0 = FindVertGrid(faces[i].v30, faces[i].n0, faces[i].uv0);
			int v1 = FindVertGrid(faces[i].v31, faces[i].n1, faces[i].uv1);
			int v2 = FindVertGrid(faces[i].v32, faces[i].n2, faces[i].uv2);

			matfaces[mtlid].tris.Add(v0);
			matfaces[mtlid].tris.Add(v1);
			matfaces[mtlid].tris.Add(v2);
		}

		mesh.vertices = verts.ToArray();

		mesh.subMeshCount = matfaces.Count;

		if ( recalc )
			mesh.RecalculateNormals();
		else
			mesh.normals = norms.ToArray();

		mesh.uv = uvs.ToArray();

		for ( int i = 0; i < matfaces.Count; i++ )
			mesh.SetTriangles(matfaces[i].tris.ToArray(), i);

		if ( tangents )
			BuildTangents(mesh);

		if ( optimize )
			;

		mesh.RecalculateBounds();

		checkgrid = null;
	}
}

public class MegaCacheMeshConstructorOBJNoUV : MegaCacheMeshConstructor
{
	static int FindVertGrid(Vector3 p, Vector3 n)
	{
		int sd = subdivs - 1;

		int gx = 0;
		int gy = 0;
		int gz = 0;

		if ( size.x > 0.0f )
			gx = (int)(sd * ((p.x - min.x) / size.x));

		if ( size.y > 0.0f )
			gy = (int)(sd * ((p.y - min.y) / size.y));

		if ( size.z > 0.0f )
			gz = (int)(sd * ((p.z - min.z) / size.z));

		MegaCacheFaceGrid fg = checkgrid[gx, gy, gz];

		if ( fg == null )
		{
			fg = new MegaCacheFaceGrid();
			checkgrid[gx, gy, gz] = fg;
		}

		for ( int i = 0; i < fg.verts.Count; i++ )
		{
			int ix = fg.verts[i];
			if ( verts[ix].x == p.x && verts[ix].y == p.y && verts[ix].z == p.z )
			{
				if ( norms[ix].x == n.x && norms[ix].y == n.y && norms[ix].z == n.z )
					return ix;
			}
		}

		fg.verts.Add(verts.Count);

		verts.Add(p);
		norms.Add(n);

		return verts.Count - 1;
	}

	static public void Construct(List<MegaCacheFace> faces, Mesh mesh, Vector3[] meshverts, bool optimize, bool recalc, bool tangents)
	{
		mesh.Clear();

		if ( meshverts == null || meshverts.Length == 0 || faces.Count == 0 )
			return;

		verts.Clear();
		norms.Clear();
		tris.Clear();
		matfaces.Clear();

		BuildGrid(meshverts);

		int maxmat = 0;
		for ( int i = 0; i < faces.Count; i++ )
		{
			if ( faces[i].mtlid > maxmat )
				maxmat = faces[i].mtlid;
		}
		maxmat++;

		for ( int i = 0; i < maxmat; i++ )
		{
			matfaces.Add(new MegaCacheMatFaces());
		}

		for ( int i = 0; i < faces.Count; i++ )
		{
			int mtlid = faces[i].mtlid;
			int v0 = FindVertGrid(faces[i].v30, faces[i].n0);
			int v1 = FindVertGrid(faces[i].v31, faces[i].n1);
			int v2 = FindVertGrid(faces[i].v32, faces[i].n2);

			matfaces[mtlid].tris.Add(v0);
			matfaces[mtlid].tris.Add(v1);
			matfaces[mtlid].tris.Add(v2);
		}

		mesh.vertices = verts.ToArray();

		mesh.subMeshCount = matfaces.Count;

		if ( recalc )
			mesh.RecalculateNormals();
		else
			mesh.normals = norms.ToArray();

		for ( int i = 0; i < matfaces.Count; i++ )
			mesh.SetTriangles(matfaces[i].tris.ToArray(), i);

		if ( tangents )
			BuildTangents(mesh);

		if ( optimize )
			;

		mesh.RecalculateBounds();

		checkgrid = null;
	}
}