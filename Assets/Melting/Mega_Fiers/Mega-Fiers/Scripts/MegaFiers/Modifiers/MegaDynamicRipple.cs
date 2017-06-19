
using UnityEngine;

[AddComponentMenu("Modifiers/Dynamic Ripple")]
public class MegaDynamicRipple : MegaModifier
{
	public MegaAxis	axis = MegaAxis.Y;
	public int		cols = 8;
	public int		rows = 8;
	[HideInInspector]
	public float[]	buffer1;
	[HideInInspector]
	public float[]	buffer2;
	[HideInInspector]
	public int[]	vertexIndices;
	public float	damping = 0.999f;
	public float	WaveHeight = 2.0f;
	public float	Force = 1.0f;
	public float DropsPerSec = 1.0f;
	public float speed = 0.5f;

	public float[]	input;
	public float inputdamp = 0.99f;
	public float InputForce = 0.1f;
	public bool Obstructions = false;
	public float[]	blockers;

	float time = 0.0f;

	public float scale = 1.0f;
	private bool swapMe = true;

	public bool	bilinearSample = false;

	public override string ModName() { return "Dynamic Ripple"; }
	public override string GetHelpURL() { return "?page_id=2395"; }

	public Texture2D	obTexture;
	Collider mycollider;

	[ContextMenu("Reset Sim")]
	public void ResetGrid()
	{
		Setup();
	}


	public void SetObstructions(Texture2D obtex)
	{
		obTexture = obtex;

		if ( blockers == null || blockers.Length != buffer1.Length )
		{
			blockers = new float[buffer1.Length];
		}

		if ( obTexture )
		{
			for ( int y = 0; y < rows; y++ )
			{
				int yoff = (y * (cols));
				float yf = (float)y / (float)rows;
				for ( int x = 0; x < cols; x++ )
				{
					float xf = (float)x / (float)cols;
					if ( bilinearSample )
						blockers[yoff + x] = obTexture.GetPixelBilinear(xf, yf).grayscale;
					else
						blockers[yoff + x] = obTexture.GetPixel(x, y).grayscale;
				}
			}
		}
		else
		{
			for ( int i = 0; i < blockers.Length; i++ )
				blockers[i] = 1.0f;
		}
	}

	// Float version
	public override void Modify(MegaModifiers mc)
	{
		for ( int i = 0; i < verts.Length; i++ )
		{
			Vector3 p = verts[i];	//tm.MultiplyPoint3x4(verts[i]);

			int vertIndex = vertexIndices[i];

			p[vertcomponent] += currentBuffer[vertIndex] * scale;	// * 1.0f / Force) * WaveHeight;

			sverts[i] = p;	//invtm.MultiplyPoint3x4(p);
		}
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		if ( i >= 0 )
		{
			int vertIndex = vertexIndices[i];

			p[vertcomponent] += currentBuffer[vertIndex] * scale;	// * 1.0f / Force) * WaveHeight;
		}

		return invtm.MultiplyPoint3x4(p);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		// init
		if ( buffer1 == null || rows * cols != buffer1.Length )
		{
			Setup();
			swapMe = false;
			currentBuffer = buffer2;
			return false;
		}

		// Update ripples
		if ( swapMe )
		{
			// process the ripples for this frame
			processRipples(buffer1, buffer2);
			currentBuffer = buffer2;
		}
		else
		{
			processRipples(buffer2, buffer1);
			currentBuffer = buffer2;
		}

		swapMe = !swapMe;

		return Prepare(mc);
	}

	float[] currentBuffer;

	public override bool Prepare(MegaModContext mc)
	{
		if ( mycollider == null )
			mycollider = GetComponent<Collider>();
		return true;
	}

	// Ripple code

	public int	vertcomponent = 1;

