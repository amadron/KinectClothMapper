
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;

public class MegaCacheOBJMtl
{
	public string		name;
	public Color		Ka;
	public Color		Kd;
	public Color		Ks;
	public Color		Tf;
	public float		Tr;
	public Color		Ke;
	public float		Ns;
	public float		Ni;
	public float		d;
	public int			illum;
	public string		map_Ka;
	public string		map_Kd;
	public Texture2D	Kdtexture;
}

public class MegaCacheObjImporter
{
	class MegaCacheOBJFace
	{
		public int[]	v		= new int[4];
		public int[]	uv		= new int[4];
		public int[]	n		= new int[4];
		public bool		quad	= false;
		public int		smthgrp;
		public int		mtl;
	}

	class MegaCacheOBJMesh
	{
		public List<Vector3>			vertices	= new List<Vector3>();
		public List<Vector3>			normals		= new List<Vector3>();
		public List<Vector2>			uv			= new List<Vector2>();
		public List<Vector2>			uv1			= new List<Vector2>();
		public List<Vector2>			uv2			= new List<Vector2>();
		public List<MegaCacheOBJFace>	faces		= new List<MegaCacheOBJFace>();
	}

	static List<MegaCacheOBJMtl>	mtls	= new List<MegaCacheOBJMtl>();
	static List<MegaCacheFace>		faces	= new List<MegaCacheFace>();
	static int						currentmtl;
	static int						smthgrp;
	static int						offset = 0;
	static MegaCacheOBJMtl			loadmtl;

	static public void Init()
	{
		mtls.Clear();
	}

	static public int NumMtls()
	{
		return mtls.Count;
	}

	static public MegaCacheOBJMtl GetMtl(int i)
	{
		return mtls[i];
	}

	static public Mesh ImportFile(string filePath, float scale, bool adjust, bool tangents, bool loadmtls)
	{
		faces.Clear();

		StreamReader stream = File.OpenText(filePath);
		string entireText = stream.ReadToEnd();
		stream.Close();

		MegaCacheOBJMesh newMesh = new MegaCacheOBJMesh();
		populateMeshStructNew(entireText, ref newMesh, loadmtls);
		Mesh mesh = new Mesh();

		int v1 = 0;
		int v2 = 1;
		int v3 = 2;
		int v4 = 3;
		currentmtl = 0;
		smthgrp = 0;

		for ( int i = 0; i < newMesh.vertices.Count; i++ )
		{
			newMesh.vertices[i] *= scale;

			if ( adjust )
			{
				Vector3 p = newMesh.vertices[i];
				p.x = -p.x;
				newMesh.vertices[i] = p;
			}
		}

		for ( int i = 0; i < newMesh.normals.Count; i++ )
		{
			Vector3 p = newMesh.normals[i];
			p.x = -p.x;
			newMesh.normals[i] = p;
		}

		Vector3 n1 = Vector3.forward;
		Vector3 n2 = Vector3.forward;
		Vector3 n3 = Vector3.forward;
		Vector3 n4 = Vector3.forward;

		if ( newMesh.uv.Count == 0 )
		{
			for ( int t = 0; t < newMesh.faces.Count; t++ )
			{
				MegaCacheOBJFace f = newMesh.faces[t];

				if ( newMesh.normals.Count > 0 )
				{
					n1 = newMesh.normals[f.n[v1]];
					n2 = newMesh.normals[f.n[v2]];
					n3 = newMesh.normals[f.n[v3]];
					if ( f.quad )
						n4 = newMesh.normals[f.n[v4]];
				}

				if ( adjust )
					faces.Add(new MegaCacheFace(newMesh.vertices[f.v[v1]], newMesh.vertices[f.v[v3]], newMesh.vertices[f.v[v2]], n1, n3, n2, Vector3.zero, Vector3.zero, Vector3.zero, 1, f.mtl));
				else
					faces.Add(new MegaCacheFace(newMesh.vertices[f.v[v1]], newMesh.vertices[f.v[v2]], newMesh.vertices[f.v[v3]], n1, n2, n3, Vector3.zero, Vector3.zero, Vector3.zero, 1, f.mtl));

				if ( f.quad )
				{
					if ( adjust )
						faces.Add(new MegaCacheFace(newMesh.vertices[f.v[v1]], newMesh.vertices[f.v[v4]], newMesh.vertices[f.v[v3]], n1, n4, n3, Vector3.zero, Vector3.zero, Vector3.zero, 1, f.mtl));
					else
						faces.Add(new MegaCacheFace(newMesh.vertices[f.v[v1]], newMesh.vertices[f.v[v3]], newMesh.vertices[f.v[v4]], n1, n3, n4, Vector3.zero, Vector3.zero, Vector3.zero, 1, f.mtl));
				}
			}

			MegaCacheMeshConstructorOBJNoUV.Construct(faces, mesh, newMesh.vertices.ToArray(), true, false, tangents);
		}
		else
		{
			for ( int t = 0; t < newMesh.faces.Count; t++ )
			{
				MegaCacheOBJFace f = newMesh.faces[t];

				if ( newMesh.normals.Count > 0 )
				{
					n1 = newMesh.normals[f.n[v1]];
					n2 = newMesh.normals[f.n[v2]];
					n3 = newMesh.normals[f.n[v3]];
					if ( f.quad )
						n4 = newMesh.normals[f.n[v4]];
				}

				if ( adjust )
					faces.Add(new MegaCacheFace(newMesh.vertices[f.v[v1]], newMesh.vertices[f.v[v3]], newMesh.vertices[f.v[v2]], n1, n3, n2, newMesh.uv[f.uv[v1]], newMesh.uv[f.uv[v3]], newMesh.uv[f.uv[v2]], 1, f.mtl));
				else
					faces.Add(new MegaCacheFace(newMesh.vertices[f.v[v1]], newMesh.vertices[f.v[v2]], newMesh.vertices[f.v[v3]], n1, n2, n3, newMesh.uv[f.uv[v1]], newMesh.uv[f.uv[v2]], newMesh.uv[f.uv[v3]], 1, f.mtl));

				if ( f.quad )
				{
					if ( adjust )
						faces.Add(new MegaCacheFace(newMesh.vertices[f.v[v1]], newMesh.vertices[f.v[v4]], newMesh.vertices[f.v[v3]], n1, n4, n3, newMesh.uv[f.uv[v1]], newMesh.uv[f.uv[v4]], newMesh.uv[f.uv[v3]], 1, f.mtl));
					else
						faces.Add(new MegaCacheFace(newMesh.vertices[f.v[v1]], newMesh.vertices[f.v[v3]], newMesh.vertices[f.v[v4]], n1, n3, n4, newMesh.uv[f.uv[v1]], newMesh.uv[f.uv[v3]], newMesh.uv[f.uv[v4]], 1, f.mtl));
				}
			}

			MegaCacheMeshConstructorOBJ.Construct(faces, mesh, newMesh.vertices.ToArray(), false, false, tangents);
		}

		return mesh;
	}

