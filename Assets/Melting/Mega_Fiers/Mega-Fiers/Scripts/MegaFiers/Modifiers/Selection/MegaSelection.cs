
using UnityEngine;

// Going to need a clear selection
public class MegaSelectionMod : MegaModifier
{
	public override MegaModChannel ChannelsChanged()	{ return MegaModChannel.Selection; }

	public virtual	void	GetSelection(MegaModifiers mc)	{ }
	public override bool	ModLateUpdate(MegaModContext mc)
	{
		GetSelection(mc.mod);
		return false;		// Dont need to do any mapping
	}

	public override void DrawGizmo(MegaModContext context)
	{
	}

	// TEST this is needed
	public override void DoWork(MegaModifiers mc, int index, int start, int end, int cores)
	{
		for ( int i = start; i < end; i++ )
			sverts[i] = verts[i];
	}
}