
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaModifier))]
public class MegaModifierEditor : Editor
{
	public Texture		image;
	public bool			showhelp = false;
	public virtual Texture	LoadImage()			{ return null; }
	public virtual string	GetHelpString()	{ return "Modifier by Chris West"; }
	public virtual bool		Inspector()	{ return true; }
	public virtual bool		DisplayCommon() { return true; }

	//public bool	useUndo = true;
	private MegaModifier	src;
	private MegaUndo		undoManager;
#if false
	SerializedProperty labelProp;
	SerializedProperty maxlodProp;
	SerializedProperty modenabledProp;
	SerializedProperty displaygizmoProp;
	SerializedProperty orderProp;
	SerializedProperty gizcol2Prop;
	SerializedProperty gizcol1Prop;
	SerializedProperty gizmodetailProp;
	SerializedProperty offsetProp;
	SerializedProperty gizmoposProp;
	SerializedProperty gizmorotProp;
	SerializedProperty gizmoscaleProp;
#endif

	public virtual void Enable()
	{
	}

	private void OnEnable()
	{
		src = target as MegaModifier;

		// Instantiate undoManager
		if ( src != null )
			undoManager = new MegaUndo(src, src.ModName() + " change");
#if false
		labelProp = serializedObject.FindProperty("Label");
		maxlodProp = serializedObject.FindProperty("MaxLOD");
		modenabledProp = serializedObject.FindProperty("ModEnabled");
		displaygizmoProp = serializedObject.FindProperty("DisplayGizmo");
		orderProp = serializedObject.FindProperty("Order");
		gizcol2Prop = serializedObject.FindProperty("gizCol2");
		gizcol1Prop = serializedObject.FindProperty("gizCol1");
		gizmodetailProp = serializedObject.FindProperty("steps");
		offsetProp = serializedObject.FindProperty("Offset");
		gizmoposProp = serializedObject.FindProperty("gizmoPos");
		gizmorotProp = serializedObject.FindProperty("gizmoRot");
		gizmoscaleProp = serializedObject.FindProperty("gizmoScale");
#endif
		Enable();
	}

	void OnDestroy()
	{
#if UNITY_3_5
		MegaModifiers[] con = (MegaModifiers[])FindSceneObjectsOfType(typeof(MegaModifiers));
#else
		MegaModifiers[] con = (MegaModifiers[])FindObjectsOfType(typeof(MegaModifiers));
#endif

		for ( int i = 0; i < con.Length; i++ )
		{
			con[i].BuildList();
		}
	}

	public bool showmodparams = true;
	//bool showweight = true;