	static public string ReadLine(string input)
	{
		StringBuilder sb = new StringBuilder();
		while ( true )
		{
			if ( offset >= input.Length )
				break;

			int ch = input[offset++];
			if ( ch == '\r' || ch == '\n' )
			{
				while ( offset < input.Length )
				{
					int ch1 = input[offset++];
					if ( ch1 != '\n' )
					{
						offset--;
						break;
					}
				}

				return sb.ToString();
			}
			sb.Append((char)ch);
		}

		if ( sb.Length > 0 )
			return sb.ToString();

		return null;
	}

	static void populateMeshStructNew(string entireText, ref MegaCacheOBJMesh mesh, bool loadmtls)
	{
		offset = 0;
		string currentText = ReadLine(entireText);

		char[] splitIdentifier = { ' ' };
		char[] splitIdentifier2 = { '/' };
		string[] brokenString;
		string[] brokenBrokenString;

		while ( currentText != null )
		{
			currentText = currentText.Trim();
			brokenString = currentText.Split(splitIdentifier, 50);
			switch ( brokenString[0] )
			{
				case "g": break;
				case "usemtl":
					if ( loadmtls )
						currentmtl = GetMtlID(brokenString[1]);
					else
						currentmtl = 0;
					break;

				case "usemap": break;
				case "mtllib": break;
				case "v": mesh.vertices.Add(new Vector3(float.Parse(brokenString[1]), float.Parse(brokenString[2]), float.Parse(brokenString[3]))); break;
				case "vt": mesh.uv.Add(new Vector2(float.Parse(brokenString[1]), float.Parse(brokenString[2]))); break;
				case "vt1": mesh.uv1.Add(new Vector2(float.Parse(brokenString[1]), float.Parse(brokenString[2]))); break;
				case "vt2": mesh.uv2.Add(new Vector2(float.Parse(brokenString[1]), float.Parse(brokenString[2]))); break;
				case "vn": mesh.normals.Add(new Vector3(float.Parse(brokenString[1]), float.Parse(brokenString[2]), float.Parse(brokenString[3]))); break;
				case "vc": break;
				case "f":
					int j = 1;

					MegaCacheOBJFace oface = new MegaCacheOBJFace();

					oface.mtl = currentmtl;
					oface.smthgrp = smthgrp;

					while ( j < brokenString.Length && ("" + brokenString[j]).Length > 0 )
					{
						brokenBrokenString = brokenString[j].Split(splitIdentifier2, 3);

						if ( j == 4 )
							oface.quad = true;

						oface.v[j - 1] = int.Parse(brokenBrokenString[0]) - 1;

						if ( brokenBrokenString.Length > 1 )
						{
							if ( brokenBrokenString[1] != "" )
								oface.uv[j - 1] = int.Parse(brokenBrokenString[1]) - 1;

							if ( brokenBrokenString.Length > 2 )
								oface.n[j - 1] = int.Parse(brokenBrokenString[2]) - 1;
						}
						j++;
					}
					mesh.faces.Add(oface);
					break;

				case "s": break;
			}

			currentText = ReadLine(entireText);

			if ( currentText != null )
				currentText = currentText.Replace("  ", " ");
		}
	}

