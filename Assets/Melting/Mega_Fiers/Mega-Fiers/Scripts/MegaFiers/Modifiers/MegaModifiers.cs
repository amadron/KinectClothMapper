
using UnityEngine;
using System;
using System.Collections.Generic;

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
using System.Threading;
#endif

// Do we put verts in here
public struct MegaModContext
{
	public MegaBox3			bbox;
	public Vector3			Offset;
	public MegaModifiers	mod;
	public GameObject		go;
}

public enum MegaJobType
{
	Modifier,
	FaceNormalCalc,
	VertexNormalCalc,
	FaceTangentCalc,
	VertexTangentCalc,
}

public enum MegaNormalMethod
{
	Unity,
	Mega,
}

[System.Serializable]
public class MegaNormMap
{
	public int[]	faces;
}

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
public class MegaTaskInfo
{
	public string			name;
	public volatile int		start;
	public volatile int		end;
	public AutoResetEvent	pauseevent;
	public Thread			_thread;
	public MegaModContext	modcontext;
	public int				index;
	public int				cores;
	public MegaJobType		jobtype;
}
#endif

// Collision mesh if a proxy added as a target
// Selection sets might be handy
public class MegaModifiers : MonoBehaviour
{
	[HideInInspector]
	public Bounds			bbox			= new Bounds();
	public bool				recalcnorms		= true;
	public MegaNormalMethod	NormalMethod	= MegaNormalMethod.Mega;
	public bool				recalcbounds	= false;
	public bool				recalcCollider	= false;
	public bool				recalcTangents	= false;
	public bool				dynamicMesh		= false;
	public bool				Enabled			= true;
	public bool				DoLateUpdate	= false;
	//public bool				GrabVerts		= false;
	public bool				DrawGizmos		= true;
	[HideInInspector]
	public Vector3[]		verts;
	[HideInInspector]
	public Vector3[]		sverts;
	[HideInInspector]
	public Vector2[]		uvs;
	[HideInInspector]
	public Vector2[]		suvs;
	[HideInInspector]
	public Mesh				mesh;
	[HideInInspector]
	public MegaModifier[]	mods			= null;
	[HideInInspector]
	public int				UpdateMesh		= 0;
	[HideInInspector]
	public MegaModChannel	dirtyChannels;
	[HideInInspector]
	public GameObject		sourceObj;
	[HideInInspector]
	public Color[]			cols;
	[HideInInspector]
	public float[]			selection;
	[HideInInspector]
	public List<GameObject>	group = new List<GameObject>();
	public static bool		GlobalDisplay = true;
	Vector4[]				tangents;
	Vector3[]				tan1;
	Vector3[]				tan2;
	public MegaModContext	modContext = new MegaModContext();

	public void InitVertSource()
	{
		VertsSource = true;
		UVsSource = true;
	}

	public Vector3[] GetSourceVerts(MegaModifierTarget target)
	{
		if ( VertsSource )
		{
			VertsSource = false;
			return target.verts;
		}

		return target.sverts;
	}

	public Vector3[] GetDestVerts(MegaModifierTarget target)
	{
		return target.sverts;
	}

	public Vector3[] GetSourceVerts()
	{
		if ( VertsSource )
		{
			VertsSource = false;
			return verts;
		}

		return sverts;
	}

	public void ChangeSourceVerts()
	{
		if ( VertsSource )
			VertsSource = false;
	}

	public Vector3[] GetDestVerts()
	{
		return sverts;
	}

	public Vector2[] GetSourceUvs()
	{
		if ( UVsSource )
		{
			UVsSource = false;
			return uvs;
		}
		return suvs;
	}

	public Vector2[] GetDestUvs()
	{
		return suvs;
	}

	private static int CompareOrder(MegaModifier m1, MegaModifier m2)
	{
		return m1.Order - m2.Order;
	}

	//void Reset()
	//{
		//ReStart();
	//}

#if false
	void Start()
	{
		Debug.Log("Mod STart");
		ReStart();
	}

