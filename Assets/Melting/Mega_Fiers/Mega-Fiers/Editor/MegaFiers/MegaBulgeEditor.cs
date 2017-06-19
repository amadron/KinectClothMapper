
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaBulge))]
public class MegaBulgeEditor : MegaModifierEditor
{

	public override bool Inspector()
	{
		MegaBulge mod = (MegaBulge)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		mod.Amount = EditorGUILayout.Vector3Field("Radius", mod.Amount);
		mod.FallOff = EditorGUILayout.Vector3Field("Falloff", mod.FallOff);
		mod.LinkFallOff = EditorGUILayout.Toggle("Link Falloff", mod.LinkFallOff);
		return false;
	}
}
