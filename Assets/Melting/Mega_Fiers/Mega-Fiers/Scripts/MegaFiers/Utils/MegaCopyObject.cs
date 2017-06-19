
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

#if !UNITY_FLASH //&& !UNITY_METRO && !UNITY_WP8
public class MegaCopyObject
{
	static GameObject CopyMesh(GameObject subject)
	{
		GameObject clone = (GameObject)GameObject.Instantiate(subject);

		MeshFilter[] mfs = subject.GetComponentsInChildren<MeshFilter>();
		MeshFilter[] clonemfs = clone.GetComponentsInChildren<MeshFilter>();

		MeshCollider[] mcs = clone.GetComponentsInChildren<MeshCollider>();
		MeshCollider[] clonemcs = clone.GetComponentsInChildren<MeshCollider>();

		int l = mfs.Length;

		for ( int i = 0; i < l; i++ )
		{
			MeshFilter mf = mfs[i];
			MeshFilter clonemf = clonemfs[i];
			Mesh mesh = mf.sharedMesh;
			Mesh clonemesh = new Mesh();
			clonemesh.vertices = mesh.vertices;
#if UNITY_5_0 || UNITY_5_1 || UNITY_5 || UNITY_6
			clonemesh.uv2 = mesh.uv2;
			clonemesh.uv3 = mesh.uv3;
			clonemesh.uv4 = mesh.uv4;
#else
			clonemesh.uv1 = mesh.uv1;
			clonemesh.uv2 = mesh.uv2;
#endif
			clonemesh.uv = mesh.uv;
			clonemesh.normals = mesh.normals;
			clonemesh.tangents = mesh.tangents;
			clonemesh.colors = mesh.colors;

			clonemesh.subMeshCount = mesh.subMeshCount;

			for ( int s = 0; s < mesh.subMeshCount; s++ )
			{
				clonemesh.SetTriangles(mesh.GetTriangles(s), s);
			}

			//clonemesh.triangles = mesh.triangles;

#if UNITY_5_3 || UNITY_5_4 || UNITY_6
			CopyBlendShapes(mesh, clonemesh);
#if false
			int bcount = mesh.blendShapeCount;	//GetBlendShapeFrameCount();

			Vector3[] deltaverts = new Vector3[mesh.vertexCount];
			Vector3[] deltanorms = new Vector3[mesh.vertexCount];
			Vector3[] deltatans = new Vector3[mesh.vertexCount];

			for ( int j = 0; j < bcount; j++ )
			{
				int frames = mesh.GetBlendShapeFrameCount(j);
				string bname = mesh.GetBlendShapeName(j);

				for ( int f = 0; f < frames; f++ )
				{
					mesh.GetBlendShapeFrameVertices(j, f, deltaverts, deltanorms, deltatans);
					float weight = mesh.GetBlendShapeFrameWeight(j, f);

					clonemesh.AddBlendShapeFrame(bname, weight, deltaverts, deltanorms, deltatans);
				}
			}
#endif
#endif

			clonemesh.boneWeights = mesh.boneWeights;
			clonemesh.bindposes = mesh.bindposes;
			clonemesh.name = mesh.name + "_copy";
			clonemesh.RecalculateBounds();
			clonemf.sharedMesh = clonemesh;

			for ( int j = 0; j < mcs.Length; j++ )
			{
				MeshCollider mc = mcs[j];
				if ( mc.sharedMesh == mesh )
					clonemcs[j].sharedMesh = clonemesh;
			}
		}

		return clone;
	}

	static void CopyBlendShapes(Mesh mesh, Mesh clonemesh)
	{
#if UNITY_5_3 || UNITY_5_4 || UNITY_6
		int bcount = mesh.blendShapeCount;	//GetBlendShapeFrameCount();

		Vector3[] deltaverts = new Vector3[mesh.vertexCount];
		Vector3[] deltanorms = new Vector3[mesh.vertexCount];
		Vector3[] deltatans = new Vector3[mesh.vertexCount];

		for ( int j = 0; j < bcount; j++ )
		{
			int frames = mesh.GetBlendShapeFrameCount(j);
			string bname = mesh.GetBlendShapeName(j);

			for ( int f = 0; f < frames; f++ )
			{
				mesh.GetBlendShapeFrameVertices(j, f, deltaverts, deltanorms, deltatans);
				float weight = mesh.GetBlendShapeFrameWeight(j, f);

				clonemesh.AddBlendShapeFrame(bname, weight, deltaverts, deltanorms, deltatans);
			}
		}
#endif
	}