	// Call if base mesh changes
	public void ReStart()
	{
		mesh = MegaUtils.GetMesh(gameObject);

		if ( mesh != null )
		{
			bbox = mesh.bounds;
			sverts = new Vector3[mesh.vertexCount];
			verts = mesh.vertices;

			uvs = mesh.uv;
			suvs = new Vector2[mesh.uv.Length];
			cols = mesh.colors;

			mods = GetComponents<MegaModifier>();

			Array.Sort(mods, CompareOrder);

			for ( int i = 0; i < mods.Length; i++ )
			{
				if ( mods[i] != null )
					mods[i].ModStart(this);	// Some mods like push error if we dont do this, put in error check and disable 
			}
		}

		UpdateMesh = -1;
	}
#endif

	bool VertsSource = false;
	bool UVsSource = false;

	public void UpdateCols(int first, Color[] newcols)
	{
		if ( cols == null || cols.Length == 0 )
			cols = new Color[verts.Length];

		int end = first + newcols.Length;
		if ( end > cols.Length )
			end = cols.Length;

		int ix = 0;

		for ( int i = first; i < end; i++ )
		{
			cols[i] = newcols[ix++];
		}
	}

	public void UpdateCol(int i, Color col)
	{
		if ( cols == null || cols.Length == 0 )
			cols = new Color[verts.Length];

		if ( i < cols.Length )
			cols[i] = col;
	}

	public void UpdateCols(Color[] newcols)
	{
		if ( cols == null || cols.Length == 0 )
			cols = new Color[verts.Length];

		if ( newcols.Length != verts.Length )
		{
			Debug.Log("Number of new Colors does not match vertex count for mesh!");
			return;
		}
		cols = newcols;
	}

	public void ModifyObject()
	{
		if ( Enabled && mods != null )
		{
			dirtyChannels = MegaModChannel.None;

			//if ( GrabVerts )
			//{
				//if ( sverts.Length < mesh.vertexCount )
					//sverts = new Vector3[mesh.vertexCount];

				//verts = mesh.vertices;
			//}

			VertsSource = true;
			UVsSource = true;

			modContext.mod = this;
			selection = null;

			for ( int i = 0; i < mods.Length; i++ )
			{
				MegaModifier mod = mods[i];

				if ( mod != null && mod.ModEnabled )
				{
					// **group**
					if ( mod.instance )
					{
						// Actually dont even do this have a GetValues method
						mod.SetValues(mod.instance);
					}

					if ( (mod.ChannelsReq() & MegaModChannel.Verts) != 0 )	// should be changed
					{
						mod.verts = GetSourceVerts();
						mod.sverts = GetDestVerts();
					}

					modContext.Offset = mod.Offset;
					modContext.bbox = mod.bbox;

					mod.SetTM();
					if ( mod.ModLateUpdate(modContext) )
					{
						if ( selection != null )
						{
							mod.ModifyWeighted(this);
							if ( UpdateMesh < 1 )
								UpdateMesh = 1;
						}
						else
						{
							if ( UpdateMesh < 1 )
							{
								mod.Modify(this);
								UpdateMesh = 1;
							}
							else
							{
								mod.Modify(this);
							}
						}

						dirtyChannels |= mod.ChannelsChanged();
						mod.ModEnd(this);
					}
				}
			}

			if ( UpdateMesh == 1 )
			{
				SetMesh(ref sverts);
				UpdateMesh = 0;
			}
			else
			{
				if ( UpdateMesh == 0 )
				{
					SetMesh(ref verts);
					UpdateMesh = -1;	// Dont need to set verts again until a mod is enabled
				}
			}
		}
	}

	MeshCollider meshCol = null;

	public void SetMesh(ref Vector3[] _verts)
	{
		if ( mesh == null )
			return;

		// Force system to use the PS3 remapping method
		//if ( true )	//Application.platform == RuntimePlatform.PS3 && !Application.isEditor )
		//{
		//	SetPS3Mesh();
		//	return;
		//}

		if ( (dirtyChannels & MegaModChannel.Verts) != 0 )
			mesh.vertices = sverts;	//_verts;	// mesh.vertices = GetVerts(true);

		if ( (dirtyChannels & MegaModChannel.UV) != 0 )
			mesh.uv = suvs;	//GetUVs(true);

		if ( recalcnorms )
			RecalcNormals();

		if ( recalcTangents )
			BuildTangents();

		if ( recalcbounds )
			mesh.RecalculateBounds();

		if ( recalcCollider )
		{
			if ( meshCol == null )
				meshCol = GetComponent<MeshCollider>();

			if ( meshCol != null )
			{
				meshCol.sharedMesh = null;
				meshCol.sharedMesh = mesh;
			}
		}
	}

