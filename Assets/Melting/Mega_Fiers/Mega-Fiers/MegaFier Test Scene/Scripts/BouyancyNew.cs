// Buoyancy.cs
// by Alex Zhdankin
// Version 2.1
//
// http://forum.unity3d.com/threads/72974-Buoyancy-script
//
// Terms of use: do whatever you like

using System.Collections.Generic;
using UnityEngine;

public class BouyancyNew : MonoBehaviour
{
	//	public Ocean ocean;

	public float density = 500;
	public int slicesPerAxis = 2;
	public bool isConcave = false;
	public int voxelsLimit = 16;

	private const float DAMPFER = 0.1f;
	private const float WATER_DENSITY = 1000;

	private float voxelHalfHeight;
	private Vector3 localArchimedesForce;
	private List<Vector3> voxels;
	private bool isMeshCollider;
	//private List<Vector3[]> forces; // For drawing force gizmos

	public GameObject water;
	public MegaDynamicRipple	dynamicwater;

	Transform dwtrans;
	Rigidbody	rbody;
	Collider	mycollider;

	/// <summary>
	/// Provides initialization.
	/// </summary>
	private void Start()
	{
		if ( water )
		{
			dynamicwater = (MegaDynamicRipple)water.GetComponent<MegaDynamicRipple>();
		}

		rbody = GetComponent<Rigidbody>();
		mycollider = GetComponent<Collider>();

		if ( dynamicwater )
			dwtrans = dynamicwater.transform;
		else
		{
			Debug.LogWarning(string.Format("[Buoyancy.cs] No Dynamic Ripple modifier found", name));
		}

		//forces = new List<Vector3[]>(); // For drawing force gizmos

		// Store original rotation and position
		var originalRotation = transform.rotation;
		var originalPosition = transform.position;
		transform.rotation = Quaternion.identity;
		transform.position = Vector3.zero;

		// The object must have a collider
		if ( mycollider == null )
		{
			mycollider = gameObject.AddComponent<MeshCollider>();
			Debug.LogWarning(string.Format("[Buoyancy.cs] Object \"{0}\" had no collider. MeshCollider has been added.", name));
		}
		isMeshCollider = GetComponent<MeshCollider>() != null;

		var bounds = mycollider.bounds;
		if ( bounds.size.x < bounds.size.y )
		{
			voxelHalfHeight = bounds.size.x;
		}
		else
		{
			voxelHalfHeight = bounds.size.y;
		}
		if ( bounds.size.z < voxelHalfHeight )
		{
			voxelHalfHeight = bounds.size.z;
		}
		voxelHalfHeight /= 2.0f * slicesPerAxis;

		// The object must have a RidigBody
		if ( rbody == null )
		{
			rbody = gameObject.AddComponent<Rigidbody>();
			Debug.LogWarning(string.Format("[Buoyancy.cs] Object \"{0}\" had no Rigidbody. Rigidbody has been added.", name));
		}
		rbody.centerOfMass = new Vector3(0, -bounds.extents.y * 0f, 0) + transform.InverseTransformPoint(bounds.center);

		voxels = SliceIntoVoxels(isMeshCollider && isConcave);

		// Restore original rotation and position
		transform.rotation = originalRotation;
		transform.position = originalPosition;

		float volume = rbody.mass / density;

		WeldPoints(voxels, voxelsLimit);

		float archimedesForceMagnitude = WATER_DENSITY * Mathf.Abs(Physics.gravity.y) * volume;
		localArchimedesForce = new Vector3(0, archimedesForceMagnitude, 0) / voxels.Count;

		Debug.Log(string.Format("[Buoyancy.cs] Name=\"{0}\" volume={1:0.0}, mass={2:0.0}, density={3:0.0}", name, volume, rbody.mass, density));
	}

	/// <summary>
	/// Slices the object into number of voxels represented by their center points.
	/// <param name="concave">Whether the object have a concave shape.</param>
	/// <returns>List of voxels represented by their center points.</returns>
	/// </summary>
	private List<Vector3> SliceIntoVoxels(bool concave)
	{
		var points = new List<Vector3>(slicesPerAxis * slicesPerAxis * slicesPerAxis);

		if ( concave )
		{
			var meshCol = GetComponent<MeshCollider>();

			var convexValue = meshCol.convex;
			meshCol.convex = false;

			// Concave slicing
			var bounds = mycollider.bounds;
			for ( int ix = 0; ix < slicesPerAxis; ix++ )
			{
				for ( int iy = 0; iy < slicesPerAxis; iy++ )
				{
					for ( int iz = 0; iz < slicesPerAxis; iz++ )
					{
						float x = bounds.min.x + bounds.size.x / slicesPerAxis * (0.5f + ix);
						float y = bounds.min.y + bounds.size.y / slicesPerAxis * (0.5f + iy);
						float z = bounds.min.z + bounds.size.z / slicesPerAxis * (0.5f + iz);

						var p = transform.InverseTransformPoint(new Vector3(x, y, z));

						if ( PointIsInsideMeshCollider(meshCol, p) )
						{
							points.Add(p);
						}
					}
				}
			}
			if ( points.Count == 0 )
			{
				points.Add(bounds.center);
			}

			meshCol.convex = convexValue;
		}
		else
		{
			// Convex slicing
			var bounds = GetComponent<Collider>().bounds;
			for ( int ix = 0; ix < slicesPerAxis; ix++ )
			{
				for ( int iy = 0; iy < slicesPerAxis; iy++ )
				{
					for ( int iz = 0; iz < slicesPerAxis; iz++ )
					{
						float x = bounds.min.x + bounds.size.x / slicesPerAxis * (0.5f + ix);
						float y = bounds.min.y + bounds.size.y / slicesPerAxis * (0.5f + iy);
						float z = bounds.min.z + bounds.size.z / slicesPerAxis * (0.5f + iz);

						var p = transform.InverseTransformPoint(new Vector3(x, y, z));

						points.Add(p);
					}
				}
			}
		}

		return points;
	}

