using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControl : MonoBehaviour {
    bool manipulate;
    Vector3 lastPos;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    /*
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with : " + other.name);
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Thump Left");
    }
    */
    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Thumb Still colliding");
    }

}
