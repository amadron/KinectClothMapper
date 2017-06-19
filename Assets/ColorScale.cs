using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorScale : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Camera cam = GameObject.Find("OrtoCamera").GetComponent<Camera>();
        float height = (float) (Camera.main.orthographicSize * 2.0);
        float width = height * Screen.width / Screen.height;
        transform.localScale = new Vector3(width, height, 0.1f);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
