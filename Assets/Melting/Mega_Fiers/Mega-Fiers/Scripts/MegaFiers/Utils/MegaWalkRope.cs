using UnityEngine;

[ExecuteInEditMode]
public class MegaWalkRope : MonoBehaviour
{
	public GameObject bridge;
	[HideInInspector]
	public MegaRopeDeform mod;
	public float offset = 0.0f;	// Character offset
	public bool checkonbridge = false;
	public float weight = 1.0f;
	Mesh mesh;

	void LateUpdate()
	{
		if ( bridge )
		{
			// Get the bridge modifier
			if ( mod == null )
				mod = bridge.GetComponent<MegaRopeDeform>();

			if ( mesh == null )
			{
				MeshFilter mf = bridge.GetComponent<MeshFilter>();
				mesh = mf.sharedMesh;
			}

			if ( mod && mesh )
			{
				int ax = (int)mod.axis;
				Vector3 pos = transform.position;

				// Get into local space
				Vector3 lpos = mod.transform.worldToLocalMatrix.MultiplyPoint(pos);

				bool onbridge = true;
				if ( checkonbridge )
				{
					if ( lpos.x > mesh.bounds.min.x && lpos.x < mesh.bounds.max.x && lpos.z > mesh.bounds.min.z && lpos.z < mesh.bounds.max.z )
						onbridge = true;
					else
						onbridge = false;
				}

				// Are we on the bridge
				if ( onbridge )
				{
					// How far across are we
					float alpha = (lpos[ax] - mod.soft.masses[0].pos.x) / (mod.soft.masses[mod.soft.masses.Count - 1].pos.x - mod.soft.masses[0].pos.x);

					if ( alpha > 0.0f || alpha < 1.0f )
					{
						Vector2 rpos = mod.SetWeight(lpos[ax], weight);

						lpos.y = rpos.y + (offset * 0.01f);	// 0.01 is just to make inspector easier to control in my test scene which is obvioulsy very small
						transform.position = bridge.transform.localToWorldMatrix.MultiplyPoint(lpos);
					}
				}
				else
				{
					SetPos(mod, 0.0f);
				}
			}
		}
	}

	public void SetPos(MegaRopeDeform mod, float alpha)
	{
		mod.weightPos = alpha * 100.0f;
		mod.weight = weight;
	}
}