	// Use this for initialization
	void Setup()
	{
		int len = rows * cols;
		buffer1 = new float[len];
		buffer2 = new float[len];
		input = new float[len];

		swapMe = false;
		currentBuffer = buffer2;

		vertexIndices = new int[verts.Length];

		SetObstructions(obTexture);

		for ( int i = 0; i < len; i++ )
		{
			buffer1[i] = 0;
			buffer2[i] = 0;
		}

		int xc = 0;
		int yc = 1;

		Vector3 size = bbox.Size();

		if ( size.x == 0.0f )
			axis = MegaAxis.X;

		if ( size.y == 0.0f )
			axis = MegaAxis.Y;

		if ( size.z == 0.0f )
			axis = MegaAxis.Z;

		// this will produce a list of indices that are sorted the way I need them to 
		// be for the algo to work right
		switch ( axis )
		{
			case MegaAxis.X:
				vertcomponent = 0;
				xc = 1;
				yc = 2;
				break;

			case MegaAxis.Y:
				vertcomponent = 1;
				xc = 0;
				yc = 2;
				break;

			case MegaAxis.Z:
				vertcomponent = 2;
				xc = 0;
				yc = 1;
				break;
		}

		for ( int i = 0; i < verts.Length; i++ )
		{
			float column = ((verts[i][xc] - bbox.min[xc]) / size[xc]);// + 0.5;
			float row = ((verts[i][yc] - bbox.min[yc]) / size[yc]);// + 0.5;

			if ( column >= 1.0f )
				column = 0.999f;

			if ( row >= 1.0f )
				row = 0.999f;

			int ci = (int)(column * (float)(cols));
			int ri = (int)(row * (float)(rows));
			float position = (ri * (cols)) + ci;	// + 0.5f;
			vertexIndices[i] = (int)position;	//] = i;
		}
	}

	// Normalized position, or even a world and local
	void splashAtPoint(int x, int y, float force)
	{
		int p = ((y * (cols)) + x);
		buffer1[p] = force;
		buffer1[p - 1] = force * 0.5f;
		buffer1[p + 1] = force * 0.5f;
		buffer1[p + (cols + 0)] = force * 0.5f;
		buffer1[p + (cols + 0) + 1] = force * 0.45f;
		buffer1[p + (cols + 0) - 1] = force * 0.45f;
		buffer1[p - (cols + 0)] = force * 0.5f;
		buffer1[p - (cols + 0) + 1] = force * 0.45f;
		buffer1[p - (cols + 0) - 1] = force * 0.45f;
	}

	void splashAtPoint1(int x, int y, float force)
	{
		int p = ((y * (cols)) + x);
		buffer1[p] = force;
		buffer1[p - 1] = force * 0.5f;
		buffer1[p + 1] = force * 0.5f;
		buffer1[p + (cols + 0)] = force * 0.5f;
		buffer1[p + (cols + 0) + 1] = force * 0.45f;
		buffer1[p + (cols + 0) - 1] = force * 0.45f;
		buffer1[p - (cols + 0)] = force * 0.5f;
		buffer1[p - (cols + 0) + 1] = force * 0.45f;
		buffer1[p - (cols + 0) - 1] = force * 0.45f;

		buffer2[p] = force;
		buffer2[p - 1] = force * 0.5f;
		buffer2[p + 1] = force * 0.5f;
		buffer2[p + (cols + 0)] = force * 0.5f;
		buffer2[p + (cols + 0) + 1] = force * 0.45f;
		buffer2[p + (cols + 0) - 1] = force * 0.45f;
		buffer2[p - (cols + 0)] = force * 0.5f;
		buffer2[p - (cols + 0) + 1] = force * 0.45f;
		buffer2[p - (cols + 0) - 1] = force * 0.45f;
	}

	public int wakesize = 1;
	public float wakefalloff = 1.0f;
	public float wakeforce = 1.0f;

	int ipart(float x)
	{
		return (int)x;
	}

	int round(float x)
	{
		return ipart(x + 0.5f);
	}

	float fpart(float x)
	{
		return x - (int)x;
	}

	float rfpart(float x)
	{
		return 1.0f - fpart(x);
	}

