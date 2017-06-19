
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Collections;

public class MegaKML
{
	enum kmlGeometryType
	{
		POINT,
		LINESTRING
	}
	enum kmlTagType
	{
		POINT,
		LINESTRING,
		COORDINATES
	}

	List<Hashtable> PointsCollection = new List<Hashtable>();	//all parsed kml points
	List<Hashtable> LinesCollection = new List<Hashtable>();	//all parsed kml lines
	Hashtable Point;	//single point (part of PointsCollection)
	Hashtable Line;	//single line (part of LinesCollection)
	Hashtable Coordinates;	//object coordinate

	Hashtable KMLCollection = new Hashtable();//parsed KML

	kmlGeometryType? currentGeometry = null;//currently parsed geometry object
	kmlTagType? currentKmlTag = null;//currently parsed kml tag        

	string lastError;

	List<Vector3> points = new List<Vector3>();

	public Hashtable KMLDecode(string fileName)
	{
		points.Clear();
		readKML(fileName);
		if ( PointsCollection != null )
			KMLCollection.Add("POINTS", PointsCollection);

		if ( LinesCollection != null )
			KMLCollection.Add("LINES", LinesCollection);

		return KMLCollection;
	}

	private void readKML(string fileName)
	{
		using ( XmlReader kmlread = XmlReader.Create(fileName) )
		{
			while ( kmlread.Read() )//read kml node by node
			{
				switch ( kmlread.NodeType )
				{
					case XmlNodeType.Element:
						switch ( kmlread.Name.ToUpper() )
						{
							case "POINT":
								currentGeometry = kmlGeometryType.POINT;
								Point = new Hashtable();
								break;

							case "LINESTRING":
								currentGeometry = kmlGeometryType.LINESTRING;
								Line = new Hashtable();
								break;

							case "COORDINATES":
								currentKmlTag = kmlTagType.COORDINATES;
								break;
						}
						break;

					case XmlNodeType.EndElement:
						switch ( kmlread.Name.ToUpper() )
						{
							case "POINT":
								if ( Point != null )
									PointsCollection.Add(Point);
								Point = null;
								currentGeometry = null;
								currentKmlTag = null;
								break;

							case "LINESTRING":
								if ( Line != null )
									LinesCollection.Add(Line);
								Line = null;
								currentGeometry = null;
								currentKmlTag = null;
								break;
						}
						break;

					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
					case XmlNodeType.Comment:
					case XmlNodeType.XmlDeclaration:
						switch ( currentKmlTag )
						{
							case kmlTagType.COORDINATES:
								parseGeometryVal(kmlread.Value);//try to parse coordinates
								break;
						}
						break;

					case XmlNodeType.DocumentType:
						break;

					default: break;
				}
			}
		}
	}

	protected void parseGeometryVal(string tag_value)
	{
		switch ( currentGeometry )
		{
			case kmlGeometryType.POINT:
				parsePoint(tag_value);
				break;

			case kmlGeometryType.LINESTRING:
				parseLine(tag_value);
				break;
		}
	}

	protected void parsePoint(string tag_value)
	{
		Hashtable value = null;
		string[] coordinates;

		switch ( currentKmlTag )
		{
			case kmlTagType.COORDINATES:
				value = new Hashtable();
				coordinates = tag_value.Split(',');
				if ( coordinates.Length < 2 )
					lastError = "ERROR IN FORMAT OF POINT COORDINATES";

				value.Add("LNG", coordinates[0].Trim());
				value.Add("LAT", coordinates[1].Trim());
				Point.Add("COORDINATES", value);
				break;
		}
	}

	protected void parseLine(string tag_value)
	{
		string[] vertex;
		string[] coordinates;

		switch ( currentKmlTag )
		{
			case kmlTagType.COORDINATES:
				vertex = tag_value.Trim().Split(' ');//Split linestring to vertexes

				foreach ( string point in vertex )
				{
					coordinates = point.Split(',');
					if ( coordinates.Length < 2 )
						LastError = "ERROR IN FORMAT OF LINESTRING COORDINATES";

					points.Add(new Vector3(float.Parse(coordinates[0]), float.Parse(coordinates[2]), float.Parse(coordinates[1])));
				}
				break;
		}
	}

	public string LastError
	{
		get { return lastError; }
		set
		{
			lastError = value;
			throw new System.Exception(lastError);
		}
	}

	public Vector3[] GetPoints(float scale)
	{
		Bounds bounds = new Bounds(points[0], Vector3.zero);

		for ( int i = 0; i < points.Count; i++ )
			bounds.Encapsulate(points[i]);

		for ( int i = 0; i < points.Count; i++ )
			points[i] = ConvertLatLon(points[i], bounds.center, scale, false);

		return points.ToArray();
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

		p.z = (float)(-x * scl);
		p.y = (float)y;

		return p;
	}
}
// 219
