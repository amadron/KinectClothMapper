using UnityEngine;

[AddComponentMenu("Modifiers/DisplaceRT")]
public class MegaDisplaceRT : MegaModifier
{
	public RenderTexture	rtmap;
	public float			amount = 0.0f;
	public Vector2			offset = Vector2.zero;
	public float			vertical = 0.0f;
	public Vector2			scale = Vector2.one;
	public MegaChannel		channel = MegaChannel.Red;
	public bool				CentLum = true;
	public float			CentVal = 0.5f;
	public float			Decay = 0.0f;

	[HideInInspector]
	public Vector2[] uvs;
	[HideInInspector]
	public Vector3[] normals;

	Texture2D	map;
	public override string ModName() { return "DisplaceRT"; }
	public override string GetHelpURL() { return "?page_id=168"; }

	public override MegaModChannel ChannelsReq() { return MegaModChannel.Verts | MegaModChannel.UV; }
	public override MegaModChannel ChannelsChanged() { return MegaModChannel.Verts; }

	[ContextMenu("Init")]
	public virtual void Init()
	{
		MegaModifyObject mod = (MegaModifyObject)GetComponent<MegaModifyObject>();
		uvs = mod.cachedMesh.uv;
		normals = mod.cachedMesh.normals;
	}

	public override void MeshChanged()
	{
		Init();
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		if ( i >= 0 )
		{
			Vector2 uv = Vector2.Scale(uvs[i] + offset, scale);
			Color col = map.GetPixelBilinear(uv.x, uv.y);

			float str = amount;

			if ( Decay != 0.0f )
				str *= (float)Mathf.Exp(-Decay * p.magnitude);

			if ( CentLum )
				str *= (col[(int)channel] + CentVal);
			else
				str *= (col[(int)channel]);

			float of = col[(int)channel] * str;
			p.x += (normals[i].x * of) + (normals[i].x * vertical);
			p.y += (normals[i].y * of) + (normals[i].y * vertical);
			p.z += (normals[i].z * of) + (normals[i].z * vertical);
		}

		return invtm.MultiplyPoint3x4(p);
	}

	public override void Modify(MegaModifiers mc)
	{
		for ( int i = 0; i < verts.Length; i++ )
			sverts[i] = Map(i, verts[i]);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		if ( rtmap == null )
			return false;

		if ( map == null || rtmap.width != map.width || rtmap.height != map.height )
			map = new Texture2D(rtmap.width, rtmap.height);

		if ( uvs == null || uvs.Length == 0 )
			uvs = mc.mod.mesh.uv;

		if ( normals == null || normals.Length == 0 )
		{
			MegaModifyObject mobj = (MegaModifyObject)GetComponent<MegaModifyObject>();
			if ( mobj )
				normals = mobj.cachedMesh.normals;
			else
				normals = mc.mod.mesh.normals;
		}

		if ( uvs.Length == 0 )
			return false;

		if ( normals.Length == 0 )
			return false;

		if ( map == null )
			return false;

		RenderTexture.active = rtmap;

		map.ReadPixels(new Rect(0, 0, rtmap.width, rtmap.height), 0, 0);
		return true;
	}
}