	static GameObject CopyMesh(GameObject subject, MegaModifyObject mod)
	{
		GameObject clone = new GameObject();	//(GameObject)GameObject.Instantiate(subject);

		MeshFilter newmf = clone.AddComponent<MeshFilter>();

		SkinnedMeshRenderer oldsmr = subject.GetComponent<SkinnedMeshRenderer>();
		SkinnedMeshRenderer newsmr = null;

		if ( oldsmr )
		{
			newsmr = clone.AddComponent<SkinnedMeshRenderer>();

			newsmr.sharedMaterials = oldsmr.sharedMaterials;
		}
		else
		{
			MeshRenderer oldmr = subject.GetComponent<MeshRenderer>();
			MeshRenderer newmr = clone.AddComponent<MeshRenderer>();

			newmr.sharedMaterials = oldmr.sharedMaterials;
		}

		MeshFilter[] mfs = subject.GetComponentsInChildren<MeshFilter>();

		MeshCollider[] mcs = clone.GetComponentsInChildren<MeshCollider>();
		MeshCollider[] clonemcs = clone.GetComponentsInChildren<MeshCollider>();

		int l = mfs.Length;

		for ( int i = 0; i < l; i++ )
		{
			MeshFilter mf = mfs[i];
			Mesh mesh = mf.sharedMesh;
			Mesh clonemesh = new Mesh();

			clonemesh.vertices = mod.verts;	//mesh.vertices;
#if UNITY_5_0 || UNITY_5_1 || UNITY_5
			clonemesh.uv2 = mesh.uv2;
			clonemesh.uv3 = mesh.uv3;
			clonemesh.uv4 = mesh.uv4;
#else
			clonemesh.uv1 = mesh.uv1;
			clonemesh.uv2 = mesh.uv2;
#endif
			clonemesh.uv = mod.uvs;	//mesh.uv;
			if ( mod.NormalMethod == MegaNormalMethod.Mega && mod.norms != null && mod.norms.Length > 0 )
				clonemesh.normals = mod.norms;	//mesh.normals;
			else
				clonemesh.normals = mesh.normals;

			clonemesh.tangents = mesh.tangents;
			clonemesh.colors = mesh.colors;

			clonemesh.subMeshCount = mesh.subMeshCount;

			for ( int s = 0; s < mesh.subMeshCount; s++ )
				clonemesh.SetTriangles(mesh.GetTriangles(s), s);

			CopyBlendShapes(mesh, clonemesh);

			clonemesh.boneWeights = mesh.boneWeights;
			clonemesh.bindposes = mesh.bindposes;
			clonemesh.name = mesh.name + "_copy";
			clonemesh.RecalculateBounds();

			newmf.sharedMesh = clonemesh;

			for ( int j = 0; j < mcs.Length; j++ )
			{
				MeshCollider mc = mcs[j];
				if ( mc.sharedMesh == mesh )
					clonemcs[j].sharedMesh = clonemesh;
			}

			if ( newsmr && oldsmr )
			{
				newsmr.sharedMesh = clonemesh;
#if UNITY_5_3 || UNITY_5_4 || UNITY_6
				for ( int b = 0; b < mesh.blendShapeCount; b++ )
				{
					newsmr.SetBlendShapeWeight(b, oldsmr.GetBlendShapeWeight(b));
				}
#endif
			}
		}

		return clone;
	}

	static void CopyModObj(MegaModifyObject from, MegaModifyObject to)
	{
		if ( from && to )
		{
			to.Enabled = from.Enabled;
			to.recalcbounds = from.recalcbounds;
			to.recalcCollider = from.recalcCollider;
			to.recalcnorms = from.recalcnorms;
			to.DoLateUpdate = from.DoLateUpdate;
			//to.GrabVerts = from.GrabVerts;
			to.dynamicMesh = from.dynamicMesh;
			to.DrawGizmos = from.DrawGizmos;
			to.NormalMethod = from.NormalMethod;
		}
	}

	static public GameObject DoCopyObjects(GameObject from)
	{
		MegaModifyObject fromMod = from.GetComponent<MegaModifyObject>();

		GameObject to;

		if ( fromMod )
			to = CopyMesh(from, fromMod);
		else
			to = CopyMesh(from);
		MegaModifyObject mo = to.AddComponent<MegaModifyObject>();

		CopyModObj(fromMod, mo);

		MegaModifier[] mods = from.GetComponents<MegaModifier>();

		for ( int i = 0; i < mods.Length; i++ )
		{
			Component com = CopyComponent(mods[i], to);
			// TODO: Add method to modifiers so can deal with any special cases

			if ( com )
			{
				MegaModifier mod = (MegaModifier)com;
				mod.PostCopy(mods[i]);
			}
		}

		MegaWrap wrap = from.GetComponent<MegaWrap>();

		if ( wrap )
			CopyComponent(wrap, to);

		if ( mo )
			mo.MeshUpdated();
		to.name = from.name + " - Copy";
		return to;
	}

