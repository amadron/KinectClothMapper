
using UnityEngine;
using System;
using System.Collections.Generic;

public class MegaShapeOSMTag
{
	public bool		show = true;
	public bool		import = false;
	public MegaShapeOSMWay	way;
	public string	k;
	public List<MegaShapeOSMTag>	vs = new List<MegaShapeOSMTag>();
}

public class MegaShapeOSMNode
{
	//public int		id;
	public ulong		id;
	public Vector3 pos = Vector3.zero;
}

public class MegaShapeOSMWay
{
	//public int			id;
	public ulong id;
	public List<ulong> nodes = new List<ulong>();
	public string		name	= "None";
	public List<MegaShapeOSMTag>	tags = new List<MegaShapeOSMTag>();
}

public class MegaShapeOSM
{
	static public List<MegaShapeOSMNode>	osmnodes = new List<MegaShapeOSMNode>();
	static public List<MegaShapeOSMWay> osmways = new List<MegaShapeOSMWay>();
	static public List<MegaShapeOSMTag>	tags = new List<MegaShapeOSMTag>();

	MegaShapeOSMNode FindNode(ulong id)
	{
		for ( int i = 0; i < osmnodes.Count; i++ )
		{
			if ( osmnodes[i].id == id )
			{
				return osmnodes[i];
			}
		}

		return null;
	}

	public void AdjustPoints(float scale)
	{
		Bounds bounds = new Bounds(osmnodes[0].pos, Vector3.zero);

		for ( int i = 0; i < osmnodes.Count; i++ )
			bounds.Encapsulate(osmnodes[i].pos);

		for ( int i = 0; i < osmnodes.Count; i++ )
			osmnodes[i].pos = ConvertLatLon(osmnodes[i].pos, bounds.center, scale, false);
	}

	Vector3 ConvertLatLon(Vector3 pos, Vector3 centre, float scale, bool adjust)
	{
		double scl = (111322.3167 / scale);

		double x = pos.x - centre.x;
		double y = pos.y - centre.y;
		double z = pos.z - centre.z;

		Vector3 p;

		if ( adjust )
		{
			double r = 6378137.0;
			p.x = (float)(z * (2.0 * Mathf.Tan(Mathf.Deg2Rad * (0.5f)) * r * Mathf.Cos(Mathf.Deg2Rad * (float)x)));
		}
		else
			p.x = (float)(z * scl);

		p.z = (float)(x * scl);
		p.y = (float)y;

		return p;
	}

	public void LoadXMLTags(string sxldata)	//, float scale, bool cspeed, string name, float smoothness)
	{
		osmnodes.Clear();
		osmways.Clear();
		tags.Clear();

		MegaXMLReader xml = new MegaXMLReader();
		MegaXMLNode node = xml.read(sxldata);

		ParseXML(node);
	}

	bool CanImport(MegaShapeOSMWay way)
	{
		for ( int i = 0; i < way.tags.Count; i++ )
		{
			if ( way.tags[i].import )
				return true;
		}

		return false;
	}

	string GetName(MegaShapeOSMWay way)
	{
		string name = "";

		for ( int i = 0; i < way.tags.Count; i++ )
		{
			if ( way.tags[i].import )
			{
				if ( name.Length > 0 )
					name += " ";

				name += way.tags[i].k;
			}
		}

		return name;
	}

	public void LoadXML(string sxldata, float scale, bool cspeed, string name, float smoothness, bool combine)
	{
		GameObject root = new GameObject();

		root.name = name;

		// Get bounds for imports
		AdjustPoints(scale);

		// Create a new shape object for each way
		for ( int i = 0; i < osmways.Count; i++ )
		{
			MegaShapeOSMWay way = osmways[i];

			if ( way.nodes.Count > 1 && CanImport(way) )
			{
				GameObject	osmobj = new GameObject();
				osmobj.transform.position = Vector3.zero;
				osmobj.transform.parent = root.transform;

				if ( way.name.Length == 0 )
					way.name = "No Name";

				osmobj.name = GetName(way);	//way.name;

				MegaShape shape = (MegaShape)osmobj.AddComponent<MegaShape>();

				shape.smoothness = smoothness;	//smooth;
				shape.drawHandles = false;

				MegaSpline spline = shape.splines[0];	//NewSpline(shape);
				spline.knots.Clear();
				spline.constantSpeed = cspeed;
				spline.subdivs = 40;

				bool closed = false;
				int count = way.nodes.Count;

				if ( way.nodes[0] == way.nodes[count - 1] )
				{
					count--;
					closed = true;
				}

				Vector3[] points = new Vector3[count];

				for ( int j = 0; j < count; j++ )
				{
					MegaShapeOSMNode nd = FindNode(way.nodes[j]);
					points[j] = nd.pos;
				}

				Bounds bounds = new Bounds(points[0], Vector3.zero);

				for ( int k = 0; k < points.Length; k++ )
					bounds.Encapsulate(points[k]);

				for ( int k = 0; k < points.Length; k++ )
					points[k] -= bounds.center;

				osmobj.transform.position = bounds.center;	//Vector3.zero;

				shape.BuildSpline(0, points, closed);

				shape.drawTwist = true;
				shape.makeMesh = false;
				shape.imported = true;

				shape.CoordAdjust(scale, new Vector3(1.0f, 1.0f, 1.0f), 0);
				shape.CalcLength();	//10);

				shape.stepdist = (shape.splines[0].length / shape.splines[0].knots.Count) / 1.0f;
			}

		}

		osmnodes.Clear();
		osmways.Clear();
		tags.Clear();
	}

