
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaZStretchWarp))]
public class MegaZStretchWarpEditor : MegaWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/ZStretch")]
	static void CreateStarShape() { CreateWarp("ZStretch", typeof(MegaZStretchWarp)); }

	public override string GetHelpString() { return "ZStretch Warp Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\stretch_help.png"); }

	public override bool Inspector()
	{
		MegaZStretchWarp mod = (MegaZStretchWarp)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		mod.amount = EditorGUILayout.FloatField("Amount", mod.amount);
		mod.amplify = EditorGUILayout.FloatField("Amplify", mod.amplify);
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		mod.doRegion = EditorGUILayout.Toggle("Do Region", mod.doRegion);
		mod.from = EditorGUILayout.FloatField("From", mod.from);
		mod.to = EditorGUILayout.FloatField("To", mod.to);
		return false;
	}
}