	public void RecalcNormals()
	{
		if ( NormalMethod == MegaNormalMethod.Unity || dynamicMesh )	//|| mapping == null )
			mesh.RecalculateNormals();
		else
		{
			if ( mapping == null )
				BuildNormalMapping(mesh, false);

#if UNITY_FLASH || UNITY_PS3 || UNITY_METRO || UNITY_WP8
			RecalcNormals(mesh, sverts);
#else
			if ( !UseThreading || !ThreadingOn || Cores < 1 || !Application.isPlaying )
				RecalcNormals(mesh, sverts);
			else
				RecalcNormalsMT(mesh, sverts);
#endif
		}
	}

	// Can thread this
	void BuildTangents()
	{
		if ( uvs == null )
			return;

		BuildTangents(mesh, sverts);
	}

	void BuildTangents(Mesh ms, Vector3[] _verts)
	{
		int triangleCount = ms.triangles.Length;
		int vertexCount = _verts.Length;

		if ( tan1 == null || tan1.Length != vertexCount )
			tan1 = new Vector3[vertexCount];

		if ( tan2 == null || tan2.Length != vertexCount )
			tan2 = new Vector3[vertexCount];

		if ( tangents == null || tangents.Length != vertexCount )
			tangents = new Vector4[vertexCount];

		Vector3[]	norms	= ms.normals;
		int[]		tris	= ms.triangles;

		for ( int a = 0; a < triangleCount; a += 3 )
		{
			long i1 = tris[a];
			long i2 = tris[a + 1];
			long i3 = tris[a + 2];

			Vector3 v1 = _verts[i1];
			Vector3 v2 = _verts[i2];
			Vector3 v3 = _verts[i3];

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

			tan1[i1].x += sdir.x;
			tan1[i1].y += sdir.y;
			tan1[i1].z += sdir.z;
			tan1[i2].x += sdir.x;
			tan1[i2].y += sdir.y;
			tan1[i2].z += sdir.z;
			tan1[i3].x += sdir.x;
			tan1[i3].y += sdir.y;
			tan1[i3].z += sdir.z;

			tan2[i1].x += tdir.x;
			tan2[i1].y += tdir.y;
			tan2[i1].z += tdir.z;
			tan2[i2].x += tdir.x;
			tan2[i2].y += tdir.y;
			tan2[i2].z += tdir.z;
			tan2[i3].x += tdir.x;
			tan2[i3].y += tdir.y;
			tan2[i3].z += tdir.z;
		}

		for ( int a = 0; a < _verts.Length; a++ )
		{
			Vector3 n = norms[a];
			Vector3 t = tan1[a];

			Vector3.OrthoNormalize(ref n, ref t);
			tangents[a].x = t.x;
			tangents[a].y = t.y;
			tangents[a].z = t.z;
			tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
		}

		ms.tangents = tangents;
	}

	public void Sort()
	{
		Array.Sort(mods, CompareOrder);
	}

	// Call if you change a priority or externally add a mod
	public void BuildList()
	{
		mods = GetComponents<MegaModifier>();

		for ( int i = 0; i < mods.Length; i++ )
		{
			if ( mods[i].Order == -1 )
				mods[i].Order = i;
		}
		Array.Sort(mods, CompareOrder);
	}

#if false
	public MegaModifier Add(Type type)
	{
		MegaModifier mod = (MegaModifier)gameObject.AddComponent(type);

		if ( mod != null )
			BuildList();

		return mod;
	}

	public static MegaModifiers Get(GameObject go)
	{
		MegaModifiers mod = (MegaModifiers)go.GetComponent<MegaModifiers>();

		if ( mod == null )
			mod = go.AddComponent<MegaModifiers>();

		return mod;
	}
#endif

	void OnDrawGizmosSelected()
	{
		modContext.mod = this;
		modContext.go = gameObject;

		if ( GlobalDisplay && DrawGizmos && Enabled )
		{
			for ( int i = 0; i < mods.Length; i++ )
			{
				MegaModifier mod = mods[i];

				if ( mod != null )
				{
					if ( mod.ModEnabled && mod.DisplayGizmo )
					{
						modContext.Offset = mod.Offset;
						modContext.bbox = mod.bbox;
						mod.DrawGizmo(modContext);
					}
				}
			}
		}
	}

