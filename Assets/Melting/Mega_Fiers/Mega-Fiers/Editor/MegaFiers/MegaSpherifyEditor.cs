
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaSpherify))]
public class MegaSpherifyEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Spherify Modifier by Chris West"; }
	//public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("bend_help.png"); }

	public override bool Inspector()
	{
		MegaSpherify mod = (MegaSpherify)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		mod.percent = EditorGUILayout.FloatField("Percent", mod.percent);
		mod.FallOff = EditorGUILayout.FloatField("FallOff", mod.FallOff);
		return false;
	}
}