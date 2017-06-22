using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorScale : MonoBehaviour {
    public float screenWidth;
    public float screenHeight;
    public static float resFactorY;
    public static float resFactorX;
	// Use this for initialization
	void Start () {
        Camera cam = GameObject.Find("OrtoCamera").GetComponent<Camera>();
        screenHeight = (float) (Camera.main.orthographicSize * 2.0);
        screenWidth = screenHeight * Screen.width / Screen.height;
        resFactorY = (float) Screen.height / 1080;
        resFactorX = (float) Screen.width / 1920;
        transform.localScale = new Vector3(screenWidth, screenHeight, 0.1f);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
