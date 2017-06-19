
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaDisplace))]
public class MegaDisplaceEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Displace Modifier by Chris West"; }
	//public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_web.png"); }

	public override bool Inspector()
	{
		MegaDisplace mod = (MegaDisplace)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		mod.map = (Texture2D)EditorGUILayout.ObjectField("Map", mod.map, typeof(Texture2D), true);
		mod.amount = EditorGUILayout.FloatField("Amount", mod.amount);
		mod.offset = EditorGUILayout.Vector2Field("Offset", mod.offset);
		mod.vertical = EditorGUILayout.FloatField("Vertical", mod.vertical);
		mod.scale = EditorGUILayout.Vector2Field("Scale", mod.scale);
		mod.channel = (MegaChannel)EditorGUILayout.EnumPopup("Channel", mod.channel);
		mod.CentLum = EditorGUILayout.Toggle("Cent Lum", mod.CentLum);
		mod.CentVal = EditorGUILayout.FloatField("Cent Val", mod.CentVal);
		mod.Decay = EditorGUILayout.FloatField("Decay", mod.Decay);
		return false;
	}
}