	static public void ImportMtl(string filePath)
	{
		LoadMtlLib(filePath);
	}

	static MegaCacheOBJMtl HaveMaterial(string name)
	{
		for ( int i = 0; i < mtls.Count; i++ )
		{
			if ( mtls[i].name == name )
			{
				return mtls[i];
			}
		}

		MegaCacheOBJMtl mtl = new MegaCacheOBJMtl();
		mtl.name = name;
		mtls.Add(mtl);
		return mtl;
	}

	static int GetMtlID(string name)
	{
		if ( mtls.Count > 0 )
		{
			for ( int i = 0; i < mtls.Count; i++ )
			{
				if ( mtls[i].name == name )
				{
					return i;
				}
			}

			Debug.Log("Missing Material " + name);
		}
		return 0;
	}

#if false
	static public Texture2D LoadTexture(string filename)
	{
		Texture2D tex = null;

		if ( File.Exists(filename) )
		{
			byte[] buf = File.ReadAllBytes(filename);

			tex = new Texture2D(2, 2);
			tex.LoadImage(buf);
		}

		return tex;
	}
#endif

	static void LoadMtlLib(string filename)
	{
		string path = Path.GetDirectoryName(filename);

		StreamReader stream = File.OpenText(filename);
		string entireText = stream.ReadToEnd();
		stream.Close();

		using ( StringReader reader = new StringReader(entireText) )
		{
			string currentText = reader.ReadLine();
			char[] splitIdentifier = { ' ' };
			string[] brokenString;

			while ( currentText != null )
			{
				currentText = currentText.Trim();
				brokenString = currentText.Split(splitIdentifier, 50);
				switch ( brokenString[0] )
				{
					case "newmtl":
						MegaCacheOBJMtl mtl = HaveMaterial(brokenString[1]);
						loadmtl = mtl;
						break;

					case "Ns":
						loadmtl.Ns = float.Parse(brokenString[1]);
						break;

					case "Ni":
						loadmtl.Ni = float.Parse(brokenString[1]);
						break;

					case "d":
						loadmtl.d = float.Parse(brokenString[1]);
						break;

					case "Tr":
						loadmtl.Tr = float.Parse(brokenString[1]);
						break;

					case "Tf":
						loadmtl.Tf.r = float.Parse(brokenString[1]);
						loadmtl.Tf.g = float.Parse(brokenString[2]);
						loadmtl.Tf.b = float.Parse(brokenString[3]);
						break;

					case "illum":
						loadmtl.illum = int.Parse(brokenString[1]);
						break;

					case "Ka":
						loadmtl.Ka.r = float.Parse(brokenString[1]);
						loadmtl.Ka.g = float.Parse(brokenString[2]);
						loadmtl.Ka.b = float.Parse(brokenString[3]);
						break;

					case "Kd":
						loadmtl.Kd.r = float.Parse(brokenString[1]);
						loadmtl.Kd.g = float.Parse(brokenString[2]);
						loadmtl.Kd.b = float.Parse(brokenString[3]);
						break;

					case "Ks":
						loadmtl.Ks.r = float.Parse(brokenString[1]);
						loadmtl.Ks.g = float.Parse(brokenString[2]);
						loadmtl.Ks.b = float.Parse(brokenString[3]);
						break;

					case "Ke":
						loadmtl.Ke.r = float.Parse(brokenString[1]);
						loadmtl.Ke.g = float.Parse(brokenString[2]);
						loadmtl.Ke.b = float.Parse(brokenString[3]);
						break;

					case "map_Ka":
						loadmtl.map_Ka = brokenString[1];
						break;

					case "map_Kd":
						string dir = Path.GetDirectoryName(brokenString[1]);

						if ( dir.Length == 0 )
							loadmtl.map_Kd = path + "/" + brokenString[1];
						else
							loadmtl.map_Kd = brokenString[1];
						break;
				}

				currentText = reader.ReadLine();
				if ( currentText != null )
					currentText = currentText.Replace("  ", " ");
			}
		}
	}
}