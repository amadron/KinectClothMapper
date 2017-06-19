using UnityEngine;

public class MegaToggleMultiCore : MonoBehaviour
{
	bool Enabled = false;	//true;

	void Start()
	{
		MegaModifiers.ThreadingOn = Enabled;
	}

	void Update()
	{
		if ( Input.GetKeyDown(KeyCode.T) )
		{
			Enabled = !Enabled;
			MegaModifiers.ThreadingOn = Enabled;
		}
	}
}
