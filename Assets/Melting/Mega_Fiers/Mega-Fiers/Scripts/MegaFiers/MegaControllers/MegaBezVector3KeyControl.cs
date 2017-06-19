
using UnityEngine;
using System.IO;

[System.Serializable]
public class MegaBezVector3Key	// Should be derived from Key
{
	public Vector3	val;
	public Vector3	intan;
	public Vector3	outtan;
	public Vector3	coef1;
	public Vector3	coef2;
	public Vector3	coef3;
}

[System.Serializable]
public class MegaBezVector3KeyControl : MegaControl
{
	public MegaBezVector3Key[]	Keys;
	private const float SCALE = 4800.0f;
	public Vector3	f;

	public void Scale(float scl)
	{
		for ( int i = 0; i < Keys.Length; i++ )
		{
			Keys[i].val *= scl;
			Keys[i].intan *= scl;
			Keys[i].outtan *= scl;
		}

		InitKeys();
	}

	public void Scale(Vector3 scl)
	{
		for ( int i = 0; i < Keys.Length; i++ )
		{
			Keys[i].val.x *= scl.x;
			Keys[i].val.y *= scl.y;
			Keys[i].val.z *= scl.z;

			Keys[i].intan.x *= scl.x;
			Keys[i].intan.y *= scl.y;
			Keys[i].intan.z *= scl.z;

			Keys[i].outtan.x *= scl.x;
			Keys[i].outtan.y *= scl.y;
			Keys[i].outtan.z *= scl.z;
		}

		InitKeys();
	}

	public void Move(Vector3 scl)
	{
		for ( int i = 0; i < Keys.Length; i++ )
		{
			Keys[i].val += scl;
			Keys[i].intan += scl;
			Keys[i].outtan += scl;
		}

		InitKeys();
	}

	public void Rotate(Matrix4x4 tm)
	{
		for ( int i = 0; i < Keys.Length; i++ )
		{
			Keys[i].val = tm.MultiplyPoint3x4(Keys[i].val);
			Keys[i].intan = tm.MultiplyPoint3x4(Keys[i].intan);
			Keys[i].outtan = tm.MultiplyPoint3x4(Keys[i].outtan);
		}

		InitKeys();
	}

	public void InitKeys()
	{
		for ( int i = 0; i < Keys.Length - 1; i++ )
		{
			float dt		= Times[i + 1] - Times[i];
			Vector3 hout	= Keys[i].val + (Keys[i].outtan * SCALE) * (dt / 3.0f);
			Vector3 hin		= Keys[i + 1].val + (Keys[i + 1].intan * SCALE) * (dt / 3.0f);

			Keys[i].coef1 = Keys[i + 1].val + 3.0f * (hout - hin) - Keys[i].val;
			Keys[i].coef2 = 3.0f * (hin - 2.0f * hout + Keys[i].val);
			Keys[i].coef3 = 3.0f * (hout - Keys[i].val);
		}
	}

	public void Interp(float alpha, int key)
	{
		if ( alpha == 0.0f )
			f = Keys[key].val;
		else
		{
			if ( alpha == 1.0f )
				f = Keys[key + 1].val;
			else
			{
				float tp2 = alpha * alpha;
				float tp3 = tp2 * alpha;

				f.x = Keys[key].coef1.x * tp3 + Keys[key].coef2.x * tp2 + Keys[key].coef3.x * alpha + Keys[key].val.x;
				f.y = Keys[key].coef1.y * tp3 + Keys[key].coef2.y * tp2 + Keys[key].coef3.y * alpha + Keys[key].val.y;
				f.z = Keys[key].coef1.z * tp3 + Keys[key].coef2.z * tp2 + Keys[key].coef3.z * alpha + Keys[key].val.z;

			}
		}
	}

	public override Vector3 GetVector3(float t)
	{
		if ( Times.Length == 1 )
		{
			return Keys[0].val;
		}
		int key = GetKey(t);

		float alpha = (t - Times[key]) / (Times[key + 1] - Times[key]);

		if ( alpha < 0.0f )
			alpha = 0.0f;
		else
		{
			if ( alpha > 1.0f )
				alpha = 1.0f;
		}

		// Do ease and hermite here maybe
		Interp(alpha, key);

		lastkey = key;
		lasttime = t;

		return f;
	}
}
