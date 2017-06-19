
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaCurveDeform))]
public class MegaCurveDeformEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Mega Curve Deform Modifier by Chris West"; }

	public override bool Inspector()
	{
		MegaCurveDeform mod = (MegaCurveDeform)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif

		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		mod.defCurve = EditorGUILayout.CurveField("Curve", mod.defCurve);
		mod.MaxDeviation = EditorGUILayout.FloatField("Max Deviation", mod.MaxDeviation);

		mod.UsePos = EditorGUILayout.BeginToggleGroup("Use Pos", mod.UsePos);
		mod.Pos = EditorGUILayout.FloatField("Pos", mod.Pos);
		EditorGUILayout.EndToggleGroup();
		return false;
	}
}