	void swap(ref float v1, ref float v2)
	{
		float temp = v1;
		v1 = v2;
		v2 = temp;
	}

	void plot(int x, int y, float force)
	{
		input[x + (y * cols)] = force;
	}

	void drawLine(float x1, float y1, float x2, float y2, float force)
	{
		float dx = x2 - x1;
		float dy = y2 - y1;

		if ( Mathf.Abs(dx) < Mathf.Abs(dy) )
		{
			swap(ref x1, ref y1);
			swap(ref x2, ref y2);
			swap(ref dx, ref dy);
		}

		if ( x2 < x1 )
		{
			swap(ref x1, ref x2);
			swap(ref y1, ref y2);
		}

		float gradient = dy / dx;

		// handle first endpoint
		float xend = round(x1);
		float yend = y1 + gradient * (xend - x1);
		float xgap = rfpart(x1 + 0.5f);
		int xpxl1 = (int)xend;  // this will be used in the main loop
		int ypxl1 = ipart(yend);

		plot(xpxl1, ypxl1, rfpart(yend) * xgap * force);
		plot(xpxl1, ypxl1 + 1, fpart(yend) * xgap * force);
		float intery = yend + gradient; // first y-intersection for the main loop

		// handle second endpoint
		xend = round(x2);
		yend = y2 + gradient * (xend - x2);
		xgap = fpart(x2 + 0.5f);
		int xpxl2 = (int)xend;  // this will be used in the main loop
		int ypxl2 = ipart(yend);
		plot(xpxl2, ypxl2, rfpart(yend) * xgap * force);
		plot(xpxl2, ypxl2 + 1, fpart(yend) * xgap * force);

		// main loop
		for ( int x = xpxl1 + 1; x < xpxl2 - 1; x++ )
		{
			plot(x, ipart(intery), rfpart(intery) * force);
			plot(x, ipart(intery) + 1, fpart(intery) * force);
			intery = intery + gradient;
		}
	}

	void wakeAtPointWu(float x, float y, float force)
	{
	}

	// Add correct amount for position in cell
	public void wakeAtPointAdd1(float x, float y, float force)
	{
		int xi = Mathf.RoundToInt(x) - 1;
		int yi = Mathf.RoundToInt(y) - 1;

		float[]	dists = new float[4];

		int index = 0;
		int count = 0;
		float tdist = 0.0f;
		for ( int j = yi; j < yi + 2; j++ )
		{
			for ( int i = xi; i < xi + 2; i++ )
			{
				float dx = ((float)i + 0.5f) - x;
				float dy = ((float)j + 0.5f) - y;
				float dist = Mathf.Sqrt(dx * dx + dy * dy);

				if ( dist < 1.0f )
				{
					tdist += dist;
					dists[index] = dist;
					count++;

					//input[i + (j * cols)] = force * (1.0f - dist);
				}
				else
					dists[index] = 1.0f;

				index++;
			}
		}

		index = 0;
		for ( int j = yi; j < yi + 2; j++ )
		{
			for ( int i = xi; i < xi + 2; i++ )
			{
				if ( j >= 0 && j < rows && i >= 0 && i < cols )
				{
					if ( dists[index] < 1.0f )
					{
						input[i + (j * cols)] = force * (dists[index] / tdist);	// * (1.0f - dists[index]);
					}
				}
				index++;
			}
		}
	}

	void wakeAtPointAdd(int x, int y, float force)
	{
		int p = ((y * (cols)) + x);

		input[p] = force;
	}

	void wakeAtPoint(int x, int y, float force)
	{
		int p = ((y * (cols)) + x);

		input[p] = force;
		input[p - 1] = force * 0.5f;
		input[p + 1] = force * 0.5f;
		input[p + (cols + 0)] = force * 0.5f;
		input[p + (cols + 0) + 1] = force * 0.45f;
		input[p + (cols + 0) - 1] = force * 0.45f;
		input[p - (cols + 0)] = force * 0.5f;
		input[p - (cols + 0) + 1] = force * 0.45f;
		input[p - (cols + 0) - 1] = force * 0.45f;
	}

