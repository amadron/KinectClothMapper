using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaMatSelect))]
public class MegaMatSelectEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Material Select Modifier by Chris West"; }
	//public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	public override bool DisplayCommon() { return false; }

	public override bool Inspector()
	{
		MegaMatSelect mod = (MegaMatSelect)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		mod.Label = EditorGUILayout.TextField("Label", mod.Label);
		mod.MaxLOD = EditorGUILayout.IntField("MaxLOD", mod.MaxLOD);
		mod.ModEnabled = EditorGUILayout.Toggle("Enabled", mod.ModEnabled);
		mod.Order = EditorGUILayout.IntField("Order", mod.Order);
		mod.weight = EditorGUILayout.FloatField("Weight", mod.weight);
		mod.otherweight = EditorGUILayout.FloatField("Other Weight", mod.otherweight);
		mod.matnum = EditorGUILayout.IntField("Material Num", mod.matnum);

		mod.displayWeights = EditorGUILayout.Toggle("Show Weights", mod.displayWeights);
		//mod.gizCol = EditorGUILayout.ColorField("Gizmo Col", mod.gizCol);
		mod.gizSize = EditorGUILayout.FloatField("Gizmo Size", mod.gizSize);

		if ( GUI.changed )
		{
			mod.update = true;
		}

		return false;
	}

	public override void DrawSceneGUI()
	{
		MegaMatSelect mod = (MegaMatSelect)target;

		MegaModifiers mc = mod.gameObject.GetComponent<MegaModifiers>();

		float[] sel = mod.GetSel();

		if ( mc != null && sel != null )
		{
			Color col = Color.black;

			Matrix4x4 tm = mod.gameObject.transform.localToWorldMatrix;
			Handles.matrix = Matrix4x4.identity;

			if ( mod.displayWeights )
			{
				for ( int i = 0; i < sel.Length; i++ )
				{
					float w = sel[i];
					if ( w > 0.5f )
						col = Color.Lerp(Color.green, Color.red, (w - 0.5f) * 2.0f);
					else
						col = Color.Lerp(Color.blue, Color.green, w * 2.0f);
					Handles.color = col;

					Vector3 p = tm.MultiplyPoint(mc.sverts[i]);

					if ( w > 0.001f )
						Handles.DotCap(i, p, Quaternion.identity, mod.gizSize);
				}
			}
		}
	}
}