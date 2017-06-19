
using UnityEngine;

public class ToggleMultiCore : MonoBehaviour
{
	bool Enabled = false;	//true;

	void Start()
	{
		//Application.targetFrameRate = 60;
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