	void processRipples(float[] source, float[] dest)
	{
		for ( int y = 1; y < rows - 1; y++ )
		{
			int yoff = (y * (cols));
			for ( int x = 1; x < cols - 1; x++ )
			{
				input[yoff + x] *= inputdamp;
			}
		}

		if ( Obstructions )
		{
			for ( int y = 1; y < rows - 1; y++ )
			{
				int yoff = (y * (cols));
				for ( int x = 1; x < cols - 1; x++ )
				{
					int p = yoff + x;
					dest[p] = input[p] + (((source[p - 1] + source[p + 1] + source[p - (cols)] + source[p + (cols)]) * speed) - dest[p]);
					dest[p] = dest[p] * damping * blockers[p];
				}
			}
		}
		else
		{
			for ( int y = 1; y < rows - 1; y++ )
			{
				int yoff = (y * (cols));
				for ( int x = 1; x < cols - 1; x++ )
				{
					int p = yoff + x;
					dest[p] = input[p] + (((source[p - 1] + source[p + 1] + source[p - (cols)] + source[p + (cols)]) * speed) - dest[p]);
					dest[p] = dest[p] * damping;
				}
			}
		}
	}

	void Update()
	{
		if ( buffer1 == null || rows * cols != buffer1.Length )
			return;

		time += Time.deltaTime;

		int drops = (int)(time * DropsPerSec);	//Time.deltaTime

		if ( drops > 0 )	//time > DropTime )
		{
			time = 0.0f;

			if ( rows > 8 && cols > 8 )
			{
				for ( int i = 0; i < drops; i++ )
				{
					int x = Random.Range(8, cols - 8);
					int y = Random.Range(8, rows - 8);
					splashAtPoint(x, y, Force * Random.Range(0.1f, 1.0f));
				}
			}
		}

		checkInput();
	}

	// We need to do draw from last pos to new to void gaps
	float lastcol = -1.0f;
	float lastrow = -1.0f;
	bool	lastdown = false;

	public float GetWaterHeight(Vector3 lpos)
	{
		if ( currentBuffer == null )
			return 0.0f;

		float x = (lpos.x - bbox.min.x) / (bbox.max.x - bbox.min.x);
		float y = (lpos.y - bbox.min.y) / (bbox.max.y - bbox.min.y);

		int xi = (int)(x * cols);
		int yi = (int)(y * rows);

		if ( xi < 0 || xi >= cols )
			return 0.0f;

		if ( yi < 0 || yi >= rows )
			return 0.0f;

		return currentBuffer[(yi * cols) + xi];
	}