	//private static GUILayoutOption colWidth = GUILayout.MaxWidth(75.0f);

#if true
	public void CommonModParamsBasic(MegaModifier mod)
	{
		// Basic mod stuff
		//showmodparams = EditorGUILayout.Foldout(showmodparams, "Modifier Common Params");

		//if ( showmodparams )
		//{
			mod.Label = EditorGUILayout.TextField("Label", mod.Label);
			mod.MaxLOD = EditorGUILayout.IntField("MaxLOD", mod.MaxLOD);
			mod.ModEnabled = EditorGUILayout.Toggle("Mod Enabled", mod.ModEnabled);
			mod.useUndo = EditorGUILayout.Toggle("Use Undo", mod.useUndo);
			mod.DisplayGizmo = EditorGUILayout.Toggle("Display Gizmo", mod.DisplayGizmo);
			int order = EditorGUILayout.IntField("Order", mod.Order);

			if ( order != mod.Order )
			{
				mod.Order = order;

				MegaModifiers context = mod.GetComponent<MegaModifiers>();

				if ( context != null )
					context.BuildList();
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Gizmo Col");
			//mod.gizCol1 = EditorGUILayout.ColorField("Giz Col 1", mod.gizCol1, colWidth);
			//mod.gizCol2 = EditorGUILayout.ColorField("Giz Col 2", mod.gizCol2, colWidth);
			mod.gizCol1 = EditorGUILayout.ColorField(mod.gizCol1);	//, colWidth);
			mod.gizCol2 = EditorGUILayout.ColorField(mod.gizCol2);	//, colWidth);
			EditorGUILayout.EndHorizontal();

			mod.steps = EditorGUILayout.IntField("Gizmo Detail", mod.steps);
			if ( mod.steps < 1 )
				mod.steps = 1;
		//}

		//mod.useWeights = EditorGUILayout.Toggle("Use Weights", mod.useWeights);

		//if ( mod.useWeights )
		//	mod.weightChannel = (MegaWeightChannel)EditorGUILayout.EnumPopup("Weight Channel", mod.weightChannel);
	}
#else

	public void CommonModParamsBasic(MegaModifier mod)
	{
		EditorGUILayout.PropertyField(labelProp, new GUIContent("Label"));
		EditorGUILayout.PropertyField(maxlodProp, new GUIContent("MaxLOD"));
		EditorGUILayout.PropertyField(modenabledProp, new GUIContent("Mod Enabled"));
		EditorGUILayout.PropertyField(displaygizmoProp, new GUIContent("Display Gizmo"));
		int order = mod.Order;
		EditorGUILayout.PropertyField(orderProp, new GUIContent("Order"));
		if ( order != mod.Order )
		{
			mod.Order = order;

			MegaModifiers context = mod.GetComponent<MegaModifiers>();

			if ( context != null )
				context.BuildList();
		}

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Gizmo Col");
		EditorGUILayout.PropertyField(gizcol1Prop, GUIContent.none, true);	//, new GUIContent("Display Gizmo"));
		EditorGUILayout.PropertyField(gizcol2Prop, GUIContent.none, true);	//, new GUIContent("Display Gizmo"));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.PropertyField(gizmodetailProp, new GUIContent("Gizmo Detail"));	//, new GUIContent("Display Gizmo"));
	}
#endif

#if true
	public void CommonModParams(MegaModifier mod)
	{
		showmodparams = EditorGUILayout.Foldout(showmodparams, "Modifier Common Params");

		if ( showmodparams )
		{
			EditorGUILayout.BeginHorizontal();

			if ( GUILayout.Button("Rst Off") )
			{
				mod.Offset = Vector3.zero;
				EditorUtility.SetDirty(target);
			}

			if ( GUILayout.Button("Rst Pos") )
			{
				mod.gizmoPos = Vector3.zero;
				EditorUtility.SetDirty(target);
			}

			if ( GUILayout.Button("Rst Rot") )
			{
				mod.gizmoRot = Vector3.zero;
				EditorUtility.SetDirty(target);
			}

			if ( GUILayout.Button("Rst Scl") )
			{
				mod.gizmoScale = Vector3.one;
				EditorUtility.SetDirty(target);
			}
			EditorGUILayout.EndHorizontal();

			mod.Offset			= EditorGUILayout.Vector3Field("Offset", mod.Offset);
			mod.gizmoPos		= EditorGUILayout.Vector3Field("Gizmo Pos", mod.gizmoPos);
			mod.gizmoRot		= EditorGUILayout.Vector3Field("Gizmo Rot", mod.gizmoRot);
			mod.gizmoScale		= EditorGUILayout.Vector3Field("Gizmo Scale", mod.gizmoScale);
			CommonModParamsBasic(mod);
		}
	}
#else

	public void CommonModParams(MegaModifier mod)
	{
		showmodparams = EditorGUILayout.Foldout(showmodparams, "Modifier Common Params");

		if ( showmodparams )
		{
			EditorGUILayout.BeginHorizontal();

			if ( GUILayout.Button("Rst Off") )
			{
				mod.Offset = Vector3.zero;
				EditorUtility.SetDirty(target);
			}

			if ( GUILayout.Button("Rst Pos") )
			{
				mod.gizmoPos = Vector3.zero;
				EditorUtility.SetDirty(target);
			}

			if ( GUILayout.Button("Rst Rot") )
			{
				mod.gizmoRot = Vector3.zero;
				EditorUtility.SetDirty(target);
			}

			if ( GUILayout.Button("Rst Scl") )
			{
				mod.gizmoScale = Vector3.one;
				EditorUtility.SetDirty(target);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.PropertyField(offsetProp, new GUIContent("Offset"), true);
			EditorGUILayout.PropertyField(gizmoposProp, new GUIContent("Gizmo Pos"), true);
			EditorGUILayout.PropertyField(gizmorotProp, new GUIContent("Gizmo Rot"), true);
			EditorGUILayout.PropertyField(gizmoscaleProp, new GUIContent("Gizmo Scale"), true);

			CommonModParamsBasic(mod);
		}
	}
#endif

	public virtual void DrawGUI()
	{
		MegaModifier mod = (MegaModifier)target;
		MegaModifiers context = mod.GetComponent<MegaModifiers>();
		if ( context == null )
		{
			EditorGUILayout.LabelField("You need to Add a Mega Modify Object Component");
			return;
		}

		//showhelp = EditorGUILayout.Foldout(showhelp, "Help");

		//if ( showhelp )
		//{
			//if ( image == null )
				//image = LoadImage();

			//if ( image != null )
			//{
				//float w = Screen.width - 12.0f;
				//float h = (w / image.width) * image.height;

				//if ( h > image.height )
					//h = image.height;

				//GUILayout.Label((Texture)image, GUIStyle.none, GUILayout.Width(w), GUILayout.Height(h));
			//}
		//}

		if ( DisplayCommon() )
			CommonModParams((MegaModifier)target);

		if ( GUI.changed )
			EditorUtility.SetDirty(target);

		if ( Inspector() )
			DrawDefaultInspector();

		//if ( showhelp )
			//GUILayout.TextArea(GetHelpString());
	}

	public virtual void DrawSceneGUI()
	{
		MegaModifier mod = (MegaModifier)target;

		if ( mod.ModEnabled && mod.DisplayGizmo && MegaModifiers.GlobalDisplay && showmodparams )
		{
			MegaModifiers context = mod.GetComponent<MegaModifiers>();

			if ( context != null && context.Enabled && context.DrawGizmos )
			{
				//mod.Offset = -Handles.PositionHandle(-mod.Offset, Quaternion.identity);
				float a = mod.gizCol1.a;
				Color col = Color.white;

				Quaternion rot = mod.transform.localRotation;
#if false
				Handles.matrix = Matrix4x4.identity;

				if ( mod.Offset != Vector3.zero )
				{
					Vector3 pos = mod.transform.localToWorldMatrix.MultiplyPoint(-mod.Offset);
					Handles.Label(pos, mod.ModName() + " Offset\n" + mod.Offset.ToString("0.000"));
					col = Color.blue;
					col.a = a;
					Handles.color = col;
					Handles.ArrowCap(0, pos, rot * Quaternion.Euler(180.0f, 0.0f, 0.0f), mod.GizmoSize());
					col = Color.green;
					col.a = a;
					Handles.color = col;
					Handles.ArrowCap(0, pos, rot * Quaternion.Euler(90.0f, 0.0f, 0.0f), mod.GizmoSize());
					col = Color.red;
					col.a = a;
					Handles.color = col;
					Handles.ArrowCap(0, pos, rot * Quaternion.Euler(0.0f, -90.0f, 0.0f), mod.GizmoSize());
				}

				// gizmopos
				if ( mod.gizmoPos != Vector3.zero )
				{
					Vector3 pos = mod.transform.localToWorldMatrix.MultiplyPoint(-mod.gizmoPos);
					Handles.Label(pos, mod.ModName() + " Pos\n" + mod.gizmoPos.ToString("0.000"));
					col = Color.blue;
					col.a = a;
					Handles.color = col;
					Handles.ArrowCap(0, pos, rot * Quaternion.Euler(180.0f, 0.0f, 0.0f), mod.GizmoSize());
					col = Color.green;
					col.a = a;
					Handles.color = col;
					Handles.ArrowCap(0, pos, rot * Quaternion.Euler(90.0f, 0.0f, 0.0f), mod.GizmoSize());
					col = Color.red;
					col.a = a;
					Handles.color = col;
					Handles.ArrowCap(0, pos, rot * Quaternion.Euler(0.0f, -90.0f, 0.0f), mod.GizmoSize());
				}
#else
				Handles.matrix = mod.transform.localToWorldMatrix;	//Matrix4x4.identity;

				if ( mod.Offset != Vector3.zero )
				{
					Vector3 pos = -mod.Offset;	//mod.transform.localToWorldMatrix.MultiplyPoint(-mod.Offset);
					Handles.Label(pos, mod.ModName() + " Offset\n" + mod.Offset.ToString("0.000"));
					col = Color.blue;
					col.a = a;
					Handles.color = col;
					Handles.ArrowCap(0, pos, rot * Quaternion.Euler(180.0f, 0.0f, 0.0f), mod.GizmoSize());
					col = Color.green;
					col.a = a;
					Handles.color = col;
					Handles.ArrowCap(0, pos, rot * Quaternion.Euler(90.0f, 0.0f, 0.0f), mod.GizmoSize());
					col = Color.red;
					col.a = a;
					Handles.color = col;
					Handles.ArrowCap(0, pos, rot * Quaternion.Euler(0.0f, -90.0f, 0.0f), mod.GizmoSize());
				}

				// gizmopos
				if ( mod.gizmoPos != Vector3.zero )
				{
					Vector3 pos = -mod.gizmoPos;	//mod.transform.localToWorldMatrix.MultiplyPoint(-mod.gizmoPos);
					Handles.Label(pos, mod.ModName() + " Pos\n" + mod.gizmoPos.ToString("0.000"));
					col = Color.blue;
					col.a = a;
					Handles.color = col;
					Handles.ArrowCap(0, pos, rot * Quaternion.Euler(180.0f, 0.0f, 0.0f), mod.GizmoSize());
					col = Color.green;
					col.a = a;
					Handles.color = col;
					Handles.ArrowCap(0, pos, rot * Quaternion.Euler(90.0f, 0.0f, 0.0f), mod.GizmoSize());
					col = Color.red;
					col.a = a;
					Handles.color = col;
					Handles.ArrowCap(0, pos, rot * Quaternion.Euler(0.0f, -90.0f, 0.0f), mod.GizmoSize());
				}
#endif
				Handles.matrix = Matrix4x4.identity;
			}
		}
	}

	public override void OnInspectorGUI()
	{
		MegaModifier mod = (MegaModifier)target;

		serializedObject.Update();

		if ( mod.useUndo )
			undoManager.CheckUndo();

		DrawGUI();

		serializedObject.ApplyModifiedProperties();

		if ( GUI.changed )
			EditorUtility.SetDirty(target);

		if ( mod.useUndo )
			undoManager.CheckDirty();
	}

	public void OnSceneGUI()
	{
		DrawSceneGUI();
	}
}