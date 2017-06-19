
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaCrumple))]
public class MegaCrumpleEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Crumple Modifier by Unity"; }

	public override bool Inspector()
	{
		MegaCrumple mod = (MegaCrumple)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		mod.scale = EditorGUILayout.FloatField("Scale", mod.scale);
		mod.speed = EditorGUILayout.FloatField("Speed", mod.speed);
		mod.phase = EditorGUILayout.FloatField("Phase", mod.phase);
		mod.animate = EditorGUILayout.Toggle("Animate", mod.animate);
		return false;
	}
}
