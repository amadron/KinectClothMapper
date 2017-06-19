
using UnityEngine;

[AddComponentMenu("Modifiers/Cylindrify")]
public class MegaCylindrify : MegaModifier
{
	public float Percent = 0.0f;
	public float Decay = 0.0f;

	public override string ModName() { return "Cylindrify"; }
	public override string GetHelpURL() { return "?page_id=166"; }

	float size;
	float per;

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		float dcy = Mathf.Exp(-Decay * p.magnitude);

		float k = ((size / Mathf.Sqrt(p.x * p.x + p.z * p.z) / 2.0f - 1.0f) * per * dcy) + 1.0f;
		p.x *= k;
		p.z *= k;
		return invtm.MultiplyPoint3x4(p);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public void SetTM1()
	{
		tm = Matrix4x4.identity;

		MegaMatrix.RotateZ(ref tm, -gizmoRot.z * Mathf.Deg2Rad);
		MegaMatrix.RotateY(ref tm, -gizmoRot.y * Mathf.Deg2Rad);
		MegaMatrix.RotateX(ref tm, -gizmoRot.x * Mathf.Deg2Rad);

		MegaMatrix.SetTrans(ref tm, gizmoPos + Offset);

		//tm.SetTRS(gizmoPos + Offset, rot, gizmoScale);
		invtm = tm.inverse;
	}

	public MegaAxis axis;
	Matrix4x4		mat = new Matrix4x4();

	public override bool Prepare(MegaModContext mc)
	{
		mat = Matrix4x4.identity;

		switch ( axis )
		{
			case MegaAxis.X: MegaMatrix.RotateZ(ref mat, Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref mat, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		SetAxis(mat);

		float xsize = bbox.max.x - bbox.min.x;
		float zsize = bbox.max.z - bbox.min.z;
		size = (xsize > zsize) ? xsize : zsize;

		// Get the percentage to spherify at this time
		per = Percent / 100.0f;

		return true;
	}
}
