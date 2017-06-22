using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSwitch : MonoBehaviour {
    public MegaPointCache pla;
	// Use this for initialization
	void Awake () {
        pla = transform.parent.gameObject.GetComponentInChildren<MegaPointCache>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (pla != null)
            pla.animated = true;
    }
}
