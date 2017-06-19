
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(MegaWorldPathDeform))]
public class MegaWorldPathDeformEditor : MegaModifierEditor
{
	//void OnSceneGUI()
	//{
	//PathDeform pd = (PathDeform)target;
	//Display(pd);
	//}

	public override bool Inspector()
	{
		MegaWorldPathDeform mod = (MegaWorldPathDeform)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif
		mod.usedist = EditorGUILayout.Toggle("Use Distance", mod.usedist);

		if ( mod.usedist )
			mod.distance = EditorGUILayout.FloatField("Distance", mod.distance);
		else
			mod.percent = EditorGUILayout.FloatField("Percent", mod.percent);

		mod.stretch = EditorGUILayout.FloatField("Stretch", mod.stretch);
		mod.twist = EditorGUILayout.FloatField("Twist", mod.twist);
		mod.rotate = EditorGUILayout.FloatField("Rotate", mod.rotate);
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		mod.flip = EditorGUILayout.Toggle("Flip", mod.flip);

		mod.path = (MegaShape)EditorGUILayout.ObjectField("Path", mod.path, typeof(MegaShape), true);
		if ( mod.path != null && mod.path.splines.Count > 1 )
		{
			//shape.selcurve = EditorGUILayout.IntField("Curve", shape.selcurve);
			mod.curve = EditorGUILayout.IntSlider("Curve", mod.curve, 0, mod.path.splines.Count - 1);
			if ( mod.curve < 0 )
				mod.curve = 0;

			if ( mod.curve > mod.path.splines.Count - 1 )
				mod.curve = mod.path.splines.Count - 1;
		}

		mod.animate = EditorGUILayout.Toggle("Animate", mod.animate);
		mod.speed = EditorGUILayout.FloatField("Speed", mod.speed);
		mod.tangent = EditorGUILayout.FloatField("Tangent", mod.tangent);
		mod.UseTwistCurve = EditorGUILayout.Toggle("Use Twist Curve", mod.UseTwistCurve);
		mod.twistCurve = EditorGUILayout.CurveField("Twist Curve", mod.twistCurve);
		mod.UseStretchCurve = EditorGUILayout.Toggle("Use Stretch Curve", mod.UseStretchCurve);
		mod.stretchCurve = EditorGUILayout.CurveField("Stretch Curve", mod.stretchCurve);

		mod.Up = EditorGUILayout.Vector3Field("Up", mod.Up);
		return false;
	}
#if false
	void Display(PathDeform pd)
	{
		if ( pd.path != null )
		{
			Matrix4x4 mat = pd.transform.localToWorldMatrix * pd.path.transform.localToWorldMatrix * pd.mat;

			for ( int s = 0; s < pd.path.splines.Count; s++ )
			{
				float ldist = pd.path.stepdist;
				if ( ldist < 0.1f )
					ldist = 0.1f;

				float ds = pd.path.splines[s].length / (pd.path.splines[s].length / ldist);

				int c		= 0;
				int k		= -1;
				int lk	= -1;

				Vector3 first = pd.path.splines[s].Interpolate(0.0f, pd.path.normalizedInterp, ref lk);

				for ( float dist = ds; dist < pd.path.splines[s].length; dist += ds )
				{
					float alpha = dist / pd.path.splines[s].length;
					Vector3 pos = pd.path.splines[s].Interpolate(alpha, pd.path.normalizedInterp, ref k);

					if ( k != lk )
					{
						for ( lk = lk + 1; lk <= k; lk++ )
						{
							Handles.DrawLine(mat.MultiplyPoint(first), mat.MultiplyPoint(pd.path.splines[s].knots[lk].p));
							first = pd.path.splines[s].knots[lk].p;
						}
					}

					lk = k;

					Handles.DrawLine(mat.MultiplyPoint(first), mat.MultiplyPoint(pos));

					c++;

					first = pos;
				}

				if ( pd.path.splines[s].closed )
				{
					Vector3 pos = pd.path.splines[s].Interpolate(0.0f, pd.path.normalizedInterp, ref k);

					Handles.DrawLine(mat.MultiplyPoint(first), mat.MultiplyPoint(pos));
				}
			}
		}
	}
#endif
}