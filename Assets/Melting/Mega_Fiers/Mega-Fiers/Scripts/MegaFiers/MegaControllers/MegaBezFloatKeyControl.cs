
using UnityEngine;
using System.IO;

[System.Serializable]
public class MegaBezFloatKey
{
	public float	val;
	public float	intan;
	public float	outtan;
	public float	intanx;
	public float	outtanx;
	public float	coef0;
	public float	coef1;
	public float	coef2;
	public float	coef3;
}

[System.Serializable]
public class MegaBezFloatKeyControl : MegaControl
{
	public MegaBezFloatKey[]	Keys;
	private const float SCALE = 4800.0f;

	public float	f;

	public void InitKeys()
	{
		for ( int i = 0; i < Keys.Length - 1; i++ )
		{
			float dt	= Times[i + 1] - Times[i];
			float hout	= Keys[i].val + (Keys[i].outtan * SCALE) * (dt / 3.0f);
			float hin	= Keys[i + 1].val + (Keys[i + 1].intan * SCALE) * (dt / 3.0f);

			Keys[i].coef1 = Keys[i + 1].val + 3.0f * (hout - hin) - Keys[i].val;
			Keys[i].coef2 = 3.0f * (hin - 2.0f * hout + Keys[i].val);
			Keys[i].coef3 = 3.0f * (hout - Keys[i].val);
		}
	}

	public void InitKeys(float scale)
	{
		for ( int i = 0; i < Keys.Length - 1; i++ )
		{
			float dt	= Times[i + 1] - Times[i];
			float hout	= Keys[i].val + (Keys[i].outtan * scale) * (dt / 3.0f);
			float hin	= Keys[i + 1].val + (Keys[i + 1].intan * scale) * (dt / 3.0f);

			Keys[i].coef1 = Keys[i + 1].val + 3.0f * (hout - hin) - Keys[i].val;
			Keys[i].coef2 = 3.0f * (hin - 2.0f * hout + Keys[i].val);
			Keys[i].coef3 = 3.0f * (hout - Keys[i].val);
		}
	}

	public void InitKeysMaya()
	{
		for ( int i = 0; i < Keys.Length - 1; i++ )
		{
			float x0 = Times[i];
			float x1 = Times[i] + Keys[i].outtanx;
			float x2 = Times[i + 1] - Keys[i + 1].intanx;
			float x3 = Times[i + 1];

			float y0 = Keys[i].val;
			float y1 = Keys[i].val + Keys[i].outtan;
			float y2 = Keys[i + 1].val - Keys[i + 1].intan;
			float y3 = Keys[i + 1].val;

			float dx = x3 - x0;
			float dy = y3 - y0;

			float tan_x = x1 - x0;
			float m1 = 0.0f;
			float m2 = 0.0f;
			if ( tan_x != 0.0f )
				m1 = (y1 - y0) / tan_x;

			tan_x = x3 - x2;
			if ( tan_x != 0.0f )
				m2 = (y3 - y2) / tan_x;

			float length = 1.0f / (dx * dx);
			float d1 = dx * m1;
			float d2 = dx * m2;
			Keys[i].coef0 = (d1 + d2 - dy - dy) * length / dx;
			Keys[i].coef1 = (dy + dy + dy - d1 - d1 - d2) * length;
			Keys[i].coef2 = m1;
			Keys[i].coef3 = y0;
		}
	}

	public float GetHermiteFloat(float tt)
	{
		if ( Times.Length == 1 )
			return Keys[0].val;

		int key = GetKey(tt);

		float t = Mathf.Clamp01((tt - Times[key]) / (Times[key + 1] - Times[key]));

		t = Mathf.Lerp(Times[key], Times[key + 1], t) - Times[key];
		return (t * (t * (t * Keys[key].coef0 + Keys[key].coef1) + Keys[key].coef2) + Keys[key].coef3);
	}

	public void MakeKey(MegaBezFloatKey key, Vector2 pco, Vector2 pleft, Vector2 pright, Vector2 co, Vector2 left, Vector2 right)
	{
		float f1 = pco.y * 100.0f;
		float f2 = pright.y * 100.0f;
		float f3 = left.y * 100.0f;
		float f4 = co.y * 100.0f;

		key.val = f1;
		key.coef3 = 3.0f * (f2 - f1);
		key.coef2 = 3.0f * (f1 - 2.0f * f2 + f3);
		key.coef1 = f4 - f1 + 3.0f * (f2 - f3);
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

				f = Keys[key].coef1 * tp3 + Keys[key].coef2 * tp2 + Keys[key].coef3 * alpha + Keys[key].val;
			}
		}
	}

	public override float GetFloat(float t)
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