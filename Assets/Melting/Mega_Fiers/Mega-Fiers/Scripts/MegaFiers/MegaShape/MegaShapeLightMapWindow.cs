
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Do this with icons
public class MegaShapeLightMapWindow : EditorWindow
{
	static public void Init()
	{
		MegaShapeLightMapWindow window = ScriptableObject.CreateInstance<MegaShapeLightMapWindow>();
		window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
		window.ShowUtility();
	}

	void OnGUI()
	{
		if ( Selection.activeGameObject == null )
			return;

		MegaShape shape = Selection.activeGameObject.GetComponent<MegaShape>();
		if ( shape == null )
			return;

		//UnwrapParam uv1 = new UnwrapParam();
		//UnwrapParam.SetDefaults(out uv1);

		//loft.genLightMap = EditorGUILayout.BeginToggleGroup("Gen LightMap", loft.genLightMap);
		shape.angleError = EditorGUILayout.Slider("Angle Error", shape.angleError, 0.0f, 1.0f);
		shape.areaError = EditorGUILayout.Slider("Area Error", shape.areaError, 0.0f, 1.0f);
		shape.hardAngle = EditorGUILayout.FloatField("Hard Angle", shape.hardAngle);
		shape.packMargin = EditorGUILayout.FloatField("Pack Margin", shape.packMargin);

		EditorStyles.textField.wordWrap = false;

		EditorGUILayout.BeginHorizontal();
		if ( GUILayout.Button("Build") )
		{
			UnwrapParam uv = new UnwrapParam();
			//UnwrapParam.SetDefaults(out uv);
			uv.angleError = shape.angleError;
			uv.areaError = shape.areaError;
			uv.hardAngle = shape.hardAngle;
			uv.packMargin = shape.packMargin;

			Unwrapping.GenerateSecondaryUVSet(shape.shapemesh, uv);

			this.Close();
		}

		if ( GUILayout.Button("Cancel") )
		{
			this.Close();
		}
		EditorGUILayout.EndHorizontal();
	}
}
#endif