	void checkInput()
	{
		if ( Input.GetMouseButton(0) )
		{
			RaycastHit[] hits;

			hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition));
			for ( int i = 0; i < hits.Length; i++ )
			{
				if ( hits[i].collider.gameObject == gameObject )
				{
					if ( hits[i].collider is BoxCollider )
					{
						Vector3 p = gameObject.transform.worldToLocalMatrix.MultiplyPoint(hits[i].point);
						BoxCollider bc = (BoxCollider)hits[i].collider;

                        p -= bc.center;
						if ( bc.size.x != 0.0f )
							p.x /= bc.size.x;

						if ( bc.size.y != 0.0f )
							p.y /= bc.size.y;

						if ( bc.size.z != 0.0f )
							p.z /= bc.size.z;

						p.x += 0.5f;
						p.y += 0.5f;
						p.z += 0.5f;

						float column = 0.0f;
						float row = 0.0f;

						switch ( axis )
						{
							case MegaAxis.X:
								vertcomponent = 0;
								column = (p.y) * (cols - 1);
								row = p.z * (rows - 1);
								break;

							case MegaAxis.Y:
								column = (p.x) * (cols - 1);
								row = p.z * (rows - 1);
								break;

							case MegaAxis.Z:
								column = (p.x) * (cols - 1);
								row = p.y * (rows - 1);
								break;
						}

						if ( lastdown )
							Line(lastcol, lastrow, column, row);
						else
							wakeAtPointAdd1((int)column, (int)row, -InputForce);

						lastdown = true;
						lastrow = row;
						lastcol = column;
						return;
					}
					else
					{
						float column = (1.0f - hits[i].textureCoord.x) * (cols - 1);
						float row = hits[i].textureCoord.y * (rows - 1);

						if ( lastdown )
							Line(lastcol, lastrow, column, row);
						else
							wakeAtPointAdd1((int)column, (int)row, -InputForce);

						lastdown = true;
						lastrow = row;
						lastcol = column;
						return;
					}
				}
			}
		}
		else
			lastdown = false;
	}

	void checkInput1()
	{
		if ( Input.GetMouseButton(0) )
		{
			RaycastHit hit;
			if ( Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit) )
			{
				if ( hit.collider.gameObject != gameObject )
					return;

				float column = (1.0f - hit.textureCoord.x) * (cols - 1);
				float row = hit.textureCoord.y * (rows - 1);

				if ( lastdown )
					Line(lastcol, lastrow, column, row);
				else
					wakeAtPointAdd1((int)column, (int)row, -InputForce);

				lastdown = true;
				lastrow = row;
				lastcol = column;
			}
		}
		else
			lastdown = false;
	}

	void Line(float x0, float y0, float x1, float y1)
	{
		if ( Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0) )
		{
			int ys = (int)Mathf.Abs(y0 - y1);

			float dy = 1.0f;
			if ( y1 < y0 )
				dy = -1.0f;

			float dx = (x1 - x0);

			if ( ys > 0 )
				dx /= (float)ys;

			for ( int y = 0; y <= ys; y++ )
			{
				wakeAtPointAdd1(x0, y0, -InputForce);
				x0 += dx;
				y0 += dy;
			}
		}
		else
		{
			int xs = (int)Mathf.Abs(x0 - x1);

			float dx = 1.0f;
			if ( x1 < x0 )
				dx = -1.0f;

			float dy = (y1 - y0);

			if ( xs > 0 )
				dy /= (float)xs;

			for ( int x = 0; x <= xs; x++ )
			{
				wakeAtPointAdd1(x0, y0, -InputForce);
				x0 += dx;
				y0 += dy;
			}
		}
	}

	public void Line(float x0, float y0, float x1, float y1, float force)
	{
		if ( Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0) )
		{
			int ys = (int)Mathf.Abs(y0 - y1);

			float dy = 1.0f;
			if ( y1 < y0 )
				dy = -1.0f;

			float dx = (x1 - x0);

			if ( ys > 0 )
				dx /= (float)ys;

			for ( int y = 0; y <= ys; y++ )
			{
				wakeAtPointAdd1(x0, y0, force);
				x0 += dx;
				y0 += dy;
			}
		}
		else
		{
			int xs = (int)Mathf.Abs(x0 - x1);

			float dx = 1.0f;
			if ( x1 < x0 )
				dx = -1.0f;

			float dy = (y1 - y0);

			if ( xs > 0 )
				dy /= (float)xs;

			for ( int x = 0; x <= xs; x++ )
			{
				wakeAtPointAdd1(x0, y0, force);
				x0 += dx;
				y0 += dy;
			}
		}
	}

	public void ForceAt(float x, float y, float force)
	{
		Vector3 lpos = Vector3.zero;
		lpos.x = x;
		lpos.z = y;

		lpos = transform.worldToLocalMatrix.MultiplyPoint(lpos);

		x = (lpos.x - bbox.min[xc]) / (bbox.max[xc] - bbox.min[xc]);
		y = (lpos.y - bbox.min[yc]) / (bbox.max[yc] - bbox.min[yc]);

		int xi = (int)(x * cols);
		int yi = (int)(y * rows);

		if ( xi < 0 || xi >= cols )
			return;

		if ( yi < 0 || yi >= rows )
			return;

		input[(yi * cols) + xi] = force;
	}

	public void ForceAt(Vector3 p, float force)
	{
		p = transform.worldToLocalMatrix.MultiplyPoint(p);

		BoxCollider bc = (BoxCollider)mycollider;
		if ( bc.size.x != 0.0f )
			p.x /= bc.size.x;

		if ( bc.size.y != 0.0f )
			p.y /= bc.size.y;

		if ( bc.size.z != 0.0f )
			p.z /= bc.size.z;

		p.x += 0.5f;
		p.y += 0.5f;
		p.z += 0.5f;

		float column = 0.0f;
		float row = 0.0f;

		switch ( axis )
		{
			case MegaAxis.X:
				vertcomponent = 0;
				column = (p.y) * (cols - 1);
				row = p.z * (rows - 1);
				break;

			case MegaAxis.Y:
				column = (p.x) * (cols - 1);
				row = p.z * (rows - 1);
				break;

			case MegaAxis.Z:
				column = (p.x) * (cols - 1);
				row = p.y * (rows - 1);
				break;
		}

		int xi = (int)column;
		int yi = (int)row;

		if ( xi < 0 || xi >= cols )
			return;

		if ( yi < 0 || yi >= rows )
			return;

		input[(yi * cols) + xi] = force;
	}

	int xc = 0;
	int yc = 0;

	void BuildMesh()
	{
		Vector3 pos = Vector3.zero;
		Vector3 last = Vector3.zero;

		xc = 0;
		yc = 0;

		switch ( axis )
		{
			case MegaAxis.X:
				xc = 1;
				yc = 2;
				break;

			case MegaAxis.Y:
				xc = 0;
				yc = 2;
				break;

			case MegaAxis.Z:
				xc = 0;
				yc = 1;
				break;
		}

		for ( int i = 0; i < rows; i++ )
		{
			pos.z = bbox.min[yc] + ((bbox.max[yc] - bbox.min[yc]) * ((float)i / (float)rows));

			for ( int j = 0; j < cols; j++ )
			{
				pos.x = bbox.min[xc] + ((bbox.max[xc] - bbox.min[xc]) * ((float)j / (float)cols));

				pos.y = currentBuffer[(i * cols) + j];

				if ( j > 0 )
					Gizmos.DrawLine(last, pos);

				last = pos;
			}
		}

		for ( int j = 0; j < cols; j++ )
		{
			pos.x = bbox.min[xc] + ((bbox.max[xc] - bbox.min[xc]) * ((float)j / (float)cols));

			for ( int i = 0; i < rows; i++ )
			{
				pos.z = bbox.min[yc] + ((bbox.max[yc] - bbox.min[yc]) * ((float)i / (float)rows));

				pos.y = currentBuffer[(i * cols) + j];

				if ( i > 0 )
					Gizmos.DrawLine(last, pos);

				last = pos;
			}
		}
	}

	public override void DrawGizmo(MegaModContext context)
	{
		Gizmos.color = Color.yellow;

		Matrix4x4 gtm = Matrix4x4.identity;
		Vector3 pos = gizmoPos;
		pos.x = -pos.x;
		pos.y = -pos.y;
		pos.z = -pos.z;

		Vector3 scl = gizmoScale;
		scl.x = 1.0f - (scl.x - 1.0f);
		scl.y = 1.0f - (scl.y - 1.0f);
		gtm.SetTRS(pos, Quaternion.Euler(gizmoRot), scl);

		Matrix4x4 tm = Matrix4x4.identity;

		switch ( axis )
		{
			case MegaAxis.X:
				MegaMatrix.RotateZ(ref tm, 90.0f * Mathf.Deg2Rad);
				break;

			case MegaAxis.Z:
				MegaMatrix.RotateX(ref tm, 90.0f * Mathf.Deg2Rad);
				break;
		}

		Gizmos.matrix = transform.localToWorldMatrix * gtm * tm;

		BuildMesh();
	}
}