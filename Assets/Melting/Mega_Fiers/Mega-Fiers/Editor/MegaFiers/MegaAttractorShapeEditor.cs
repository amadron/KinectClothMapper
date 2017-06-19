
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaAttractorShape))]
public class MegaAttractorShapeEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Spline Attractor Modifier by Chris West"; }

	public override bool Inspector()
	{
		MegaAttractorShape mod = (MegaAttractorShape)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif

		mod.shape = (MegaShape)EditorGUILayout.ObjectField("Shape", mod.shape, typeof(MegaShape), true);
		if ( mod.shape != null && mod.shape.splines.Count > 1 )
		{
			mod.curve = EditorGUILayout.IntSlider("Curve", mod.curve, 0, mod.shape.splines.Count - 1);
			if ( mod.curve < 0 )
				mod.curve = 0;

			if ( mod.curve > mod.shape.splines.Count - 1 )
				mod.curve = mod.shape.splines.Count - 1;
		}

		mod.itercount = EditorGUILayout.IntSlider("Iter Count", mod.itercount, 1, 5);
		mod.attractType = (MegaAttractType)EditorGUILayout.EnumPopup("Type", mod.attractType);
		mod.limit = EditorGUILayout.FloatField("Limit", mod.limit);
		mod.distance = EditorGUILayout.FloatField("Distance", mod.distance);
		if ( mod.distance < 0.0f )
			mod.distance = 0.0f;

		if ( mod.attractType != MegaAttractType.Rotate )
			mod.force = EditorGUILayout.FloatField("Force", mod.force);
		else
		{
			mod.rotate = EditorGUILayout.FloatField("Rotate", mod.rotate);
			mod.slide = EditorGUILayout.FloatField("Slide", mod.slide);
		}
		mod.crv = EditorGUILayout.CurveField("Influence Curve", mod.crv);

		mod.splinechanged = EditorGUILayout.Toggle("Spline Changed", mod.splinechanged);
		mod.flat = EditorGUILayout.Toggle("Mesh is Flat", mod.flat);

		return false;
	}
}