	// Multithreaded test
	public static bool	ThreadingOn;
	public bool			UseThreading = true;
	public static int	Cores = 0;	//SystemInfo.processorCount;	// 0 is not mt

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
	static MegaTaskInfo[]	tasks;

	void MakeThreads()
	{
		if ( Cores > 0 )
		{
			isRunning = true;
			tasks = new MegaTaskInfo[Cores];

			for ( int i = 0; i < Cores; i++ )
			{
				tasks[i] = new MegaTaskInfo();

				tasks[i].name = "ThreadID " + i;
				tasks[i].pauseevent = new AutoResetEvent(false);
				tasks[i]._thread = new Thread(DoWork);
				tasks[i]._thread.Start(tasks[i]);
			}
		}
	}
#endif

	// Per modifyobject to use mt or not
	// auto bias of first task on num of waits
	public void ModifyObjectMT()
	{
#if UNITY_FLASH || UNITY_PS3 || UNITY_METRO || UNITY_WP8
		ModifyObject();
#else
		if ( Cores == 0 )
			Cores = SystemInfo.processorCount - 1;

		// Dont use mt in editor mode
		if ( !UseThreading || !ThreadingOn || Cores < 1 || !Application.isPlaying )
		{
			ModifyObject();
			return;
		}

		if ( tasks == null )
			MakeThreads();

		if ( Enabled && mods != null )
		{
			dirtyChannels = MegaModChannel.None;

			//if ( GrabVerts )
			//{
				//if ( sverts.Length < mesh.vertexCount )
					//sverts = new Vector3[mesh.vertexCount];

				//verts = mesh.vertices;
			//}

			VertsSource = true;
			UVsSource = true;

			modContext.mod = this;

			PrepareForMT();

			// Set up tasks
			int step = verts.Length / (Cores + 1);

			if ( Cores > 0 )
			{
				int index = step;
				for ( int i = 0; i < tasks.Length; i++ )
				{
					tasks[i].jobtype	= MegaJobType.Modifier;
					tasks[i].index		= i + 1;
					tasks[i].cores		= tasks.Length;
					tasks[i].start		= index;
					tasks[i].end		= index + step;
					tasks[i].modcontext = modContext;
					index += step;
				}

				tasks[Cores - 1].end = verts.Length;

				for ( int i = 0; i < tasks.Length; i++ )
					tasks[i].pauseevent.Set();
			}

			// Do this thread work
			DoWork1(0, step);	// Bias the first job to reduce wait

			// Now need to sit and wait for jobs done, we should be doing work here
			WaitJobs();
			Done();

			if ( UpdateMesh == 1 )
			{
				SetMesh(ref sverts);
				UpdateMesh = 0;
			}
			else
			{
				if ( UpdateMesh == 0 )
				{
					SetMesh(ref verts);
					UpdateMesh = -1;	// Dont need to set verts again until a mod is enabled
				}
			}
		}
#endif
	}

	void PrepareForMT()
	{
		selection = null;

		for ( int m = 0; m < mods.Length; m++ )
		{
			MegaModifier mod = mods[m];
			mod.valid = false;

			if ( mod != null && mod.ModEnabled )
			{
				modContext.Offset = mod.Offset;
				modContext.bbox = mod.bbox;

				mod.SetTM();
				if ( mod.ModLateUpdate(modContext) )
				{
					mod.valid = true;
					if ( (mod.ChannelsReq() & MegaModChannel.Verts) != 0 )
					{
						mod.verts = GetSourceVerts();	// is this a ref or does it do a copy
						mod.sverts = GetDestVerts();
					}

					if ( UpdateMesh < 1 )
						UpdateMesh = 1;

					dirtyChannels |= mod.ChannelsChanged();
				}

				mod.selection = selection;

				mod.PrepareMT(this, Cores + 1);
			}
		}
	}

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
	static bool isRunning = true;

