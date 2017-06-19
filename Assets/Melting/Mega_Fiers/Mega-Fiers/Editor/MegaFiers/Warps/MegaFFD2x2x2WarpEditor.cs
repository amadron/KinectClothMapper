
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(MegaFFD2x2x2Warp))]
public class MegaFFD2x2x2WarpEditor : MegaFFDWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/FFD 2x2x2")]
	static void CreateStarShape()
	{
		CreateFFDWarp("FFD 2x2x2", typeof(MegaFFD2x2x2Warp));
	}

	public override string GetHelpString() { return "FFD2x2x2 Warp by Chris West"; }
}