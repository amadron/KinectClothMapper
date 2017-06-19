
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MegaPivotAdjust))]
public class MegaPivotAdjustEditor : MegaModifierEditor
{
	public override bool Inspector()
	{
		//MegaPivotAdjust mod = (MegaPivotAdjust)target;
		return false;
	}
}