	// Seperate task info for a morph mod as wont be doing all verts
	void DoWork(object info)
	{
		MegaTaskInfo inf = (MegaTaskInfo)info;

		while ( isRunning )
		{
			inf.pauseevent.WaitOne(Timeout.Infinite, false);

			switch ( inf.jobtype )
			{
				case MegaJobType.Modifier:
					if ( inf.end > 0 )
					{
						for ( int m = 0; m < inf.modcontext.mod.mods.Length; m++ )
						{
							MegaModifier mod = inf.modcontext.mod.mods[m];

							if ( mod.valid )
								mod.DoWork(this, inf.index, inf.start, inf.end, Cores + 1);
						}
					}
					break;

				case MegaJobType.FaceNormalCalc:
					RecalcFaceNormalsMT(inf.modcontext.mod, inf.index, Cores + 1);
					break;

				case MegaJobType.VertexNormalCalc:
					RecalcVertexNormalsMT(inf.modcontext.mod, inf.index, Cores + 1);
					break;

				case MegaJobType.FaceTangentCalc:
					BuildFaceTangentsMT(inf.modcontext.mod, inf.index, Cores + 1);
					break;

				case MegaJobType.VertexTangentCalc:
					BuildVertexTangentsMT(inf.modcontext.mod, inf.index, Cores + 1);
					break;
			}

			inf.end = 0;	// Done the job
		}
	}
#endif

	void DoWork1(int start, int end)
	{
		for ( int m = 0; m < mods.Length; m++ )
		{
			MegaModifier mod = mods[m];
			if ( mod.valid )
			{
				mod.DoWork(this, 0, start, end, Cores + 1);
			}
		}
	}

	void OnApplicationQuit()
	{
		if ( Application.isPlaying )
		{
			MegaModifiers[] modsrunning = (MegaModifiers[])Resources.FindObjectsOfTypeAll(typeof(MegaModifiers));
			if ( modsrunning.Length == 1 )
			{
#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
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
#endif
			}
		}
	}

	void Done()
	{
		for ( int m = 0; m < mods.Length; m++ )
		{
			MegaModifier mod = mods[m];
			if ( mod.valid )
				mod.ModEnd(this);
		}
	}

	// New vertex normal calculator
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

	public MegaNormMap[]	mapping;
	public int[]		tris;
	public Vector3[]	facenorms;
	public Vector3[]	norms;

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