	/// <summary>
	/// Returns whether the point is inside the mesh collider.
	/// </summary>
	/// <param name="c">Mesh collider.</param>
	/// <param name="p">Point.</param>
	/// <returns>True - the point is inside the mesh collider. False - the point is outside of the mesh collider. </returns>
	private static bool PointIsInsideMeshCollider(Collider c, Vector3 p)
	{
		Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

		foreach ( var ray in directions )
		{
			RaycastHit hit;
			if ( c.Raycast(new Ray(p - ray * 1000, ray), out hit, 1000f) == false )
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Returns two closest points in the list.
	/// </summary>
	/// <param name="list">List of points.</param>
	/// <param name="firstIndex">Index of the first point in the list. It's always less than the second index.</param>
	/// <param name="secondIndex">Index of the second point in the list. It's always greater than the first index.</param>
	private static void FindClosestPoints(IList<Vector3> list, out int firstIndex, out int secondIndex)
	{
		float minDistance = float.MaxValue, maxDistance = float.MinValue;
		firstIndex = 0;
		secondIndex = 1;

		for ( int i = 0; i < list.Count - 1; i++ )
		{
			for ( int j = i + 1; j < list.Count; j++ )
			{
				float distance = Vector3.Distance(list[i], list[j]);
				if ( distance < minDistance )
				{
					minDistance = distance;
					firstIndex = i;
					secondIndex = j;
				}
				if ( distance > maxDistance )
				{
					maxDistance = distance;
				}
			}
		}
	}

	/// <summary>
	/// Welds closest points.
	/// </summary>
	/// <param name="list">List of points.</param>
	/// <param name="targetCount">Target number of points in the list.</param>
	private static void WeldPoints(IList<Vector3> list, int targetCount)
	{
		if ( list.Count <= 2 || targetCount < 2 )
		{
			return;
		}

		while ( list.Count > targetCount )
		{
			int first, second;
			FindClosestPoints(list, out first, out second);

			var mixed = (list[first] + list[second]) * 0.5f;
			list.RemoveAt(second); // the second index is always greater that the first => removing the second item first
			list.RemoveAt(first);
			list.Add(mixed);
		}
	}

	/// <summary>
	/// Returns the water level at given location.
	/// </summary>
	/// <param name="x">x-coordinate</param>
	/// <param name="z">z-coordinate</param>
	/// <returns>Water level</returns>
	private float GetWaterLevel(float x, float z)
	{
		if ( dynamicwater )
		{
			Vector3 lpos = Vector3.zero;
			lpos.x = x;
			lpos.z = z;

			float h = dynamicwater.GetWaterHeight(water.transform.worldToLocalMatrix.MultiplyPoint(lpos));
			//h += dynamicwater.transform.position.y;

			return h;
		}
		//		return ocean == null ? 0.0f : ocean.GetWaterHeightAtLocation(x, z);
		return 0.0f;
	}

	/// <summary>
	/// Calculates physics.
	/// </summary>
	private void FixedUpdate()
	{
		//forces.Clear(); // For drawing force gizmos

		float y = dwtrans.position.y;

		foreach ( var point in voxels )
		{
			Vector3 wp = transform.TransformPoint(point);
			float waterLevel = GetWaterLevel(wp.x, wp.z) + y;

			if ( wp.y - voxelHalfHeight < waterLevel )
			{
				float k = (waterLevel - wp.y) / (2.0f * voxelHalfHeight) + 0.5f;
				if ( k > 1.0f )
				{
					k = 1.0f;
				}
				else if ( k < 0.0f )
				{
					k = 0.0f;
				}

				Vector3 velocity = rbody.GetPointVelocity(wp);
				Vector3 localDampingForce = -velocity * DAMPFER * rbody.mass;
				Vector3 force = localDampingForce + Mathf.Sqrt(k) * localArchimedesForce;
				rbody.AddForceAtPosition(force, wp);

				if ( dynamicwater )
				{
					if ( velocity.y < -ripplevel )
					{
						dynamicwater.ForceAt(wp, velocity.y * rippleforce);	//-force.y * 0.04f);
					}
				}
			}
		}
	}

	public float ripplevel = 0.4f;
	public float rippleforce = 0.1f;
	/// <summary>
	/// Draws gizmos.
	/// </summary>
	private void OnDrawGizmos()
	{
	}
}