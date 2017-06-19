using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaSimpleMod))]
public class MegaSimpleEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Simple Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	public override bool Inspector()
	{
		MegaSimpleMod mod = (MegaSimpleMod)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		mod.a3 = EditorGUILayout.Vector3Field("A3", mod.a3);
		return false;
	}
}