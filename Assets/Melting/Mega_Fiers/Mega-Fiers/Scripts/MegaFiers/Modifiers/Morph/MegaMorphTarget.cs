
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class MegaMorphTarget	//Base
{
	public string		name = "Empty";
	public float		percent;
	public bool			showparams = true;
	public Vector3[]	points;
	public MOMVert[]	mompoints;
	public MOPoint[]	loadpoints;
}

[System.Serializable]
public class MOPoint
{
	public int		id;
	public Vector3	p;
	public float	w;
}