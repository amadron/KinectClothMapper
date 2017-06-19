
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaPush))]
public class MegaPushEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Push Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\push_help.png"); }

	public override bool Inspector()
	{
		MegaPush mod = (MegaPush)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		mod.amount = EditorGUILayout.FloatField("Amount", mod.amount);
		mod.method = (MegaNormType)EditorGUILayout.EnumPopup("Method", mod.method);
		return false;
	}
}