	// My version of recalc normals
	public void RecalcNormals(Mesh ms, Vector3[] _verts)
	{
		// so first need to recalc face normals
		// then we need a map of which faces each normal in the list uses to build its new normal value
		// to build new normal its a case of add up face normals used and average, so slow bit will be face norm calc, and preprocess of
		// building map of faces used by a normal
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

	// Threaded normal calculator
	public void RecalcFaceNormalsMT(MegaModifiers mod, int cnum, int cores)
	{
		Vector3 v30 = Vector3.zero;
		Vector3 v31 = Vector3.zero;
		Vector3 v32 = Vector3.zero;
		Vector3 va = Vector3.zero;
		Vector3 vb = Vector3.zero;

		int step = (mod.tris.Length / 3) / cores;
		int start = (cnum * step) * 3;
		int end = start + (step * 3);
		if ( cnum == cores - 1 )
			end = mod.tris.Length;

		int index = start / 3;

		for ( int f = start; f < end; f += 3 )
		{
			v30 = mod.sverts[mod.tris[f]];
			v31 = mod.sverts[mod.tris[f + 1]];
			v32 = mod.sverts[mod.tris[f + 2]];

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

			mod.facenorms[index++] = v30;
		}
	}

	public void RecalcVertexNormalsMT(MegaModifiers mod, int cnum, int cores)
	{
		Vector3 v30 = Vector3.zero;

		int step = mod.norms.Length / cores;
		int start = cnum * step;
		int end = start + step;
		if ( cnum == cores - 1 )
			end = mod.norms.Length;

		for ( int n = start; n < end; n++ )
		{
			if ( mod.mapping[n].faces.Length > 0 )
			{
				Vector3 norm = mod.facenorms[mod.mapping[n].faces[0]];

				for ( int i = 1; i < mod.mapping[n].faces.Length; i++ )
				{
					v30 = mod.facenorms[mod.mapping[n].faces[i]];
					norm.x += v30.x;
					norm.y += v30.y;
					norm.z += v30.z;
				}

				float l = norm.x * norm.x + norm.y * norm.y + norm.z * norm.z;
				l = 1.0f / Mathf.Sqrt(l);
				norm.x *= l;
				norm.y *= l;
				norm.z *= l;
				mod.norms[n] = norm;
			}
			else
				mod.norms[n] = Vector3.up;
		}
	}

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
	void WaitJobs()
	{
		if ( Cores > 0 )
		{
			int	count = 0;
			bool wait = false;
			do
			{
				wait = false;
				for ( int i = 0; i < tasks.Length; i++ )
				{
					if ( tasks[i].end > 0 )
					{
						wait = true;
						break;
					}
				}

				if ( wait )
				{
					count++;
					Thread.Sleep(0);
				}
			} while ( wait );
		}
	}

	public void RecalcNormalsMT(Mesh ms, Vector3[] _verts)
	{
		// so first need to recalc face normals
		// then we need a map of which faces each normal in the list uses to build its new normal value
		// to build new normal its a case of add up face normals used and average, so slow bit will be face norm calc, and preprocess of
		// building map of faces used by a normal
		for ( int i = 0; i < Cores; i++ )
		{
			tasks[i].jobtype = MegaJobType.FaceNormalCalc;
			tasks[i].end = 1;
			tasks[i].pauseevent.Set();
		}

		// Do this thread work
		RecalcFaceNormalsMT(this, 0, Cores + 1);	// Bias the first job to reduce wait

		WaitJobs();

		for ( int i = 0; i < Cores; i++ )
		{
			tasks[i].jobtype = MegaJobType.VertexNormalCalc;
			tasks[i].end = 1;
			tasks[i].pauseevent.Set();
		}

		RecalcVertexNormalsMT(this, 0, Cores + 1);	// Bias the first job to reduce wait

		WaitJobs();
		ms.normals = norms;
	}

	void BuildTangentsMT(Mesh ms, Vector3[] _verts)
	{
		if ( uvs == null )
			return;

		int vertexCount = _verts.Length;

		if ( tan1 == null || tan1.Length != vertexCount )
			tan1 = new Vector3[vertexCount];

		if ( tan2 == null || tan2.Length != vertexCount )
			tan2 = new Vector3[vertexCount];

		if ( tangents == null || tangents.Length != vertexCount )
			tangents = new Vector4[vertexCount];

		if ( tris == null || tris.Length == 0 )
			tris = ms.triangles;

		if ( norms == null || norms.Length == 0 )
			norms = ms.normals;

		// Do task set up and start jobs
		for ( int i = 0; i < Cores; i++ )
		{
			tasks[i].jobtype = MegaJobType.FaceTangentCalc;
			tasks[i].end = 1;
			tasks[i].pauseevent.Set();
		}

		// Do this thread work
		BuildFaceTangentsMT(this, 0, Cores + 1);	// Bias the first job to reduce wait

		WaitJobs();

		for ( int i = 0; i < Cores; i++ )
		{
			tasks[i].jobtype = MegaJobType.VertexTangentCalc;
			tasks[i].end = 1;
			tasks[i].pauseevent.Set();
		}

		BuildVertexTangentsMT(this, 0, Cores + 1);	// Bias the first job to reduce wait

		WaitJobs();

		mesh.tangents = tangents;
	}

	void BuildFaceTangentsMT(MegaModifiers mc, int cnum, int cores)
	{
		int triangleCount = mc.tris.Length;

		int step = (triangleCount / 3) / cores;
		int start = (cnum * step) * 3;
		int end = start + (step * 3);
		if ( cnum == cores - 1 )
			end = triangleCount;

		Vector3 v1 = Vector3.zero;
		Vector3 v2 = Vector3.zero;
		Vector3 v3 = Vector3.zero;

		Vector2 w1 = Vector3.zero;
		Vector2 w2 = Vector3.zero;
		Vector2 w3 = Vector3.zero;
		int	i1,i2,i3;
		for ( int a = start; a < end; a += 3 )
		{
			i1 = mc.tris[a];
			i2 = mc.tris[a + 1];
			i3 = mc.tris[a + 2];
			v1 = mc.sverts[i1];
			v2 = mc.sverts[i2];
			v3 = mc.sverts[i3];

			w1 = mc.uvs[i1];
			w2 = mc.uvs[i2];
			w3 = mc.uvs[i3];
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

			lock(mc)
			{
				mc.tan1[i1].x += sdir.x;
				mc.tan1[i1].y += sdir.y;
				mc.tan1[i1].z += sdir.z;
				mc.tan1[i2].x += sdir.x;
				mc.tan1[i2].y += sdir.y;
				mc.tan1[i2].z += sdir.z;
				mc.tan1[i3].x += sdir.x;
				mc.tan1[i3].y += sdir.y;
				mc.tan1[i3].z += sdir.z;

				mc.tan2[i1].x += tdir.x;
				mc.tan2[i1].y += tdir.y;
				mc.tan2[i1].z += tdir.z;
				mc.tan2[i2].x += tdir.x;
				mc.tan2[i2].y += tdir.y;
				mc.tan2[i2].z += tdir.z;
				mc.tan2[i3].x += tdir.x;
				mc.tan2[i3].y += tdir.y;
				mc.tan2[i3].z += tdir.z;
			}
		}
	}

	void BuildVertexTangentsMT(MegaModifiers mc, int cnum, int cores)
	{
		int vertexCount = mc.sverts.Length;

		int step = vertexCount / cores;
		int start = cnum * step;
		int end = start + step;
		if ( cnum == cores - 1 )
			end = vertexCount;

		for ( int a = start; a < end; a++ )
		{
			Vector3 n = mc.norms[a];
			Vector3 t = mc.tan1[a];

			Vector3.OrthoNormalize(ref n, ref t);
			mc.tangents[a].x = t.x;
			mc.tangents[a].y = t.y;
			mc.tangents[a].z = t.z;
			mc.tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), mc.tan2[a]) < 0.0f) ? -1.0f : 1.0f;
		}
	}
