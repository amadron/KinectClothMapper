
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaVertNoise))]
public class MegaVertNoiseEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Vertical Noise Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\noise_help.png"); }

	public override bool Inspector()
	{
		MegaVertNoise mod = (MegaVertNoise)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		mod.Scale = EditorGUILayout.FloatField("Scale", mod.Scale);
		mod.Freq = EditorGUILayout.FloatField("Freq", mod.Freq);
		mod.Phase = EditorGUILayout.FloatField("Phase", mod.Phase);
		mod.decay = EditorGUILayout.FloatField("Decay", mod.decay);
		mod.Strength = EditorGUILayout.FloatField("Strength", mod.Strength);
		mod.Animate = EditorGUILayout.Toggle("Animate", mod.Animate);
		mod.Fractal = EditorGUILayout.Toggle("Fractal", mod.Fractal);
		if ( mod.Fractal )
		{
			mod.Iterations = EditorGUILayout.FloatField("Iterations", mod.Iterations);
			mod.Rough = EditorGUILayout.FloatField("Rough", mod.Rough);
		}

		return false;
	}
}