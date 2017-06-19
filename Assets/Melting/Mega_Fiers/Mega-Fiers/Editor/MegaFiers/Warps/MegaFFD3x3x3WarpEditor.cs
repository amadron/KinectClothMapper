

using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(MegaFFD3x3x3Warp))]
public class MegaFFD3x3x3WarpEditor : MegaFFDWarpEditor
{
	[MenuItem("GameObject/Create Other/MegaFiers/Warps/FFD 3x3x3")]
	static void CreateStarShape()
	{
		CreateFFDWarp("FFD 3x3x3", typeof(MegaFFD3x3x3Warp));
	}

	public override string GetHelpString() { return "FFD3x3x3 Warp by Chris West"; }
}