#endif	// UNITYFLASH

	// ***************************************************************************
	// PS3 Code
	// ***************************************************************************
#if true	//UNITY_PS3
	void Awake()
	{
		if ( Application.platform == RuntimePlatform.PS3 && !Application.isEditor )
		{
			Mesh ps3mesh = MegaUtils.GetMesh(gameObject);
			BuildPS3Mapping(verts, ps3mesh.vertices);
		}
	}

	Vector3[]		ps3verts;
	MegaPS3Vert[]	ps3mapping;

	// PS3 vertices that use an orginal mesh vertex 
	public class MegaPS3Vert
	{
		public int[]	indices;
	}

	// List to hold the shared indicies
	static List<int> matches = new List<int>();

	// Build an array of ps3 vertices that match an original vertex
	int[] FindMatches(Vector3 p, Vector3[] array)
	{
		matches.Clear();

		for ( int i = 0; i < array.Length; i++ )
		{
			if ( array[i].x == p.x && array[i].y == p.y && array[i].z == p.z )
				matches.Add(i);
		}
		return matches.ToArray();
	}

	// Build a mapping table for each original mesh vertex to find the vertices that share its position
	public void BuildPS3Mapping(Vector3[] oldverts, Vector3[] newverts)
	{
		ps3verts = new Vector3[newverts.Length];
		ps3mapping = new MegaPS3Vert[oldverts.Length];

		for ( int i = 0; i < oldverts.Length; i++ )
		{
			MegaPS3Vert ps3vert = new MegaPS3Vert();
			ps3vert.indices = FindMatches(oldverts[i], newverts);
			ps3mapping[i] = ps3vert;
		}
	}

	// Set the PS3 mesh vertices from the internal modified vertex array to the new ps3 vertex array
	public void SetPS3Mesh()
	{
		// This is here to test the remapping works in editor mode,
		// remove for use as Awake() will do this.
		if ( ps3mapping == null || ps3mapping.Length == 0 )
		{
			Mesh ps3mesh = MegaUtils.GetMesh(gameObject);
			BuildPS3Mapping(verts, ps3mesh.vertices);
		}

		if ( (dirtyChannels & MegaModChannel.Verts) != 0 )
		{
			for ( int i = 0; i < sverts.Length; i++ )
			{
				for ( int m = 0; m < ps3mapping[i].indices.Length; m++ )
					ps3verts[ps3mapping[i].indices[m]] = sverts[i];
			}

			mesh.vertices = ps3verts;
		}

		if ( (dirtyChannels & MegaModChannel.UV) != 0 )
			mesh.uv = suvs;

		if ( recalcnorms )
		{
			RecalcNormals();
		}

		if ( recalcTangents )
			BuildTangents();

		if ( recalcbounds )
			mesh.RecalculateBounds();

		if ( recalcCollider )
		{
			if ( meshCol == null )
				meshCol = GetComponent<MeshCollider>();

			if ( meshCol != null )
			{
				meshCol.sharedMesh = null;
				meshCol.sharedMesh = mesh;
			}
		}
	}
#endif
}
// 1553
// 1326