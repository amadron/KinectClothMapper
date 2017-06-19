
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

[CanEditMultipleObjects, CustomEditor(typeof(MegaCacheOBJ))]
public class MegaCacheOBJEditor : Editor
{
	SerializedProperty _prop_firstframe;
	SerializedProperty _prop_lastframe;
	SerializedProperty _prop_skip;
	SerializedProperty _prop_scale;
	SerializedProperty _prop_adjustcords;
	SerializedProperty _prop_buildtangents;
	SerializedProperty _prop_loadmtls;
	SerializedProperty _prop_saveuvs;
	SerializedProperty _prop_savenormals;
	SerializedProperty _prop_savetangents;
	SerializedProperty _prop_optimize;
	SerializedProperty _prop_time;
	SerializedProperty _prop_fps;
	SerializedProperty _prop_speed;
	SerializedProperty _prop_loopmode;
	SerializedProperty _prop_frame;
	SerializedProperty _prop_runtimefolder;
	SerializedProperty _prop_updatecollider;

	[MenuItem("GameObject/Create Other/MegaCache/OBJ Cache")]
	static void CreateOBJCache()
	{
		Vector3 pos = Vector3.zero;
		if ( UnityEditor.SceneView.lastActiveSceneView != null )
			pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("Mega Cache Obj");

		go.AddComponent<MegaCacheOBJ>();
		go.transform.position = pos;
		Selection.activeObject = go;
	}

	private void OnEnable()
	{
		_prop_firstframe	= serializedObject.FindProperty("firstframe");
		_prop_lastframe		= serializedObject.FindProperty("lastframe");
		_prop_skip			= serializedObject.FindProperty("skip");
		_prop_scale			= serializedObject.FindProperty("scale");
		_prop_adjustcords	= serializedObject.FindProperty("adjustcoord");
		_prop_buildtangents	= serializedObject.FindProperty("buildtangents");
		_prop_loadmtls		= serializedObject.FindProperty("loadmtls");
		_prop_saveuvs		= serializedObject.FindProperty("saveuvs");
		_prop_savenormals	= serializedObject.FindProperty("savenormals");
		_prop_savetangents	= serializedObject.FindProperty("savetangents");
		_prop_optimize		= serializedObject.FindProperty("optimize");
		_prop_time			= serializedObject.FindProperty("time");
		_prop_fps			= serializedObject.FindProperty("fps");
		_prop_speed			= serializedObject.FindProperty("speed");
		_prop_loopmode		= serializedObject.FindProperty("loopmode");
		_prop_frame			= serializedObject.FindProperty("frame");
		_prop_runtimefolder = serializedObject.FindProperty("runtimefolder");
		_prop_updatecollider = serializedObject.FindProperty("updatecollider");
	}

