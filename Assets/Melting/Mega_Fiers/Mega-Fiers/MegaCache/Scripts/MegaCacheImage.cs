
using UnityEngine;
using System.Collections.Generic;
using System;

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
using System.Threading;
#endif

[System.Serializable]
public class MegaCacheImageFrame
{
	public int		vc;
	public int		nc;
	public int		tc;
	public int		uvc;
	public Vector3	bmin;
	public Vector3	uvmin;
	public Vector3	bsize;
	public Vector3	uvsize;
	public byte[]	verts;
	public byte[]	norms;
	public byte[]	tangents;
	public byte[]	uvs;
	public byte[]	tris;
	public int		subcount;
	public int[]	suboffs;
	public int[]	sublen;
	public MegaCacheImageFace[]	subs;

	public void LoadSection(MegaCacheOBJ cache)
	{
		float oo127 = 1.0f / 127.0f;

		for ( int i = 0; i < vc; i++ )
		{
			int ix = i * 6;

			cache.vertcache[i].x = bmin.x + ((float)System.BitConverter.ToUInt16(verts, ix) * bsize.x);
			cache.vertcache[i].y = bmin.y + ((float)System.BitConverter.ToUInt16(verts, ix + 2) * bsize.y);
			cache.vertcache[i].z = bmin.z + ((float)System.BitConverter.ToUInt16(verts, ix + 4) * bsize.z);
		}

		for ( int i = 0; i < nc; i++ )
		{
			int ix = i * 3;
			cache.normcache[i].x = ((float)norms[ix] - 127.0f) * oo127;
			cache.normcache[i].y = ((float)norms[ix + 1] - 127.0f) * oo127;
			cache.normcache[i].z = ((float)norms[ix + 2] - 127.0f) * oo127;
		}

		for ( int i = 0; i < tc; i++ )
		{
			int ix = i * 4;
			cache.tangentcache[i].x = ((float)tangents[ix] - 127.0f) * oo127;
			cache.tangentcache[i].y = ((float)tangents[ix + 1] - 127.0f) * oo127;
			cache.tangentcache[i].z = ((float)tangents[ix + 2] - 127.0f) * oo127;
			cache.tangentcache[i].w = ((float)tangents[ix + 3] - 127.0f) * oo127;
		}

		for ( int i = 0; i < uvc; i++ )
		{
			int ix = i * 2;
			cache.uvcache[i].x = uvmin.x + ((float)uvs[ix] * uvsize.x);
			cache.uvcache[i].y = uvmin.y + ((float)uvs[ix + 1] * uvsize.y);
		}

		for ( int s = 0; s < subcount; s++ )
		{
			int soff = suboffs[s];
			for ( int f = 0; f < sublen[s]; f++ )
				cache.subs[s].tris[f] = (int)System.BitConverter.ToUInt16(tris, soff + (f * 2));

			for ( int ii = sublen[s]; ii < cache.subs[s].max; ii++ )
				cache.subs[s].tris[ii] = cache.subs[s].tris[sublen[s]];
		}
	}

	public void SetMesh(Mesh mesh, MegaCacheOBJ cache)
	{
		mesh.subMeshCount = subcount;

		mesh.vertices = cache.vertcache;
		if ( nc > 0 )
			mesh.normals = cache.normcache;

		if ( uvc > 0 )
			mesh.uv = cache.uvcache;

		if ( tc > 0 )
			mesh.tangents = cache.tangentcache;

		for ( int s = 0; s < subcount; s++ )
			mesh.SetTriangles(cache.subs[s].tris, s);

		mesh.RecalculateBounds();
	}

