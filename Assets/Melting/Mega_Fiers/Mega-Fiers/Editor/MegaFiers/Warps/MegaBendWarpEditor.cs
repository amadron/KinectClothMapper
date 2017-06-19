
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaBendWarp))]
public class MegaBendWarpEditor : MegaWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/Bend")]
	static void CreateStarShape() { CreateWarp("Bend", typeof(MegaBendWarp)); }

	public override string GetHelpString() { return "Bend Warp Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	public override bool Inspector()
	{
		MegaBendWarp mod = (MegaBendWarp)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		mod.angle = EditorGUILayout.FloatField("Angle", mod.angle);
		mod.dir			= EditorGUILayout.FloatField("Dir", mod.dir);
		mod.axis		= (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		mod.doRegion	= EditorGUILayout.Toggle("Do Region", mod.doRegion);
		mod.from		= EditorGUILayout.FloatField("From", mod.from);
		mod.to			= EditorGUILayout.FloatField("To", mod.to);
		return false;
	}
}
