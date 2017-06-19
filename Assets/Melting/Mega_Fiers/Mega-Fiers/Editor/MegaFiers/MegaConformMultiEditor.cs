

using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaConformMod))]
public class MegaConformMultiEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Multi Conform Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	public override bool DisplayCommon()
	{
		return false;
	}

	public override bool Inspector()
	{
		MegaConformMulti mod = (MegaConformMulti)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		CommonModParamsBasic(mod);

		mod.conformAmount = EditorGUILayout.Slider("Conform Amount", mod.conformAmount, 0.0f, 1.0f);
		mod.raystartoff = EditorGUILayout.FloatField("Ray Start Off", mod.raystartoff);
		mod.raydist = EditorGUILayout.FloatField("Ray Dist", mod.raydist);
		mod.offset = EditorGUILayout.FloatField("Offset", mod.offset);
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);

		if ( GUILayout.Button("Add Target") )
		{
			MegaConformTarget targ = new MegaConformTarget();
			mod.targets.Add(targ);
			GUI.changed = true;
		}

		for ( int i = 0; i < mod.targets.Count; i++ )
		{
			mod.targets[i].target = (GameObject)EditorGUILayout.ObjectField("Object", mod.targets[i].target, typeof(GameObject), true);

			mod.targets[i].children = EditorGUILayout.Toggle("Include Children", mod.targets[i].children);

			if ( GUILayout.Button("Delete") )
			{
				mod.targets.RemoveAt(i);
				GUI.changed = true;
			}
		}

		if ( GUI.changed )
		{
			mod.BuildColliderList();
		}

		return false;
	}
}