	public void GetMesh(Mesh mesh, MegaCacheOBJ cache)
	{
		float oo127 = 1.0f / 127.0f;

		cache.framevertcount = vc;

		for ( int i = 0; i < vc; i++ )
		{
			int ix = i * 6;

			cache.vertcache[i].x = bmin.x + ((float)System.BitConverter.ToUInt16(verts, ix) * bsize.x);
			cache.vertcache[i].y = bmin.y + ((float)System.BitConverter.ToUInt16(verts, ix + 2) * bsize.y);
			cache.vertcache[i].z = bmin.z + ((float)System.BitConverter.ToUInt16(verts, ix + 4) * bsize.z);
		}

		for ( int i = 0; i < nc; i++ )
		{
			int ix = i * 3;
			cache.normcache[i].x = ((float)norms[ix] - 127.0f) * oo127;
			cache.normcache[i].y = ((float)norms[ix + 1] - 127.0f) * oo127;
			cache.normcache[i].z = ((float)norms[ix + 2] - 127.0f) * oo127;
		}

		for ( int i = 0; i < tc; i++ )
		{
			int ix = i * 4;
			cache.tangentcache[i].x = ((float)tangents[ix] - 127.0f) * oo127;
			cache.tangentcache[i].y = ((float)tangents[ix + 1] - 127.0f) * oo127;
			cache.tangentcache[i].z = ((float)tangents[ix + 2] - 127.0f) * oo127;
			cache.tangentcache[i].w = ((float)tangents[ix + 3] - 127.0f) * oo127;
		}

		for ( int i = 0; i < uvc; i++ )
		{
			int ix = i * 2;
			cache.uvcache[i].x = uvmin.x + ((float)uvs[ix] * uvsize.x);
			cache.uvcache[i].y = uvmin.y + ((float)uvs[ix + 1] * uvsize.y);
		}

		mesh.subMeshCount = subcount;

		mesh.vertices = cache.vertcache;
		if ( nc > 0 )
			mesh.normals = cache.normcache;

		if ( uvc > 0 )
			mesh.uv = cache.uvcache;

		if ( tc > 0 )
			mesh.tangents = cache.tangentcache;

		for ( int s = 0; s < subcount; s++ )
		{
			int soff = suboffs[s];
			for ( int f = 0; f < sublen[s]; f++ )
				cache.subs[s].tris[f] = (int)System.BitConverter.ToUInt16(tris, soff + (f * 2));

			for ( int ii = sublen[s]; ii < cache.subs[s].max; ii++ )
				cache.subs[s].tris[ii] = cache.subs[s].tris[sublen[s]];
		}

		for ( int s = 0; s < subcount; s++ )
			mesh.SetTriangles(cache.subs[s].tris, s);

		mesh.RecalculateBounds();
	}
}

[System.Serializable]
public class MegaCacheImage : ScriptableObject
{
	public List<MegaCacheImageFrame>	frames = new List<MegaCacheImageFrame>();

	public int		maxsm;
	public int		maxv;
	public int		maxtris;
	public int[]	smfc;
	public int		lastframe = -1;
	public int		preloaded = -1;
	public int		memoryuse = 0;

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
	public bool		threadupdate = false;

	public class MegaCacheOBJTaskInfo
	{
		public string			name;
		public AutoResetEvent	pauseevent;
		public Thread			_thread;
		public MegaCacheOBJ		objcache;
		public int				end;
		public int				frame;
	}

	public int				Cores = 1;
	static bool				isRunning = false;
	MegaCacheOBJTaskInfo[]	tasks;

	void MakeThreads(MegaCacheOBJ cache)
	{
		if ( Cores > 0 )
		{
			isRunning = true;
			tasks = new MegaCacheOBJTaskInfo[Cores];

			for ( int i = 0; i < Cores; i++ )
			{
				tasks[i] = new MegaCacheOBJTaskInfo();

				tasks[i].objcache = cache;
				tasks[i].name = "ThreadID " + i;
				tasks[i].pauseevent = new AutoResetEvent(false);
				tasks[i]._thread = new Thread(DoWork);
				tasks[i]._thread.Start(tasks[i]);
			}
		}
	}

	void DoWork(object info)
	{
		MegaCacheOBJTaskInfo inf = (MegaCacheOBJTaskInfo)info;

		while ( isRunning )
		{
			inf.pauseevent.WaitOne(Timeout.Infinite, false);

			if ( inf.end > 0 )
				PreLoad(inf.frame, inf.objcache);

			inf.end = 0;	// Done the job
		}
	}

	public void GetNextFrame(MegaCacheOBJ cache, int frame)
	{
		if ( Cores == 0 )
			Cores = SystemInfo.processorCount - 1;

		if ( Cores < 1 || !Application.isPlaying )
			return;

		if ( tasks == null )
			MakeThreads(cache);

		if ( Cores > 0 )
		{
			for ( int i = 0; i < tasks.Length; i++ )
			{
				tasks[i].objcache = cache;
				tasks[i].end = 1;
				tasks[i].frame = frame;
			}

			for ( int i = 0; i < tasks.Length; i++ )
				tasks[i].pauseevent.Set();
		}
	}

	void OnDestroy()
	{
		if ( Application.isPlaying )
		{
			isRunning = false;

			if ( tasks != null )
			{
				for ( int i = 0; i < tasks.Length; i++ )
				{
					tasks[i].pauseevent.Set();

					while ( tasks[i]._thread.IsAlive )
					{
					}
				}
			}
			tasks = null;
		}
	}
#else
	public void GetNextFrame(MegaCacheOBJ cache, int frame)
	{
		PreLoad(frame, cache);
	}
#endif

	public void PreLoad(int frame, MegaCacheOBJ cache)
	{
		if ( frame != preloaded )
		{
			preloaded = frame;
			frames[frame].LoadSection(cache);
		}
	}

	public void GetMesh(Mesh mesh, int frame, MegaCacheOBJ cache)
	{
#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
		if ( threadupdate && Application.isPlaying )
			GetMeshPreLoaded(mesh, frame, cache);
		else
		{
			if ( frame != lastframe )
			{
				lastframe = frame;
				frames[frame].GetMesh(mesh, cache);
			}
		}
#else
		if ( frame != lastframe )
		{
			lastframe = frame;
			frames[frame].GetMesh(mesh, cache);
		}
#endif
	}

