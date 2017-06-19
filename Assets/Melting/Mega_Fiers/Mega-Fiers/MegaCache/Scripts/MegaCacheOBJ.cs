
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public enum MegaCacheData
{
	Mesh,
	File,
	Image,
}

[System.Serializable]
public class MegaCacheImageFace
{
	public int		max;
	public int[]	tris;
}

[System.Serializable]
public enum MegaCacheRepeatMode
{
	Loop,
	Clamp,
	PingPong,
};

[AddComponentMenu("MegaFiers/OBJ Cache")]
[ExecuteInEditMode, RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class MegaCacheOBJ : MonoBehaviour
{
	public List<Mesh>			meshes			= new List<Mesh>();
	public int					frame			= 0;
	public bool					animate			= false;
	public float				time			= 0.0f;
	public float				speed			= 1.0f;
	public float				looptime		= 5.0f;
	public float				fps				= 25.0f;
	public MegaCacheRepeatMode	loopmode		= MegaCacheRepeatMode.Loop;
	public int					firstframe		= 0;
	public int					lastframe		= 1;
	public int					skip			= 0;
	public string				lastpath		= "";
	public string				cachefile		= "";
	public int					framevertcount	= 0;
	public int					frametricount	= 0;
	public float				scale			= 1.0f;
	public bool					adjustcoord		= true;
	public bool					buildtangents	= false;
	public bool					updatecollider	= false;
	public bool					saveuvs			= true;
	public bool					savenormals		= true;
	public bool					savetangents	= true;
	public bool					optimize		= true;
	public bool					update			= false;
	public bool					loadmtls		= false;
	public MegaCacheData		datasource		= MegaCacheData.Mesh;
	public MegaCacheImage		cacheimage;
	public MeshFilter			mf;
	public int					framecount		= 0;
	public Vector3[]			vertcache;
	public Vector3[]			normcache;
	public Vector4[]			tangentcache;
	public Vector2[]			uvcache;
	public MegaCacheImageFace[]	subs;
	public int					decformat		= 0;
	public bool					shownormals		= false;
	public bool					showextras		= false;
	public float				normallen		= 1.0f;
	public bool					showdataimport	= true;
	public bool					showanimation	= true;
	public bool					showdata		= false;
	public string				namesplit		= "";
	public string				runtimefolder	= "";
	bool						optimized		= false;
	int							lastreadframe	= -1;
	int							maxv			= 0;
	int							maxsm			= 0;
	int[]						maxsmfc;
	FileStream					fs;
	BinaryReader				br;
	long[]						meshoffs;
	public Mesh					imagemesh;
	static byte[]				buffer;
	public bool					meshchanged		= false;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=6226");
	}

	void Start()
	{
		mf = GetComponent<MeshFilter>();

		if ( !Application.isEditor && !Application.isWebPlayer )
		{
			if ( datasource == MegaCacheData.File )
			{
				if ( fs == null )
				{
					string file = Path.GetFileName(cachefile);
					string fullpath = Application.dataPath + "/";
					if ( runtimefolder.Length > 0 )
						fullpath += runtimefolder + "/";

					fullpath += file;
					OpenCache(fullpath);	//cachefile);
				}
			}
		}
	}

	public void ChangeSource(MegaCacheData src)
	{
		if ( src != datasource )
		{
			CloseCache();
			datasource = src;

			if ( Application.isEditor )
				DestroyImmediate(imagemesh);
			else
				Destroy(imagemesh);

			switch ( datasource )
			{
				case MegaCacheData.Mesh:	break;
				case MegaCacheData.File:	OpenCache(cachefile);	break;
				case MegaCacheData.Image:	MountImage(cacheimage);	break;
			}
			update = true;
		}
	}

	void Update()
	{
		int fc = 0;

		switch ( datasource )
		{
			case MegaCacheData.Mesh: fc = meshes.Count - 1; break;
			case MegaCacheData.File: fc = framecount - 1; break;
			case MegaCacheData.Image:
				if ( cacheimage && cacheimage.frames != null )
					fc = cacheimage.frames.Count - 1;
				break;
		}

		if ( fc > 0 )
		{
			if ( animate )
			{
				looptime = fc / fps;
				time += Time.deltaTime * speed;

				float at = time;

				switch ( loopmode )
				{
					case MegaCacheRepeatMode.Loop:
						at = Mathf.Repeat(time, Mathf.Abs(looptime));
						if ( looptime < 0.0f )
							at = looptime - at;
						break;
					case MegaCacheRepeatMode.PingPong: at = Mathf.PingPong(time, looptime); break;
					case MegaCacheRepeatMode.Clamp: at = Mathf.Clamp(time, 0.0f, looptime); break;
				}

				frame = (int)((at / looptime) * fc);
			}

			frame = Mathf.Clamp(frame, 0, fc);

			if ( frame != lastframe )
				meshchanged = true;

			if ( datasource == MegaCacheData.Image && cacheimage )
			{
				if ( imagemesh == null )
					imagemesh = new Mesh();

				if ( mf.sharedMesh != imagemesh )
				{
					ClearMesh();
					mf.sharedMesh = imagemesh;
				}

				cacheimage.GetMesh(imagemesh, frame, this);
			}

			if ( datasource == MegaCacheData.File )
					GetFrame(frame);

			if ( datasource == MegaCacheData.Mesh )
			{
				if ( mf && meshes.Count > 0 )
				{
					if ( mf.sharedMesh != meshes[frame] || update )
					{
						mf.sharedMesh = meshes[frame];
						framevertcount = meshes[frame].vertexCount;
					}
				}
			}

			if ( updatecollider && meshchanged )
			{
				if ( meshCol == null )
					meshCol = GetComponent<MeshCollider>();

				if ( meshCol != null )
				{
					meshCol.sharedMesh = null;
					meshCol.sharedMesh = mf.sharedMesh;
				}
			}
		}
		update = false;
		meshchanged = false;
	}

	MeshCollider meshCol;

	void Reset()
	{
	}

	public void AddMesh(Mesh ms)
	{
		if ( ms )
			meshes.Add(ms);
	}

	public void DestroyMeshes()
	{
		for ( int i = 0; i < meshes.Count; i++ )
		{
			if ( Application.isPlaying )
				Destroy(meshes[i]);
			else
				DestroyImmediate(meshes[i]);
		}

		meshes.Clear();
		meshes.TrimExcess();
		System.GC.Collect();

		ClearMesh();
		mf.sharedMesh = new Mesh();
	}

	public void DestroyImage()
	{
		if ( cacheimage )
		{
			if ( Application.isEditor )
				DestroyImmediate(cacheimage);
			else
				Destroy(cacheimage);

			cacheimage = null;
		}
	}

	public void ClearMesh()
	{
		if ( Application.isEditor )
			DestroyImmediate(mf.sharedMesh);
		else
			Destroy(mf.sharedMesh);
		mf.sharedMesh = null;
	}

	public void InitImport()
	{
		MegaCacheObjImporter.Init();
	}

	public Mesh LoadFrame(string filename, int frame)
	{
		Mesh ms = null;

		char[]	splits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

		string dir= Path.GetDirectoryName(filename);
		string file = Path.GetFileNameWithoutExtension(filename);

		string[] names;
			
		if ( namesplit.Length > 0 )
		{
			names = file.Split(namesplit[0]);
			names[0] += namesplit[0];
		}
		else
			names = file.Split(splits);

		if ( names.Length > 0 )
		{
			string newfname = dir + "/" + names[0] + frame.ToString("D" + decformat) + ".obj";
			ms = LoadFrame(newfname);
		}

		return ms;
	}

	public void LoadMtl(string filename, int frame)
	{
		char[]	splits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

		string dir= Path.GetDirectoryName(filename);
		string file = Path.GetFileNameWithoutExtension(filename);

		string[] names;
		
		if ( namesplit.Length > 0 )
		{
			names = file.Split(namesplit[0]);
			names[0] += namesplit[0];
		}
		else
			names = file.Split(splits);

		if ( names.Length > 0 )
		{
			string newfname = dir + "/" + names[0] + frame.ToString("D" + decformat) + ".mtl";
			LoadMtl(newfname);
		}
	}

	public void LoadMtl(string filename)
	{
		if ( File.Exists(filename) )
			MegaCacheObjImporter.ImportMtl(filename);
	}

	public Mesh LoadFrame(string filename)
	{
		Mesh ms = null;

		if ( File.Exists(filename) )
			ms = MegaCacheObjImporter.ImportFile(filename, scale, adjustcoord, buildtangents, loadmtls);

		return ms;
	}

	public void MountImage(MegaCacheImage image)
	{
		if ( image )
		{
			subs = new MegaCacheImageFace[image.maxsm];

			for ( int i = 0; i < image.maxsm; i++ )
			{
				MegaCacheImageFace cf = new MegaCacheImageFace();

				cf.max = image.smfc[i];
				cf.tris = new int[cf.max];
				subs[i] = cf;
			}

			vertcache = new Vector3[image.maxv];
			normcache = new Vector3[image.maxv];
			tangentcache = new Vector4[image.maxv];
			uvcache = new Vector2[image.maxv];
		}
	}

	public void OpenCache(string filename)
	{
		if ( filename.Length == 0 )
			return;

		fs = new FileStream(filename, FileMode.Open);
		if ( fs != null )
		{
			br = new BinaryReader(fs);

			if ( br != null )
			{
				int version = br.ReadInt32();

				if ( version == 0 )
				{
					framecount = br.ReadInt32();

					optimized = br.ReadBoolean();

					maxv = br.ReadInt32();
					br.ReadInt32();
					maxsm = br.ReadInt32();

					subs = new MegaCacheImageFace[maxsm];

					for ( int i = 0; i < maxsm; i++ )
					{
						MegaCacheImageFace cf = new MegaCacheImageFace();

						cf.max = br.ReadInt32();
						cf.tris = new int[cf.max];
						subs[i] = cf;
					}
				}

				vertcache = new Vector3[maxv];
				normcache = new Vector3[maxv];
				tangentcache = new Vector4[maxv];
				uvcache = new Vector2[maxv];

				if ( buffer == null || buffer.Length < maxv * 16 )
					buffer = new byte[maxv * 16];

				meshoffs = new long[framecount];

				for ( int i = 0; i < framecount; i++ )
					meshoffs[i] = br.ReadInt64();

				ClearMesh();
				Mesh mesh = new Mesh();
				mf.sharedMesh = mesh;
				update = true;
			}
		}
	}

	void OnDestroy()
	{
		CloseCache();
	}

	void OnDrawGizmosSelected()
	{
		if ( shownormals )
		{
			Vector3[] verts;
			Vector3[] norms;

			verts = mf.sharedMesh.vertices;
			norms = mf.sharedMesh.normals;

			Gizmos.color = Color.red;
			Gizmos.matrix = transform.localToWorldMatrix;

			float len = normallen * 0.01f;
			Color col = Color.black;
			for ( int i = 0; i < framevertcount; i++ )
			{
				col.r = norms[i].x;
				col.g = norms[i].y;
				col.b = norms[i].z;
				Gizmos.color = col;
				Gizmos.DrawRay(verts[i], norms[i] * len);
			}

			Gizmos.matrix = Matrix4x4.identity;
		}
	}

	void GetFrame(int fnum)
	{
		if ( br == null )
		{
			OpenCache(cachefile);
		}

		GetFrame(fnum, mf.sharedMesh);
	}

	public void GetFrameRef(int fnum, Mesh _mesh)
	{
		if ( br == null )
		{
			OpenCache(cachefile);
			update = true;
		}

		GetFrame(fnum, _mesh);
	}

	public void GetFrame(int fnum, Mesh mesh)
	{
		if ( fnum != lastreadframe || update )
		{
			MakeMeshFromFrame(fnum, mesh);
			lastreadframe = fnum;
		}
	}

	public void MakeMeshFromFrame(int fnum, Mesh mesh)
	{
		if ( br != null )
		{
			fs.Position = meshoffs[fnum];

			int vc = br.ReadInt32();
			int nc = br.ReadInt32();
			int uvc = br.ReadInt32();
			int tc = br.ReadInt32();

			Vector3 bmin;
			Vector3 bmax;
			bmin.x = br.ReadSingle();
			bmin.y = br.ReadSingle();
			bmin.z = br.ReadSingle();
			bmax.x = br.ReadSingle();
			bmax.y = br.ReadSingle();
			bmax.z = br.ReadSingle();

			Vector3 bsize = (bmax - bmin) * (1.0f / 65535.0f);
			mesh.bounds.SetMinMax(bmin, bmax);

			float oo127 = 1.0f / 127.0f;

			if ( !optimized )
			{
				br.Read(buffer, 0, vc * 12);

				for ( int i = 0; i < vc; i++ )
				{
					int ix = i * 12;
					vertcache[i].x = System.BitConverter.ToSingle(buffer, ix);
					vertcache[i].y = System.BitConverter.ToSingle(buffer, ix + 4);
					vertcache[i].z = System.BitConverter.ToSingle(buffer, ix + 8);
				}
			}
			else
			{
				br.Read(buffer, 0, vc * 6);

				for ( int i = 0; i < vc; i++ )
				{
					int ix = i * 6;
					vertcache[i].x = bmin.x + ((float)System.BitConverter.ToUInt16(buffer, ix) * bsize.x);
					vertcache[i].y = bmin.y + ((float)System.BitConverter.ToUInt16(buffer, ix + 2) * bsize.y);
					vertcache[i].z = bmin.z + ((float)System.BitConverter.ToUInt16(buffer, ix + 4) * bsize.z);
				}
			}

			if ( !optimized )
			{
				br.Read(buffer, 0, nc * 12);

				for ( int i = 0; i < nc; i++ )
				{
					int ix = i * 12;
					normcache[i].x = System.BitConverter.ToSingle(buffer, ix);
					normcache[i].y = System.BitConverter.ToSingle(buffer, ix + 4);
					normcache[i].z = System.BitConverter.ToSingle(buffer, ix + 8);
				}
			}
			else
			{
				br.Read(buffer, 0, nc * 3);

				for ( int i = 0; i < nc; i++ )
				{
					int ix = i * 3;
					normcache[i].x = (float)((sbyte)buffer[ix]) * oo127;
					normcache[i].y = (float)((sbyte)buffer[ix + 1]) * oo127;
					normcache[i].z = (float)((sbyte)buffer[ix + 2]) * oo127;
				}
			}

			if ( !optimized )
			{
				br.Read(buffer, 0, tc * 16);

				for ( int i = 0; i < tc; i++ )
				{
					int ix = i * 16;
					tangentcache[i].x = System.BitConverter.ToSingle(buffer, ix);
					tangentcache[i].y = System.BitConverter.ToSingle(buffer, ix + 4);
					tangentcache[i].z = System.BitConverter.ToSingle(buffer, ix + 8);
					tangentcache[i].w = System.BitConverter.ToSingle(buffer, ix + 12);
				}
			}
			else
			{
				br.Read(buffer, 0, tc * 4);

				for ( int i = 0; i < tc; i++ )
				{
					tangentcache[i].x = (float)((sbyte)buffer[i * 4]) * oo127;
					tangentcache[i].y = (float)((sbyte)buffer[i * 4 + 1]) * oo127;
					tangentcache[i].z = (float)((sbyte)buffer[i * 4 + 2]) * oo127;
					tangentcache[i].w = (float)((sbyte)buffer[i * 4 + 3]) * oo127;
				}
			}

			if ( !optimized )
			{
				br.Read(buffer, 0, uvc * 8);

				for ( int i = 0; i < uvc; i++ )
				{
					int ix = i * 8;
					uvcache[i].x = System.BitConverter.ToSingle(buffer, ix);
					uvcache[i].y = System.BitConverter.ToSingle(buffer, ix + 4);
				}
			}
			else
			{
				Vector2 uvmin;
				Vector2 uvmax;

				uvmin.x = br.ReadSingle();
				uvmin.y = br.ReadSingle();
				uvmax.x = br.ReadSingle();
				uvmax.y = br.ReadSingle();

				Vector2 uvsize = (uvmax - uvmin) * (1.0f / 255.0f);
				br.Read(buffer, 0, uvc * 2);

				for ( int i = 0; i < uvc; i++ )
				{
					int ix = i * 2;
					uvcache[i].x = uvmin.x + ((float)((byte)buffer[ix]) * uvsize.x);
					uvcache[i].y = uvmin.y + ((float)((byte)buffer[ix + 1]) * uvsize.y);
				}
			}

			byte smcount = br.ReadByte();
			mesh.subMeshCount = smcount;

			mesh.vertices = vertcache;
			if ( nc > 0 )
				mesh.normals = normcache;

			if ( uvc > 0 )
				mesh.uv = uvcache;

			if ( tc > 0 )
				mesh.tangents = tangentcache;

			for ( int s = 0; s < smcount; s++ )
			{
				int trc = br.ReadInt32();

				br.Read(buffer, 0, trc * 2);

				for ( int f = 0; f < trc; f++ )
					subs[s].tris[f] = (int)System.BitConverter.ToUInt16(buffer, f * 2);

				for ( int ii = trc; ii < subs[s].max; ii++ )
					subs[s].tris[ii] = subs[s].tris[trc];
			}

			for ( int s = 0; s < smcount; s++ )
				mesh.SetTriangles(subs[s].tris, s);

			mesh.RecalculateBounds();
		}
	}

	public void CloseCache()
	{
		if ( br != null )
		{
			br.Close();
			br = null;
		}

		if ( fs != null )
		{
			fs.Close();
			fs = null;
		}

		buffer = null;
		GC.Collect();
	}

	public void CreateImageFromCacheFile()
	{
		if ( br == null )
			OpenCache(cachefile);

		if ( br != null )
		{
			if ( cacheimage )
				DestroyImage();

			MegaCacheImage img = (MegaCacheImage)ScriptableObject.CreateInstance<MegaCacheImage>();

			img.maxv = maxv;
			img.maxsm = maxsm;

			img.smfc = new int[maxsm];

			for ( int i = 0; i < maxsm; i++ )
				img.smfc[i] = subs[i].max;

			Mesh mesh = new Mesh();
			for ( int i = 0; i < framecount; i++ )
			{
				MakeMeshFromFrame(i, mesh);
				MegaCacheImageFrame frame = MegaCacheImage.CreateImageFrame(mesh);

				img.frames.Add(frame);
			}

			cacheimage = img;

			ChangeSource(MegaCacheData.Image);

			if ( Application.isEditor )
				DestroyImmediate(mesh);
			else
				Destroy(mesh);
			mesh = null;

			GC.Collect();
		}
	}
}
