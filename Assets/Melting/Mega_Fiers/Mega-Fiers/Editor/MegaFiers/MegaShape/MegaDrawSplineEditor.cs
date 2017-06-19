
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaDrawSpline))]
public class MegaDrawSplineEditor : Editor
{
	public override void OnInspectorGUI()
	{
		MegaDrawSpline mod = (MegaDrawSpline)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif

		mod.updatedist = Mathf.Clamp(EditorGUILayout.FloatField("Update Dist", mod.updatedist), 0.02f, 100.0f);
		mod.smooth = EditorGUILayout.Slider("Smooth", mod.smooth, 0.0f, 1.5f);
		mod.offset = EditorGUILayout.FloatField("Offset", mod.offset);
		mod.radius = EditorGUILayout.FloatField("Gizmo Radius", mod.radius);
		mod.meshstep = EditorGUILayout.FloatField("Mesh Step", mod.meshstep);
		mod.meshtype = (MeshShapeType)EditorGUILayout.EnumPopup("Mesh Type", mod.meshtype);
		mod.width = EditorGUILayout.FloatField("Width", mod.width);
		mod.height = EditorGUILayout.FloatField("Height", mod.height);
		mod.tradius = EditorGUILayout.FloatField("Tube Radius", mod.tradius);
		mod.mat = (Material)EditorGUILayout.ObjectField("Material", mod.mat, typeof(Material), true);
		mod.closed = EditorGUILayout.Toggle("Build Closed", mod.closed);
		mod.closevalue = EditorGUILayout.Slider("Close Value", mod.closevalue, 0.0f, 1.0f);
		mod.constantspd = EditorGUILayout.Toggle("Constant Speed", mod.constantspd);

		if ( GUI.changed )
			EditorUtility.SetDirty(mod);
	}
}