	public void GetMeshPreLoaded(Mesh mesh, int frame, MegaCacheOBJ cache)
	{
		if ( frame != lastframe )
		{
			if ( frame == preloaded )
				frames[frame].SetMesh(mesh, cache);
			else
				frames[frame].GetMesh(mesh, cache);

			int next = frame + 1;
			if ( next >= frames.Count )
				next = 0;

			GetNextFrame(cache, next);

			lastframe = frame;
		}
	}

	public int CalcMemory()
	{
		int mem = 0;

		for ( int i = 0; i < frames.Count; i++ )
		{
			MegaCacheImageFrame fr = frames[i];

			mem += fr.verts.Length;
			mem += fr.norms.Length;
			mem += fr.tangents.Length;
			mem += fr.uvs.Length;
			mem += fr.tris.Length;
			mem += fr.suboffs.Length * 2;
			mem += fr.sublen.Length * 2;
		}

		memoryuse = mem;
		return mem;
	}

	static public MegaCacheImageFrame CreateImageFrame(Mesh ms)
	{
		MegaCacheImageFrame frame = new MegaCacheImageFrame();

		Vector3[] verts = ms.vertices;
		Vector3[] norms = ms.normals;
		Vector2[] uvs = ms.uv;
		Vector4[] tangents = ms.tangents;

		frame.vc = verts.Length;
		frame.nc = norms.Length;
		frame.tc = tangents.Length;
		frame.uvc = uvs.Length;

		frame.bmin = ms.bounds.min;
		//Vector3 bmax = ms.bounds.max;

		Vector3 msize = ms.bounds.size;

		frame.bsize = ms.bounds.size * (1.0f / 65535.0f);

		Bounds uvb = MegaCacheUtils.GetBounds(uvs);

		frame.uvmin = uvb.min;
		frame.uvsize = uvb.size * (1.0f / 255.0f);

		frame.verts = new byte[frame.vc * 6];
		frame.norms = new byte[frame.nc * 3];
		frame.tangents = new byte[frame.tc * 4];
		frame.uvs = new byte[frame.vc * 2];
		frame.tris = new byte[ms.triangles.Length * 2];

		int ix = 0;
		byte[] by;

		for ( int v = 0; v < verts.Length; v++ )
		{
			Vector3 pos = verts[v];

			short val = (short)(((pos.x - frame.bmin.x) / msize.x) * 65535.0f);

			by = System.BitConverter.GetBytes(val);
			frame.verts[ix++] = by[0];
			frame.verts[ix++] = by[1];

			val = (short)(((pos.y - frame.bmin.y) / msize.y) * 65535.0f);

			by = System.BitConverter.GetBytes(val);
			frame.verts[ix++] = by[0];
			frame.verts[ix++] = by[1];

			val = (short)(((pos.z - frame.bmin.z) / msize.z) * 65535.0f);

			by = System.BitConverter.GetBytes(val);
			frame.verts[ix++] = by[0];
			frame.verts[ix++] = by[1];
		}

		ix = 0;
		for ( int v = 0; v < norms.Length; v++ )
		{
			Vector3 pos = norms[v];

			frame.norms[ix++] = (byte)((pos.x + 1.0f) * 127.0f);
			frame.norms[ix++] = (byte)((pos.y + 1.0f) * 127.0f);
			frame.norms[ix++] = (byte)((pos.z + 1.0f) * 127.0f);
		}

		ix = 0;
		for ( int v = 0; v < tangents.Length; v++ )
		{
			Vector4 pos = tangents[v];

			frame.tangents[ix++] = (byte)((pos.x + 1.0f) * 127.0f);
			frame.tangents[ix++] = (byte)((pos.y + 1.0f) * 127.0f);
			frame.tangents[ix++] = (byte)((pos.z + 1.0f) * 127.0f);
			frame.tangents[ix++] = (byte)((pos.w + 1.0f) * 127.0f);
		}

		ix = 0;
		for ( int v = 0; v < uvs.Length; v++ )
		{
			Vector2 pos = uvs[v];

			frame.uvs[ix++] = (byte)(((pos.x - uvb.min.x) / uvb.size.x) * 255.0f);
			frame.uvs[ix++] = (byte)(((pos.y - uvb.min.y) / uvb.size.y) * 255.0f);
		}

		frame.subcount = ms.subMeshCount;

		frame.suboffs = new int[frame.subcount];
		frame.sublen = new int[frame.subcount];

		ix = 0;
		for ( int s = 0; s < frame.subcount; s++ )
		{
			int[] tris = ms.GetTriangles(s);

			frame.suboffs[s] = ix;
			frame.sublen[s] = tris.Length;

			for ( int t = 0; t < tris.Length; t++ )
			{
				short val = (short)tris[t];

				by = System.BitConverter.GetBytes(val);

				frame.tris[ix++] = by[0];
				frame.tris[ix++] = by[1];
			}
		}

		return frame;
	}
}
