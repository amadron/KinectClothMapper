
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaHump))]
public class MegaHumpEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Hump Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\hump_help.png"); }

	public override bool Inspector()
	{
		MegaHump mod = (MegaHump)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		mod.amount = EditorGUILayout.FloatField("Amount", mod.amount);
		mod.cycles = EditorGUILayout.FloatField("Cycles", mod.cycles);
		mod.phase = EditorGUILayout.FloatField("Phase", mod.phase);
		mod.animate = EditorGUILayout.Toggle("Animate", mod.animate);
		mod.speed = EditorGUILayout.FloatField("Speed", mod.speed);
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		return false;
	}
}