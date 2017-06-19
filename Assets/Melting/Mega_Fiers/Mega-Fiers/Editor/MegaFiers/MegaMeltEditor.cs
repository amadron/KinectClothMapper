
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaMelt))]
public class MegaMeltEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Melt Modifier by Chris West"; }
	//public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	SerializedProperty	amountProp;
	SerializedProperty	spreadProp;
	SerializedProperty	materialtypeProp;
	SerializedProperty	solidityProp;
	SerializedProperty	axisProp;
	SerializedProperty	flipaxisProp;
	SerializedProperty	flatnessProp;


	public override void Enable()
	{
		amountProp = serializedObject.FindProperty("Amount");
		spreadProp = serializedObject.FindProperty("Spread");
		materialtypeProp = serializedObject.FindProperty("MaterialType");
		solidityProp = serializedObject.FindProperty("Solidity");
		axisProp = serializedObject.FindProperty("axis");
		flipaxisProp = serializedObject.FindProperty("FlipAxis");
		flatnessProp = serializedObject.FindProperty("flatness");
	}

	public override bool Inspector()
	{
		//MegaMelt mod = (MegaMelt)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		//serializedObject.Update();

		EditorGUILayout.PropertyField(amountProp, new GUIContent("Amount"));	//FloatField("Amount", mod.Amount);
		EditorGUILayout.PropertyField(spreadProp, new GUIContent("Spread"));
		EditorGUILayout.PropertyField(materialtypeProp, new GUIContent("Material Type"));
		EditorGUILayout.PropertyField(solidityProp, new GUIContent("Solidity"));
		EditorGUILayout.PropertyField(axisProp, new GUIContent("Axis"));
		EditorGUILayout.PropertyField(flipaxisProp, new GUIContent("Flip Axis"));
		EditorGUILayout.Slider(flatnessProp, 0.0f, 1.0f, new GUIContent("Flatness"));

		//mod.Amount = EditorGUILayout.FloatField("Amount", mod.Amount);
		//mod.Spread = EditorGUILayout.FloatField("Spread", mod.Spread);
		//mod.MaterialType = (MegaMeltMat)EditorGUILayout.EnumPopup("Material Type", mod.MaterialType);
		//mod.Solidity = EditorGUILayout.FloatField("Solidity", mod.Solidity);
		//mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		//mod.FlipAxis = EditorGUILayout.Toggle("Flip Axis", mod.FlipAxis);
		//mod.flatness = EditorGUILayout.Slider("Flatness", mod.flatness, 0.0f, 1.0f);

		//serializedObject.ApplyModifiedProperties();
		return false;
	}
}