	public override void OnInspectorGUI()
	{
		MegaCacheOBJ mod = (MegaCacheOBJ)target;

		serializedObject.Update();

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif

		EditorGUILayout.BeginVertical("box");
		mod.showdataimport = EditorGUILayout.Foldout(mod.showdataimport, "Data Import");

		if ( mod.showdataimport )
		{
			EditorGUILayout.PropertyField(_prop_firstframe, new GUIContent("First"));
			EditorGUILayout.PropertyField(_prop_lastframe, new GUIContent("Last"));
			EditorGUILayout.PropertyField(_prop_skip, new GUIContent("Skip"));

			int val = 0;
			mod.decformat = EditorGUILayout.IntSlider("Format name" + val.ToString("D" + mod.decformat) + ".obj", mod.decformat, 1, 6);
			mod.namesplit = EditorGUILayout.TextField("Name Split Char", mod.namesplit);
			EditorGUILayout.PropertyField(_prop_scale, new GUIContent("Import Scale"));
			EditorGUILayout.PropertyField(_prop_adjustcords, new GUIContent("Adjust Coords"));
			EditorGUILayout.PropertyField(_prop_buildtangents, new GUIContent("Build Tangents"));
			EditorGUILayout.PropertyField(_prop_updatecollider, new GUIContent("Update Collider"));
			EditorGUILayout.PropertyField(_prop_loadmtls, new GUIContent("Load Materials"));

			if ( GUILayout.Button("Load Frames") )
			{
				string file = EditorUtility.OpenFilePanel("OBJ File", mod.lastpath, "obj");

				if ( file != null && file.Length > 1 )
				{
					mod.lastpath = file;
					LoadOBJ(mod, file, mod.firstframe, mod.lastframe, mod.skip);
				}
			}

			if ( mod.meshes.Count > 0 )
			{
				if ( GUILayout.Button("Clear Stored Meshes") )
					mod.DestroyMeshes();
			}

			EditorGUILayout.EndVertical();
		}

		mod.showdata = EditorGUILayout.Foldout(mod.showdata, "Data");

		if ( mod.showdata )
		{
			MegaCacheData src = (MegaCacheData)EditorGUILayout.EnumPopup("Data Source", mod.datasource);

			if ( src != mod.datasource )
				mod.ChangeSource(src);

			switch ( mod.datasource )
			{
				case MegaCacheData.Mesh:
					if ( mod.meshes.Count > 0 )
					{
						EditorGUILayout.BeginVertical("box");
						EditorGUILayout.PropertyField(_prop_saveuvs, new GUIContent("Save Uvs"));
						EditorGUILayout.PropertyField(_prop_savenormals, new GUIContent("Save Normals"));
						EditorGUILayout.PropertyField(_prop_savetangents, new GUIContent("Save Tangents"));
						EditorGUILayout.PropertyField(_prop_optimize, new GUIContent("Optimize Data"));

						if ( GUILayout.Button("Save MegaCache File") )
						{
							string file = EditorUtility.SaveFilePanel("MegaCache File", mod.lastpath, mod.name, "mgc");

							if ( file != null && file.Length > 1 )
							{
								mod.CloseCache();
								CreateCacheFile(file);
								if ( mod.cachefile.Length == 0 )
									mod.cachefile = file;
							}
						}

						if ( GUILayout.Button("Create Image") )
							CreateCacheImage();

						EditorGUILayout.EndVertical();

					}
					break;

				case MegaCacheData.File:
					EditorGUILayout.BeginVertical("box");
					EditorGUILayout.TextArea("Cache File: " + mod.cachefile);

					if ( GUILayout.Button("Select MegaCache File") )
					{
						string file = EditorUtility.OpenFilePanel("MegaCache File", mod.lastpath, "mgc");

						if ( file != null && file.Length > 1 )
						{
							mod.CloseCache();
							mod.cachefile = file;
							mod.update = true;
							mod.OpenCache(mod.cachefile);
						}
					}

					EditorGUILayout.PropertyField(_prop_runtimefolder, new GUIContent("Runtime Folder"));

					if ( mod.cachefile.Length > 0 )
					{
						if ( GUILayout.Button("Create Image From Cache") )
						{
							bool doit = true;
							if ( mod.cacheimage )
							{
								if ( !EditorUtility.DisplayDialog("Add to or Replace", "Image already loaded do you want to Replace?", "Yes", "No") )
									doit = false;
							}

							if ( doit )
							{
								mod.CreateImageFromCacheFile();
							}
						}
					}

					EditorGUILayout.EndVertical();
					break;

				case MegaCacheData.Image:
					if ( mod.cacheimage )
					{
						EditorGUILayout.BeginVertical("box");
#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
						mod.cacheimage.threadupdate = EditorGUILayout.Toggle("Preload", mod.cacheimage.threadupdate);
#endif
						if ( GUILayout.Button("Delete Image") )
						{
							mod.DestroyImage();	// = null;
						}
						EditorGUILayout.EndVertical();
					}
					break;
			}

			string info = "";

			info += "Frame Verts: " + mod.framevertcount + "\nFrame Tris: " + (mod.frametricount / 3);

			if ( mod.datasource == MegaCacheData.Image )
			{
				if ( mod.cacheimage )
					info += "\nMemory: " + mod.cacheimage.memoryuse / (1024 * 1024) + "MB";
				else
					info += "\nNo Image File";
			}

			EditorGUILayout.HelpBox(info, MessageType.None);
		}

		mod.showanimation = EditorGUILayout.Foldout(mod.showanimation, "Animation");

		if ( mod.showanimation )
		{
			EditorGUILayout.BeginVertical("box");

			int fc = 0;
			switch ( mod.datasource )
			{
				case MegaCacheData.Mesh: fc = mod.meshes.Count - 1; break;
				case MegaCacheData.File: fc = mod.framecount - 1; break;
				case MegaCacheData.Image:
					if ( mod.cacheimage && mod.cacheimage.frames != null )
						fc = mod.cacheimage.frames.Count - 1;
					break;
			}

			if ( fc > 0 )
				EditorGUILayout.IntSlider(_prop_frame, 0, fc);

			mod.animate = EditorGUILayout.BeginToggleGroup("Animate", mod.animate);
			EditorGUILayout.PropertyField(_prop_time, new GUIContent("Time"));
			EditorGUILayout.PropertyField(_prop_fps, new GUIContent("Fps"));
			EditorGUILayout.PropertyField(_prop_speed, new GUIContent("Speed"));
			EditorGUILayout.PropertyField(_prop_loopmode, new GUIContent("Loop Mode"));

			EditorGUILayout.EndToggleGroup();
			EditorGUILayout.EndVertical();
		}

		mod.showextras = EditorGUILayout.Foldout(mod.showextras, "Extra Options");

		if ( mod.showextras )
		{
			mod.shownormals = EditorGUILayout.BeginToggleGroup("Show Normals", mod.shownormals);
			mod.normallen = EditorGUILayout.FloatField("Normal Length", mod.normallen);
			EditorGUILayout.EndToggleGroup();
		}

		if ( GUI.changed )
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}
	}

	Material CreateMaterial(string name, string shader)
	{
		if ( HaveMaterial(name) )
			return (Material)AssetDatabase.LoadAssetAtPath("Assets/MegaCache/" + name + ".mat", typeof(Material));

		Material mat = new Material(Shader.Find(shader));

		if ( !Directory.Exists("Assets/MegaCache") )
			AssetDatabase.CreateFolder("Assets", "MegaCache");

		string meshpath = "Assets/MegaCache/" + name + ".mat";
		AssetDatabase.CreateAsset(mat, meshpath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		return mat;
	}

	public void LoadOBJ(MegaCacheOBJ mod, string filename, int first, int last, int step)
	{
		if ( mod.meshes.Count > 0 )
		{
			if ( !EditorUtility.DisplayDialog("Add to or Replace", "Add new OBJ meshes to existing list, or Replace All", "Add", "Replace") )
				mod.DestroyMeshes();
		}

		if ( step < 1 )
			step = 1;

		mod.InitImport();

		for ( int i = first; i <= last; i += step )
			mod.LoadMtl(filename, i);

		for ( int i = first; i <= last; i += step )
		{
			float a = (float)(i + 1 - first) / (last - first);
			if ( !EditorUtility.DisplayCancelableProgressBar("Loading OBJ Meshes", "Frame " + i, a) )
			{
				Mesh ms = mod.LoadFrame(filename, i);
				if ( ms )
					mod.AddMesh(ms);
				else
				{
					EditorUtility.DisplayDialog("Can't Load File", "Could not load frame " + i + " of sequence! Import Stopped.", "OK");
					break;
				}
			}
			else
				break;
		}

		EditorUtility.ClearProgressBar();

		if ( mod.loadmtls )
		{
			int count = MegaCacheObjImporter.NumMtls();
			Material[] mats = new Material[count];

			for ( int i = 0; i < count; i++ )
			{
				MegaCacheOBJMtl mtl = MegaCacheObjImporter.GetMtl(i);

				switch ( mtl.illum )
				{
					case 0:
					case 1:
						mats[i] = CreateMaterial(mtl.name, "Diffuse");
						break;

					case 2:
						mats[i] = CreateMaterial(mtl.name, "Specular");
						mats[i].SetColor("_SpecCol", mtl.Ks);
						mats[i].SetFloat("_Shininess", mtl.Ns);
						break;

					case 4:
					case 6:
					case 7:
					case 9:
						mats[i] = CreateMaterial(mtl.name, "Transparent/Specular");
						mats[i].SetColor("_SpecCol", mtl.Ks);
						mats[i].SetFloat("_Shininess", mtl.Ns);
						break;

					case 3:
					case 5:
					case 8:
						mats[i] = CreateMaterial(mtl.name, "Reflection/Specular");
						mats[i].SetColor("_SpecCol", mtl.Ks);
						mats[i].SetFloat("_Shininess", mtl.Ns);
						break;
				}

				mats[i].name = mtl.name;

				mats[i].color = mtl.Kd;
				if ( mtl.map_Kd != null )
					mats[i].mainTexture = LoadTexture(mtl.map_Kd);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			mod.GetComponent<Renderer>().sharedMaterials = mats;
		}
	}

	void CreateCacheImage()
	{
		MegaCacheOBJ mod = (MegaCacheOBJ)target;

		if ( mod.cacheimage )
			mod.DestroyImage();

		MegaCacheImage img = CreateInstance<MegaCacheImage>();

		img.maxv = 0;
		img.maxsm = 0;

		for ( int i = 0; i < mod.meshes.Count; i++ )
		{
			if ( mod.meshes[i].vertexCount > img.maxv )
				img.maxv = mod.meshes[i].vertexCount;

			int sub = mod.meshes[i].subMeshCount;
			if ( sub > img.maxsm )
				img.maxsm = sub;
		}

		img.smfc = new int[img.maxsm];

		for ( int i = 0; i < mod.meshes.Count; i++ )
		{
			for ( int s = 0; s < mod.meshes[i].subMeshCount; s++ )
			{
				int len = mod.meshes[i].GetTriangles(s).Length;

				if ( len > img.smfc[s] )
					img.smfc[s] = len;
			}
		}

		for ( int i = 0; i < mod.meshes.Count; i++ )
		{
			Mesh ms = mod.meshes[i];

			MegaCacheImageFrame frame = MegaCacheImage.CreateImageFrame(ms);
			img.frames.Add(frame);
		}

		img.CalcMemory();
		mod.cacheimage = img;
	}

	void CreateCacheFile(string filename)
	{
		MegaCacheOBJ mod = (MegaCacheOBJ)target;

		// save cache file
		FileStream fs = new FileStream(filename, FileMode.Create);
		if ( fs != null )
		{
			BinaryWriter bw = new BinaryWriter(fs);

			if ( bw != null )
			{
				int version = 0;

				bw.Write(version);
				bw.Write((int)mod.meshes.Count);

				bw.Write(mod.optimize);

				// max number of verts and tris, so we can allocate a single buffer
				long[] vals = new long[mod.meshes.Count];

				int maxv = 0;
				int maxf = 0;
				int maxsm = 0;

				for ( int i = 0; i < mod.meshes.Count; i++ )
				{
					if ( mod.meshes[i].vertexCount > maxv )
						maxv = mod.meshes[i].vertexCount;

					if ( mod.meshes[i].triangles.Length > maxf )
						maxf = mod.meshes[i].triangles.Length;

					int sub = mod.meshes[i].subMeshCount;
					if ( sub > maxsm )
						maxsm = sub;
				}

				int[] smfc = new int[maxsm];

				for ( int i = 0; i < mod.meshes.Count; i++ )
				{
					for ( int s = 0; s < mod.meshes[i].subMeshCount; s++ )
					{
						int len = mod.meshes[i].GetTriangles(s).Length;

						if ( len > smfc[s] )
						{
							smfc[s] = len;
						}
					}
				}

				bw.Write(maxv);
				bw.Write(maxf);
				bw.Write(maxsm);

				for ( int i = 0; i < smfc.Length; i++ )
					bw.Write(smfc[i]);

				long fp = fs.Position;
				for ( int i = 0; i < mod.meshes.Count; i++ )
				{
					long val = 0;
					bw.Write(val);
				}

				for ( int i = 0; i < mod.meshes.Count; i++ )
				{
					Mesh ms = mod.meshes[i];
					vals[i] = fs.Position;

					Vector3[] verts = ms.vertices;
					Vector3[] norms = ms.normals;
					Vector2[] uvs = ms.uv;
					Vector4[] tangents = ms.tangents;

					bw.Write(verts.Length);

					if ( mod.savenormals )
						bw.Write(norms.Length);
					else
						bw.Write((int)0);

					if ( mod.saveuvs )
						bw.Write(uvs.Length);
					else
						bw.Write((int)0);

					if ( mod.savetangents )
						bw.Write(tangents.Length);
					else
						bw.Write((int)0);

					Vector3 bmin = ms.bounds.min;
					Vector3 bmax = ms.bounds.max;

					bw.Write(bmin.x);
					bw.Write(bmin.y);
					bw.Write(bmin.z);

					bw.Write(bmax.x);
					bw.Write(bmax.y);
					bw.Write(bmax.z);

					Vector3 msize = ms.bounds.size;

					if ( mod.optimize )
					{
						for ( int v = 0; v < verts.Length; v++ )
						{
							Vector3 pos = verts[v];

							short sb = (short)(((pos.x - bmin.x) / msize.x) * 65535.0f);
							bw.Write(sb);

							sb = (short)(((pos.y - bmin.y) / msize.y) * 65535.0f);
							bw.Write(sb);
							sb = (short)(((pos.z - bmin.z) / msize.z) * 65535.0f);
							bw.Write(sb);
						}
					}
					else
					{
						for ( int v = 0; v < verts.Length; v++ )
						{
							Vector3 pos = verts[v];
							bw.Write(pos.x);
							bw.Write(pos.y);
							bw.Write(pos.z);
						}
					}

					if ( mod.savenormals )
					{
						if ( mod.optimize )
						{
							for ( int v = 0; v < norms.Length; v++ )
							{
								Vector3 pos = norms[v];

								sbyte sb = (sbyte)(pos.x * 127.0f);
								bw.Write(sb);

								sb = (sbyte)(pos.y * 127.0f);
								bw.Write(sb);
								sb = (sbyte)(pos.z * 127.0f);
								bw.Write(sb);
							}
						}
						else
						{
							for ( int v = 0; v < norms.Length; v++ )
							{
								Vector3 pos = norms[v];
								bw.Write(pos.x);
								bw.Write(pos.y);
								bw.Write(pos.z);
							}
						}
					}

					if ( mod.savetangents )
					{
						if ( mod.optimize )
						{
							for ( int v = 0; v < tangents.Length; v++ )
							{
								Vector4 pos = tangents[v];

								sbyte sb = (sbyte)(pos.x * 127.0f);
								bw.Write(sb);

								sb = (sbyte)(pos.y * 127.0f);
								bw.Write(sb);
								sb = (sbyte)(pos.z * 127.0f);
								bw.Write(sb);
								sb = (sbyte)(pos.w * 127.0f);
								bw.Write(sb);
							}
						}
						else
						{
							for ( int v = 0; v < tangents.Length; v++ )
							{
								Vector4 tan = tangents[v];
								bw.Write(tan.x);
								bw.Write(tan.y);
								bw.Write(tan.z);
								bw.Write(tan.w);
							}
						}
					}

					if ( mod.saveuvs )
					{
						if ( mod.optimize )
						{
							Bounds uvb = MegaCacheUtils.GetBounds(uvs);

							bw.Write(uvb.min.x);
							bw.Write(uvb.min.y);
							bw.Write(uvb.max.x);
							bw.Write(uvb.max.y);

							for ( int v = 0; v < uvs.Length; v++ )
							{
								Vector2 pos = uvs[v];

								sbyte sb = (sbyte)(((pos.x - uvb.min.x) / uvb.size.x) * 255.0f);
								bw.Write(sb);

								sb = (sbyte)(((pos.y - uvb.min.y) / uvb.size.y) * 255.0f);
								bw.Write(sb);
							}
						}
						else
						{
							for ( int v = 0; v < uvs.Length; v++ )
							{
								Vector2 uv = uvs[v];
								bw.Write(uv.x);
								bw.Write(uv.y);
							}
						}
					}

					byte scount = (byte)ms.subMeshCount;

					bw.Write(scount);

					for ( int s = 0; s < scount; s++ )
					{
						int[] tris = ms.GetTriangles(s);

						bw.Write(tris.Length);

						for ( int t = 0; t < tris.Length; t++ )
						{
							ushort ix = (ushort)tris[t];
							bw.Write(ix);
						}
					}
				}

				fs.Position = fp;

				for ( int i = 0; i < vals.Length; i++ )
					bw.Write(vals[i]);

				bw.Close();
			}

			fs.Close();
		}
	}

	Texture2D LoadTexture(string filename)
	{
		Texture2D tex = null;

		if ( HaveTexture(filename) )
			return (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/MegaCache/" + Path.GetFileNameWithoutExtension(filename) + ".asset", typeof(Texture2D));

		if ( File.Exists(filename) )
		{
			byte[] buf = File.ReadAllBytes(filename);

			tex = new Texture2D(2, 2);
			tex.name = Path.GetFileNameWithoutExtension(filename);
			tex.LoadImage(buf);

			if ( !Directory.Exists("Assets/MegaCache") )
				AssetDatabase.CreateFolder("Assets", "MegaCache");

			string meshpath = "Assets/MegaCache/" + tex.name + ".asset";
			AssetDatabase.CreateAsset(tex, meshpath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		return tex;
	}

	bool HaveTexture(string filename)
	{
		if ( File.Exists("Assets/MegaCache/" + Path.GetFileNameWithoutExtension(filename) + ".asset") )
			return true;

		return false;
	}

	bool HaveMaterial(string name)
	{
		if ( File.Exists("Assets/MegaCache/" + name + ".mat") )
			return true;

		return false;
	}
}