	public void ParseXML(MegaXMLNode node)
	{
		foreach ( MegaXMLNode n in node.children )
		{
			switch ( n.tagName )
			{
				case "osm": ParseOSM(n); break;
			}

			ParseXML(n);
		}
	}

	public void ParseOSM(MegaXMLNode node)
	{
		foreach ( MegaXMLNode n in node.children )
		{
			switch ( n.tagName )
			{
				case "node":	ParseNode(n);	break;
				case "way":		ParseWay(n);	break;
			}
		}
	}

	public void ParseNode(MegaXMLNode node)
	{
		MegaShapeOSMNode onode = new MegaShapeOSMNode();

		for ( int i = 0; i < node.values.Count; i++ )
		{
			MegaXMLValue val = node.values[i];

			switch ( val.name )
			{
				case "id":
					//Debug.Log("id " + val.value);
					onode.id = ulong.Parse(val.value);
					break;
				case "lat": onode.pos.x = float.Parse(val.value); break;
				case "lon": onode.pos.z = float.Parse(val.value); break;
			}
		}

		osmnodes.Add(onode);
	}

	public void ParseWay(MegaXMLNode node)
	{
		MegaShapeOSMWay way = new MegaShapeOSMWay();

		way.name = "";

		for ( int i = 0; i < node.values.Count; i++ )
		{
			MegaXMLValue val = node.values[i];

			switch ( val.name )
			{
				case "id": way.id = ulong.Parse(val.value); break;
			}
		}

		foreach ( MegaXMLNode n in node.children )
		{
			switch ( n.tagName )
			{
				case "nd": ParseND(n, way); break;
				case "tag": ParseTag(n, way); break;
			}
		}

		osmways.Add(way);
	}

	public void ParseND(MegaXMLNode node, MegaShapeOSMWay way)
	{
		for ( int i = 0; i < node.values.Count; i++ )
		{
			MegaXMLValue val = node.values[i];

			switch ( val.name )
			{
				case "ref":	way.nodes.Add(ulong.Parse(val.value));	break;
			}
		}
	}

	MegaShapeOSMTag FindTagK(string val)
	{
		for ( int i = 0; i < tags.Count; i++ )
		{
			if ( tags[i].k == val )
			{
				return tags[i];
			}
		}

		return null;
	}

	MegaShapeOSMTag AddV(MegaShapeOSMTag tag, string v)
	{
		if ( tag != null )
		{
			for ( int i = 0; i < tag.vs.Count; i++ )
			{
				if ( tag.vs[i].k == v )
					return tag.vs[i];
			}
		}

		MegaShapeOSMTag vtag = new MegaShapeOSMTag();
		vtag.k = v;
		tag.vs.Add(vtag);
		return vtag;
	}

	public void ParseTag(MegaXMLNode node, MegaShapeOSMWay way)
	{
		MegaShapeOSMTag tag = null;

		for ( int i = 0; i < node.values.Count; i++ )
		{
			MegaXMLValue val = node.values[i];


			switch ( val.name )
			{
				case "k":
					tag = FindTagK(val.value);

					if ( tag == null )
					{
						tag = new MegaShapeOSMTag();
						tag.k = val.value;
						tags.Add(tag);
					}

					break;

				case "v":
					way.tags.Add(AddV(tag, val.value));
					break;
			}
		}
	}

	public void importData(string sxldata, float scale, bool cspeed, string name, float smoothness, bool combine)
	{
		LoadXML(sxldata, scale, cspeed, name, smoothness, combine);
	}

	public void readOSMData(string sxldata)
	{
		LoadXMLTags(sxldata);
	}
}