	static public GameObject DoCopyObjectsChildren(GameObject from)
	{
		GameObject parent = DoCopyObjects(from);

		for ( int i = 0; i < from.transform.childCount; i++ )
		{
			GameObject cobj = from.transform.GetChild(i).gameObject;

			GameObject newchild = DoCopyObjectsChildren(cobj);
			newchild.transform.parent = parent.transform;
		}

		return parent;
	}

	static Component CopyComponent(Component from, GameObject to)
	{
		bool en = false;
		Type tp = from.GetType();

		if ( tp.IsSubclassOf(typeof(Behaviour)) )
		{
			en = (from as Behaviour).enabled;
		}
		else
		{
			if ( tp.IsSubclassOf(typeof(Component)) && tp.GetProperty("enabled") != null )
				en = (bool)tp.GetProperty("enabled").GetValue(from, null);
			else
				en = true;
		}

		FieldInfo[] fields = tp.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Default);	//claredOnly);
		PropertyInfo[] properties = tp.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Default);	//claredOnly);

		Component c = to.GetComponent(tp);

		if ( c == null )
			c = to.AddComponent(tp);

		if ( tp.IsSubclassOf(typeof(Behaviour)) )
			(c as Behaviour).enabled = en;
		else
		{
			if ( tp.IsSubclassOf(typeof(Component)) && tp.GetProperty("enabled") != null )
				tp.GetProperty("enabled").SetValue(c, en, null);
		}

		for ( int j = 0; j < fields.Length; j++ )
			fields[j].SetValue(c, fields[j].GetValue(from));

		for ( int j = 0; j < properties.Length; j++ )
		{
			if ( properties[j].CanWrite )
				properties[j].SetValue(c, properties[j].GetValue(from, null), null);
		}

		return c;
	}

	static public void CopyFromTo1(GameObject obj, GameObject to)
	{
		Component[] components = obj.GetComponents<Component>();

		for ( int i = 0; i < components.Length; i++ )
		{
			bool en = false;
			Type tp = components[i].GetType();

			if ( tp.IsSubclassOf(typeof(Behaviour)) )
			{
				en = (components[i] as Behaviour).enabled;
			}
			else
			{
				if ( tp.IsSubclassOf(typeof(Component)) && tp.GetProperty("enabled") != null )
					en = (bool)tp.GetProperty("enabled").GetValue(components[i], null);
				else
					en = true;
			}

			FieldInfo[] fields = tp.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Default);	//claredOnly);
			PropertyInfo[] properties = tp.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Default);	//claredOnly);

			Component c = to.GetComponent(tp);

			if ( c == null )
				c = to.AddComponent(tp);

			if ( tp.IsSubclassOf(typeof(Behaviour)) )
				(c as Behaviour).enabled = en;
			else
			{
				if ( tp.IsSubclassOf(typeof(Component)) && tp.GetProperty("enabled") != null )
					tp.GetProperty("enabled").SetValue(c, en, null);
			}

			for ( int j = 0; j < fields.Length; j++ )
				fields[j].SetValue(c, fields[j].GetValue(tp));

			for ( int j = 0; j < properties.Length; j++ )
			{
				if ( properties[j].CanWrite )
					properties[j].SetValue(c, properties[j].GetValue(tp, null), null);
			}
		}
	}

	static public GameObject DeepCopy(GameObject subject)
	{
		GameObject clone = null;
		if ( subject )
		{
			clone = (GameObject)GameObject.Instantiate(subject);

			SkinnedMeshRenderer[] skinmesh = subject.GetComponentsInChildren<SkinnedMeshRenderer>();
			SkinnedMeshRenderer[] cskinmesh = clone.GetComponentsInChildren<SkinnedMeshRenderer>();

			int l = skinmesh.Length;

			for ( int i = 0; i < l; i++ )
			{
				Mesh mesh = skinmesh[i].sharedMesh;
				Mesh clonemesh = new Mesh();
				clonemesh.vertices = mesh.vertices;
#if UNITY_5_0 || UNITY_5_1 || UNITY_5
				clonemesh.uv2 = mesh.uv2;
				clonemesh.uv3 = mesh.uv3;
				clonemesh.uv4 = mesh.uv4;
#else
				clonemesh.uv1 = mesh.uv1;
				clonemesh.uv2 = mesh.uv2;
#endif
				clonemesh.uv = mesh.uv;
				clonemesh.normals = mesh.normals;
				clonemesh.tangents = mesh.tangents;
				clonemesh.colors = mesh.colors;

				clonemesh.subMeshCount = mesh.subMeshCount;

				for ( int s = 0; s < mesh.subMeshCount; s++ )
					clonemesh.SetTriangles(mesh.GetTriangles(s), s);

				CopyBlendShapes(mesh, clonemesh);

				clonemesh.boneWeights = mesh.boneWeights;
				clonemesh.bindposes = mesh.bindposes;
				clonemesh.name = mesh.name + "_copy";
				clonemesh.RecalculateBounds();
				cskinmesh[i].sharedMesh = clonemesh;
			}

			MeshFilter[] mfs = subject.GetComponentsInChildren<MeshFilter>();
			MeshFilter[] clonemfs = clone.GetComponentsInChildren<MeshFilter>();

			MeshCollider[] mcs = clone.GetComponentsInChildren<MeshCollider>();
			MeshCollider[] clonemcs = clone.GetComponentsInChildren<MeshCollider>();

			for ( int i = 0; i < mfs.Length; i++ )
			{
				MeshFilter mf = mfs[i];
				MeshFilter clonemf = clonemfs[i];
				Mesh mesh = mf.sharedMesh;
				Mesh clonemesh = new Mesh();
				clonemesh.vertices = mesh.vertices;
#if UNITY_5_0 || UNITY_5_1 || UNITY_5
				clonemesh.uv2 = mesh.uv2;
				clonemesh.uv3 = mesh.uv3;
				clonemesh.uv4 = mesh.uv4;
#else
				clonemesh.uv1 = mesh.uv1;
				clonemesh.uv2 = mesh.uv2;
#endif
				clonemesh.uv = mesh.uv;
				clonemesh.normals = mesh.normals;
				clonemesh.tangents = mesh.tangents;
				clonemesh.colors = mesh.colors;

				clonemesh.subMeshCount = mesh.subMeshCount;

				for ( int s = 0; s < mesh.subMeshCount; s++ )
					clonemesh.SetTriangles(mesh.GetTriangles(s), s);

				CopyBlendShapes(mesh, clonemesh);
				clonemesh.boneWeights = mesh.boneWeights;
				clonemesh.bindposes = mesh.bindposes;
				clonemesh.name = mesh.name + "_copy";
				clonemesh.RecalculateBounds();
				clonemf.sharedMesh = clonemesh;

				for ( int j = 0; j < mcs.Length; j++ )
				{
					MeshCollider mc = mcs[j];
					if ( mc.sharedMesh = mesh )
						clonemcs[j].sharedMesh = clonemesh;
				}
			}

			MegaModifyObject[] modobjs = clone.GetComponentsInChildren<MegaModifyObject>();

			for ( int i = 0; i < modobjs.Length; i++ )
			{
				modobjs[i].MeshUpdated();
			}
		}

		return clone;
	}

	static public GameObject InstanceObject(GameObject obj)
	{
		GameObject newobj = null;
		if ( obj )
		{
			MeshFilter mf = obj.GetComponent<MeshFilter>();
			MeshRenderer mr = obj.GetComponent<MeshRenderer>();

			if ( mf )
			{
				newobj = new GameObject();
				newobj.name = obj.name + " MegaInstance";

				MeshRenderer newmr = newobj.AddComponent<MeshRenderer>();
				MeshFilter newmf = newobj.AddComponent<MeshFilter>();

				newmf.sharedMesh = mf.sharedMesh;
				newmr.sharedMaterials = mr.sharedMaterials;
			}
		}

		return newobj;
	}

	public static Mesh DupMesh(Mesh mesh)
	{
		Mesh clonemesh = new Mesh();
		clonemesh.vertices = mesh.vertices;
#if UNITY_5_0 || UNITY_5_1 || UNITY_5
		clonemesh.uv2 = mesh.uv2;
		clonemesh.uv3 = mesh.uv3;
		clonemesh.uv4 = mesh.uv4;
#else
		clonemesh.uv1 = mesh.uv1;
		clonemesh.uv2 = mesh.uv2;
#endif
		clonemesh.uv = mesh.uv;
		clonemesh.normals = mesh.normals;
		clonemesh.tangents = mesh.tangents;
		clonemesh.colors = mesh.colors;

		clonemesh.subMeshCount = mesh.subMeshCount;

		for ( int s = 0; s < mesh.subMeshCount; s++ )
			clonemesh.SetTriangles(mesh.GetTriangles(s), s);

		CopyBlendShapes(mesh, clonemesh);
		clonemesh.boneWeights = mesh.boneWeights;
		clonemesh.bindposes = mesh.bindposes;
		clonemesh.name = mesh.name + "_copy";
		clonemesh.RecalculateBounds();

		return clonemesh;
	}

	public static Mesh DupMesh(Mesh mesh, string suffix)
	{
		Mesh clonemesh = new Mesh();
		clonemesh.vertices = mesh.vertices;
#if UNITY_5_0 || UNITY_5_1 || UNITY_5
		clonemesh.uv2 = mesh.uv2;
		clonemesh.uv3 = mesh.uv3;
		clonemesh.uv4 = mesh.uv4;
#else
		clonemesh.uv1 = mesh.uv1;
		clonemesh.uv2 = mesh.uv2;
#endif
		clonemesh.uv = mesh.uv;
		clonemesh.normals = mesh.normals;
		clonemesh.tangents = mesh.tangents;
		clonemesh.colors = mesh.colors;

		clonemesh.subMeshCount = mesh.subMeshCount;

		for ( int s = 0; s < mesh.subMeshCount; s++ )
			clonemesh.SetTriangles(mesh.GetTriangles(s), s);

		CopyBlendShapes(mesh, clonemesh);
		clonemesh.boneWeights = mesh.boneWeights;
		clonemesh.bindposes = mesh.bindposes;
		clonemesh.name = mesh.name + suffix;
		clonemesh.RecalculateBounds();

		return clonemesh;
	}

	static public GameObject DuplicateObject(GameObject from)
	{
		GameObject newobj = null;

		if ( from )
		{
			newobj = (GameObject)GameObject.Instantiate(from);

			if ( newobj )
			{
				MeshFilter[] mfil = newobj.GetComponentsInChildren<MeshFilter>();

				for ( int i = 0; i < mfil.Length; i++ )
					mfil[i].sharedMesh = DupMesh(mfil[i].sharedMesh);

				SkinnedMeshRenderer[] skin = newobj.GetComponentsInChildren<SkinnedMeshRenderer>();

				for ( int i = 0; i < skin.Length; i++ )
					skin[i].sharedMesh = DupMesh(skin[i].sharedMesh);

				MegaModifyObject[] mobjs = newobj.GetComponentsInChildren<MegaModifyObject>();

				for ( int i = 0; i < mobjs.Length; i++ )
					mobjs[i].MeshUpdated();

				MegaModifier[] frommods = from.GetComponentsInChildren<MegaModifier>();
				MegaModifier[] tomods = newobj.GetComponentsInChildren<MegaModifier>();

				for ( int i = 0; i < frommods.Length; i++ )
					tomods[i].PostCopy(frommods[i]);

				MegaWrap[] wraps = newobj.GetComponentsInChildren<MegaWrap>();

				for ( int i = 0; i < wraps.Length; i++ )
					wraps[i].SetMesh();

				newobj.name = from.name + " - Copy";
			}
		}

		return newobj;
	}

	static public GameObject DuplicateObjectForPrefab(GameObject from)
	{
		GameObject newobj = null;

		if ( from )
		{
			newobj = (GameObject)GameObject.Instantiate(from);

			if ( newobj )
			{
				MeshFilter[] mfil = newobj.GetComponentsInChildren<MeshFilter>();

				for ( int i = 0; i < mfil.Length; i++ )
					mfil[i].sharedMesh = DupMesh(mfil[i].sharedMesh);

				SkinnedMeshRenderer[] skin = newobj.GetComponentsInChildren<SkinnedMeshRenderer>();

				for ( int i = 0; i < skin.Length; i++ )
					skin[i].sharedMesh = DupMesh(skin[i].sharedMesh);

				MegaModifyObject[] mobjs = newobj.GetComponentsInChildren<MegaModifyObject>();

				for ( int i = 0; i < mobjs.Length; i++ )
					mobjs[i].MeshUpdated();

				MegaModifier[] frommods = from.GetComponentsInChildren<MegaModifier>();
				MegaModifier[] tomods = newobj.GetComponentsInChildren<MegaModifier>();

				for ( int i = 0; i < frommods.Length; i++ )
					tomods[i].PostCopy(frommods[i]);

				MegaWrap[] wraps = newobj.GetComponentsInChildren<MegaWrap>();

				for ( int i = 0; i < wraps.Length; i++ )
					wraps[i].SetMesh();

				newobj.name = from.name;	// + " - Copy";
			}
		}

		return newobj;
	}
}
#endif
