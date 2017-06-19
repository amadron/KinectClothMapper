
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MegaMorphBase : MegaModifier
{
	public List<MegaMorphChan>	chanBank = new List<MegaMorphChan>();
	public MegaMorphAnimType animtype = MegaMorphAnimType.Bezier;

	public override void PostCopy(MegaModifier src)
	{
		MegaMorphBase mor = (MegaMorphBase)src;

		chanBank = new List<MegaMorphChan>();

		for ( int c = 0; c < mor.chanBank.Count; c++ )
		{
			MegaMorphChan chan = new MegaMorphChan();

			MegaMorphChan.Copy(mor.chanBank[c], chan);
			chanBank.Add(chan);
		}
	}

	public string[] GetChannelNames()
	{
		string[] names = new string[chanBank.Count];

		for ( int i = 0; i < chanBank.Count; i++ )
			names[i] = chanBank[i].mName;

		return names;
	}

	public MegaMorphChan GetChannel(string name)
	{
		for ( int i = 0; i < chanBank.Count; i++ )
		{
			if ( chanBank[i].mName == name )
				return chanBank[i];
		}

		return null;
	}

	public int NumChannels()
	{
		return chanBank.Count;
	}

	public void SetPercent(int i, float percent)
	{
		if ( i >= 0 && i < chanBank.Count )
			chanBank[i].Percent = percent;
	}

	public void SetPercentLim(int i, float alpha)
	{
		if ( i >= 0 && i < chanBank.Count )
		{
			if ( chanBank[i].mUseLimit )
				chanBank[i].Percent = chanBank[i].mSpinmin + ((chanBank[i].mSpinmax - chanBank[i].mSpinmin) * alpha);
			else
				chanBank[i].Percent = alpha * 100.0f;
		}
	}

	public void SetPercent(int i, float percent, float speed)
	{
		chanBank[i].SetTarget(percent, speed);
	}

	public void ResetPercent(int[] channels, float speed)
	{
		for ( int i = 0; i < channels.Length; i++ )
		{
			int chan = channels[i];
			chanBank[chan].SetTarget(0.0f, speed);
		}
	}

	public float GetPercent(int i)
	{
		if ( i >= 0 && i < chanBank.Count )
			return chanBank[i].Percent;

		return 0.0f;
	}

	public void SetAnim(float t)
	{
		if ( animtype == MegaMorphAnimType.Bezier )
		{
			for ( int i = 0; i < chanBank.Count; i++ )
			{
				if ( chanBank[i].control != null )
				{
					if ( chanBank[i].control.Times != null )
					{
						if ( chanBank[i].control.Times.Length > 0 )
							chanBank[i].Percent = chanBank[i].control.GetFloat(t);	//, 0.0f, 100.0f);
					}
				}
			}
		}
		else
		{
			for ( int i = 0; i < chanBank.Count; i++ )
			{
				if ( chanBank[i].control != null )
				{
					if ( chanBank[i].control.Times != null )
					{
						if ( chanBank[i].control.Times.Length > 0 )
							chanBank[i].Percent = chanBank[i].control.GetHermiteFloat(t);	//, 0.0f, 100.0f);
					}
				}
			}
		}
	}

	[System.Serializable]
	public class MegaMorphBlend
	{
		public float t;
		public float weight;
	}

	public int numblends;
	public List<MegaMorphBlend>	blends;	// = new List<MegaMorphBlend>();

	public void SetAnimBlend(float t, float weight)
	{
		if ( blends == null )
		{
			blends = new List<MegaMorphBlend>();

			for ( int i = 0; i < 4; i++ )
			{
				blends.Add(new MegaMorphBlend());
			}
		}

		blends[numblends].t = t;
		blends[numblends].weight = weight;

		numblends++;
	}

	public void ClearBlends()
	{
		numblends = 0;
	}

	public void SetChannels()
	{
		float tweight = 0.0f;
		for ( int i = 0; i < numblends; i++ )
		{
			tweight += blends[i].weight;
		}

		for ( int b = 0; b < numblends; b++ )
		{
			for ( int c = 0; c < chanBank.Count; c++ )
			{
				if ( animtype == MegaMorphAnimType.Bezier )
				{
					if ( chanBank[c].control != null )
					{
						if ( chanBank[c].control.Times != null )
						{
							if ( chanBank[c].control.Times.Length > 0 )
							{
								if ( b == 0 )
									chanBank[c].Percent = chanBank[c].control.GetFloat(blends[b].t) * (blends[b].weight / tweight);	//, 0.0f, 100.0f);
								else
									chanBank[c].Percent += chanBank[c].control.GetFloat(blends[b].t) * (blends[b].weight / tweight);	//, 0.0f, 100.0f);
							}
						}
					}
				}
				else
				{
					if ( chanBank[c].control != null )
					{
						if ( chanBank[c].control.Times != null )
						{
							if ( chanBank[c].control.Times.Length > 0 )
							{
								if ( b == 0 )
									chanBank[c].Percent = chanBank[c].control.GetHermiteFloat(blends[b].t) * (blends[b].weight / tweight);	//, 0.0f, 100.0f);
								else
									chanBank[c].Percent += chanBank[c].control.GetHermiteFloat(blends[b].t) * (blends[b].weight / tweight);	//, 0.0f, 100.0f);
							}
						}
					}
				}
			}
		}
	}
}

public enum MegaMorphAnimType
{
	Bezier,
	Hermite,
}

[AddComponentMenu("Modifiers/Morph")]
public class MegaMorph : MegaMorphBase
{
	public bool				UseLimit;
	public float			Max;
	public float			Min;
	public Vector3[]		oPoints;
	public int[]			mapping;
	public float			importScale = 1.0f;
	public bool				flipyz = false;
	public bool				negx = false;
	[HideInInspector]
	public float			tolerance = 0.0001f;

	public bool				showmapping = false;
	public float			mappingSize = 0.001f;
	public int				mapStart = 0;
	public int				mapEnd = 0;

	public Vector3[]		dif;	// changed to public, check it doesnt break anything
	static Vector3[]		endpoint	= new Vector3[4];
	static Vector3[]		splinepoint	= new Vector3[4];
	static Vector3[]		temppoint	= new Vector3[2];
	Vector3[]	p1;
	Vector3[]	p2;
	Vector3[]	p3;
	Vector3[]	p4;

	public List<float>	pers = new List<float>(4);

	public override string ModName() { return "Morph"; }
	public override string GetHelpURL() { return "?page_id=257"; }

	[HideInInspector]
	public int compressedmem = 0;
	[HideInInspector]
	public int compressedmem1 = 0;
	[HideInInspector]
	public int memuse = 0;

	// This should be a MegaModifiers method, so on Start/Awake run through and regrab all verts
	// and then for each modifier call a PS3Remap method
	// Actually we could leave systems in place and have an end method that build a PS3 mesh, this would mean
	// we get the overhead of building a new vertex array but should be more than offset by not modifying dup
	// verts
	// So at start we use current verts and then remap against freshly fetched vert data
	public void PS3Remap()
	{
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		if ( animate )
		{
			animtime += Time.deltaTime * speed;

			switch ( repeatMode )
			{
				case MegaRepeatMode.Loop:	animtime = Mathf.Repeat(animtime, looptime);	break;
				//case RepeatMode.PingPong: animtime = Mathf.PingPong(animtime, looptime); break;
				case MegaRepeatMode.Clamp:	animtime = Mathf.Clamp(animtime, 0.0f, looptime); break;
			}
			//animtime = Mathf.Repeat(animtime, looptime);
			SetAnim(animtime);
		}

		if ( dif == null )
		{
			dif = new Vector3[mc.mod.verts.Length];
		}

		return Prepare(mc);
	}

	public bool  animate = false;
	public float atime = 0.0f;
	public float animtime = 0.0f;
	public float looptime = 0.0f;
	public MegaRepeatMode	repeatMode = MegaRepeatMode.Loop;
	public float speed = 1.0f;

	public override bool Prepare(MegaModContext mc)
	{
		if ( chanBank != null && chanBank.Count > 0 )
			return true;

		return false;
	}

	// Find the closest and not use a threshold, use sqr distance
	int FindVert(Vector3 vert)
	{
		float closest = Vector3.SqrMagnitude(oPoints[0] - vert);
		int find = 0;

		for ( int i = 0; i < oPoints.Length; i++ )
		{
			float dif = Vector3.SqrMagnitude(oPoints[i] - vert);

			if ( dif < closest )
			{
				closest = dif;
				find = i;
			}
		}

		return find;	//0;
	}

	void DoMapping(Mesh mesh)
	{
		mapping = new int[mesh.vertexCount];

		for ( int v = 0; v < mesh.vertexCount; v++ )
		{
			Vector3 vert = mesh.vertices[v];
			vert.x = -vert.x;
			mapping[v] = FindVert(vert);
		}
	}

	public void DoMapping(Vector3[] verts)
	{
		mapping = new int[verts.Length];

		for ( int v = 0; v < verts.Length; v++ )
		{
			mapping[v] = FindVert(verts[v]);
		}
	}

	// Only need to call if a Percent value has changed on a Channel or a target, so flag for a change
	void SetVerts(int j, Vector3[] p)
	{
		switch ( j )
		{
			case 0: p1 = p;	break;
			case 1: p2 = p; break;
			case 2: p3 = p; break;
			case 3: p4 = p; break;
		}
	}

	void SetVerts(MegaMorphChan chan, int j, Vector3[] p)
	{
		switch ( j )
		{
			case 0: chan.p1 = p; break;
			case 1: chan.p2 = p; break;
			case 2: chan.p3 = p; break;
			case 3: chan.p4 = p; break;
		}
	}

	// Seperate function for compressed data, when we compress we should be able to tell if its worth it
	// as we will need to dup opoints etc

	static int framenum;

	// oPoints whould be verts
	public override void Modify(MegaModifiers mc)
	{
		if ( nonMorphedVerts != null && nonMorphedVerts.Length > 1 )
		{
			ModifyCompressed(mc);
			return;
		}

		framenum++;
		mc.ChangeSourceVerts();

		float fChannelPercent;
		Vector3	delt;

		// cycle through channels, searching for ones to use
		bool firstchan = true;
		bool morphed = false;

		float min = 0.0f;
		float max = 100.0f;

		if ( UseLimit )
		{
			min = Min;
			max = Max;
		}

		for ( int i = 0; i < chanBank.Count; i++ )
		{
			MegaMorphChan chan = chanBank[i];
			chan.UpdatePercent();

			if ( UseLimit )
				fChannelPercent = Mathf.Clamp(chan.Percent, min, max);	//chan.mSpinmin, chan.mSpinmax);
			else
			{
				if ( chan.mUseLimit )
					fChannelPercent = Mathf.Clamp(chan.Percent, chan.mSpinmin, chan.mSpinmax);
				else
					fChannelPercent = Mathf.Clamp(chan.Percent, 0.0f, 100.0f);
			}

			//fChannelPercent *= chan.weight;

			if ( fChannelPercent != 0.0f || (fChannelPercent == 0.0f && chan.fChannelPercent != 0.0f) )
			{
				chan.fChannelPercent = fChannelPercent;

				if ( chan.mTargetCache != null && chan.mTargetCache.Count > 0 && chan.mActiveOverride )	//&& fChannelPercent != 0.0f )
				{
					morphed = true;

					if ( chan.mUseLimit )	//|| glUseLimit )
					{
					}

					if ( firstchan )
					{
						firstchan = false;
						for ( int pointnum = 0; pointnum < oPoints.Length; pointnum++ )
							dif[pointnum] = oPoints[pointnum];
					}

					if ( chan.mTargetCache.Count == 1 )
					{
						for ( int pointnum = 0; pointnum < oPoints.Length; pointnum++ )
						{
							delt = chan.mDeltas[pointnum];

							dif[pointnum].x += delt.x * fChannelPercent;
							dif[pointnum].y += delt.y * fChannelPercent;
							dif[pointnum].z += delt.z * fChannelPercent;
						}
					}
					else
					{
						int totaltargs = chan.mTargetCache.Count;	// + 1;	// + 1;

						float fProgression = fChannelPercent;	//Mathf.Clamp(fChannelPercent, 0.0f, 100.0f);
						int segment = 1;
						while ( segment <= totaltargs && fProgression >= chan.GetTargetPercent(segment - 2) )
							segment++;

						if ( segment > totaltargs )
							segment = totaltargs;

						p4 = oPoints;

						if ( segment == 1 )
						{
							p1 = oPoints;
							p2 = chan.mTargetCache[0].points;	// mpoints
							p3 = chan.mTargetCache[1].points;
						}
						else
						{
							if ( segment == totaltargs )
							{
								int targnum = totaltargs - 1;

								for ( int j = 2; j >= 0; j-- )
								{
									targnum--;
									if ( targnum == -2 )
										SetVerts(j, oPoints);
									else
										SetVerts(j, chan.mTargetCache[targnum + 1].points);
								}
							}
							else
							{
								int targnum = segment;

								for ( int j = 3; j >= 0; j-- )
								{
									targnum--;
									if ( targnum == -2 )
										SetVerts(j, oPoints);
									else
										SetVerts(j, chan.mTargetCache[targnum + 1].points);
								}
							}
						}

						float targetpercent1 = chan.GetTargetPercent(segment - 3);
						float targetpercent2 = chan.GetTargetPercent(segment - 2);

						float top = fProgression - targetpercent1;
						float bottom = targetpercent2 - targetpercent1;
						float u = top / bottom;

						{
							for ( int pointnum = 0; pointnum < oPoints.Length; pointnum++ )
							{
								Vector3 vert = oPoints[pointnum];

								float length;

								Vector3 progession;

								endpoint[0] = p1[pointnum];
								endpoint[1] = p2[pointnum];
								endpoint[2] = p3[pointnum];
								endpoint[3] = p4[pointnum];

								if ( segment == 1 )
								{
									splinepoint[0] = endpoint[0];
									splinepoint[3] = endpoint[1];
									temppoint[1] = endpoint[2] - endpoint[0];
									temppoint[0] = endpoint[1] - endpoint[0];
									length = temppoint[1].sqrMagnitude;

									if ( length == 0.0f )
									{
										splinepoint[1] = endpoint[0];
										splinepoint[2] = endpoint[1];
									}
									else
									{
										splinepoint[2] = endpoint[1] - (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];
										splinepoint[1] = endpoint[0] + chan.mCurvature * (splinepoint[2] - endpoint[0]);
									}
								}
								else
								{
									if ( segment == totaltargs )
									{
										splinepoint[0] = endpoint[1];
										splinepoint[3] = endpoint[2];
										temppoint[1] = endpoint[2] - endpoint[0];
										temppoint[0] = endpoint[1] - endpoint[2];
										length = temppoint[1].sqrMagnitude;

										if ( length == 0.0f )
										{
											splinepoint[1] = endpoint[0];
											splinepoint[2] = endpoint[1];
										}
										else
										{
											splinepoint[1] = endpoint[1] - (Vector3.Dot(temppoint[1], temppoint[0]) * chan.mCurvature / length) * temppoint[1];
											splinepoint[2] = endpoint[2] + chan.mCurvature * (splinepoint[1] - endpoint[2]);
										}
									}
									else
									{
										temppoint[1] = endpoint[2] - endpoint[0];
										temppoint[0] = endpoint[1] - endpoint[0];
										length = temppoint[1].sqrMagnitude;
										splinepoint[0] = endpoint[1];
										splinepoint[3] = endpoint[2];

										if ( length == 0.0f )
											splinepoint[1] = endpoint[0];
										else
											splinepoint[1] = endpoint[1] + (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];

										temppoint[1] = endpoint[3] - endpoint[1];
										temppoint[0] = endpoint[2] - endpoint[1];
										length = temppoint[1].sqrMagnitude;

										if ( length == 0.0f )
											splinepoint[2] = endpoint[1];
										else
											splinepoint[2] = endpoint[2] - (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];
									}
								}

								MegaUtils.Bez3D(out progession, ref splinepoint, u);

								dif[pointnum].x += (progession.x - vert.x) * chan.weight;	//delt;
								dif[pointnum].y += (progession.y - vert.y) * chan.weight;	//delt;
								dif[pointnum].z += (progession.z - vert.z) * chan.weight;	//delt;
							}
						}
					}
				}
			}
		}

		if ( morphed )
		{
			for ( int i = 0; i < mapping.Length; i++ )
				sverts[i] = dif[mapping[i]];
		}
		else
		{
			for ( int i = 0; i < verts.Length; i++ )
				sverts[i] = verts[i];
		}
	}

	bool Changed(int v, int c)
	{
		for ( int t = 0; t < chanBank[c].mTargetCache.Count; t++ )
		{
			if ( !oPoints[v].Equals(chanBank[c].mTargetCache[t].points[v]) )
				return true;
		}

		return false;
	}

	// Move compression to editor script so not included in releases
	// Option for compression, per channel or per morph, will effect multicore support
	// morph compression means simple mt split

	// TODO: have a threshold for differences
	// Do we compress per channel or whole mesh, channel would be best
	// Can only compress once as we are destroying data, so if vert counts dont match dont recompress
	public void Compress()
	{
		if ( oPoints != null )
		{
			List<int>	altered = new List<int>();

			int count = 0;

			for ( int c = 0; c < chanBank.Count; c++ )
			{
				altered.Clear();

				for ( int v = 0; v < oPoints.Length; v++ )
				{
					if ( Changed(v, c) )
						altered.Add(v);
				}

				count += altered.Count;
			}

			Debug.Log("Compressed will only morph " + count + " points instead of " + (oPoints.Length * chanBank.Count));
			compressedmem = count * 12;
		}
	}

	public void ModifyCompressed(MegaModifiers mc)
	{
		framenum++;
		mc.ChangeSourceVerts();

		float fChannelPercent;
		Vector3	delt;

		// cycle through channels, searching for ones to use
		bool firstchan = true;
		bool morphed = false;

		for ( int i = 0; i < chanBank.Count; i++ )
		{
			MegaMorphChan chan = chanBank[i];
			chan.UpdatePercent();

			if ( chan.mUseLimit )
				fChannelPercent = Mathf.Clamp(chan.Percent, chan.mSpinmin, chan.mSpinmax);
			else
				fChannelPercent = Mathf.Clamp(chan.Percent, 0.0f, 100.0f);

			if ( fChannelPercent != 0.0f || (fChannelPercent == 0.0f && chan.fChannelPercent != 0.0f) )
			{
				chan.fChannelPercent = fChannelPercent;

				if ( chan.mTargetCache != null && chan.mTargetCache.Count > 0 && chan.mActiveOverride )	//&& fChannelPercent != 0.0f )
				{
					morphed = true;

					if ( chan.mUseLimit )	//|| glUseLimit )
					{
					}

					// New bit
					if ( firstchan )
					{
						firstchan = false;
						// Save a int array of morphedpoints and use that, then only dealing with changed info
						for ( int pointnum = 0; pointnum < morphedVerts.Length; pointnum++ )
						{
							// this will change when we remove points
							int p = morphedVerts[pointnum];
							dif[p] = oPoints[p];	//morphedVerts[pointnum]];
						}
					}
					// end new

					if ( chan.mTargetCache.Count == 1 )
					{
						// Save a int array of morphedpoints and use that, then only dealing with changed info
						for ( int pointnum = 0; pointnum < morphedVerts.Length; pointnum++ )
						{
							int p = morphedVerts[pointnum];
							delt = chan.mDeltas[p];	//morphedVerts[pointnum]];
							//delt = chan.mDeltas[pointnum];	//morphedVerts[pointnum]];

							dif[p].x += delt.x * fChannelPercent;
							dif[p].y += delt.y * fChannelPercent;
							dif[p].z += delt.z * fChannelPercent;
						}
					}
					else
					{
						int totaltargs = chan.mTargetCache.Count;	// + 1;	// + 1;

						float fProgression = fChannelPercent;	//Mathf.Clamp(fChannelPercent, 0.0f, 100.0f);
						int segment = 1;
						while ( segment <= totaltargs && fProgression >= chan.GetTargetPercent(segment - 2) )
							segment++;

						if ( segment > totaltargs )
							segment = totaltargs;

						p4 = oPoints;

						if ( segment == 1 )
						{
							p1 = oPoints;
							p2 = chan.mTargetCache[0].points;	// mpoints
							p3 = chan.mTargetCache[1].points;
						}
						else
						{
							if ( segment == totaltargs )
							{
								int targnum = totaltargs - 1;

								for ( int j = 2; j >= 0; j-- )
								{
									targnum--;
									if ( targnum == -2 )
										SetVerts(j, oPoints);
									else
										SetVerts(j, chan.mTargetCache[targnum + 1].points);
								}
							}
							else
							{
								int targnum = segment;

								for ( int j = 3; j >= 0; j-- )
								{
									targnum--;
									if ( targnum == -2 )
										SetVerts(j, oPoints);
									else
										SetVerts(j, chan.mTargetCache[targnum + 1].points);
								}
							}
						}

						float targetpercent1 = chan.GetTargetPercent(segment - 3);
						float targetpercent2 = chan.GetTargetPercent(segment - 2);

						float top = fProgression - targetpercent1;
						float bottom = targetpercent2 - targetpercent1;
						float u = top / bottom;

						for ( int pointnum = 0; pointnum < morphedVerts.Length; pointnum++ )
						{
							int p = morphedVerts[pointnum];
							Vector3 vert = oPoints[p];	//pointnum];

							float length;

							Vector3 progession;

							endpoint[0] = p1[p];
							endpoint[1] = p2[p];
							endpoint[2] = p3[p];
							endpoint[3] = p4[p];

							if ( segment == 1 )
							{
								splinepoint[0] = endpoint[0];
								splinepoint[3] = endpoint[1];
								temppoint[1] = endpoint[2] - endpoint[0];
								temppoint[0] = endpoint[1] - endpoint[0];
								length = temppoint[1].sqrMagnitude;

								if ( length == 0.0f )
								{
									splinepoint[1] = endpoint[0];
									splinepoint[2] = endpoint[1];
								}
								else
								{
									splinepoint[2] = endpoint[1] - (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];
									splinepoint[1] = endpoint[0] + chan.mCurvature * (splinepoint[2] - endpoint[0]);
								}
							}
							else
							{
								if ( segment == totaltargs )
								{
									splinepoint[0] = endpoint[1];
									splinepoint[3] = endpoint[2];
									temppoint[1] = endpoint[2] - endpoint[0];
									temppoint[0] = endpoint[1] - endpoint[2];
									length = temppoint[1].sqrMagnitude;

									if ( length == 0.0f )
									{
										splinepoint[1] = endpoint[0];
										splinepoint[2] = endpoint[1];
									}
									else
									{
										splinepoint[1] = endpoint[1] - (Vector3.Dot(temppoint[1], temppoint[0]) * chan.mCurvature / length) * temppoint[1];
										splinepoint[2] = endpoint[2] + chan.mCurvature * (splinepoint[1] - endpoint[2]);
									}
								}
								else
								{
									temppoint[1] = endpoint[2] - endpoint[0];
									temppoint[0] = endpoint[1] - endpoint[0];
									length = temppoint[1].sqrMagnitude;
									splinepoint[0] = endpoint[1];
									splinepoint[3] = endpoint[2];

									if ( length == 0.0f )
										splinepoint[1] = endpoint[0];
									else
										splinepoint[1] = endpoint[1] + (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];

									temppoint[1] = endpoint[3] - endpoint[1];
									temppoint[0] = endpoint[2] - endpoint[1];
									length = temppoint[1].sqrMagnitude;

									if ( length == 0.0f )
										splinepoint[2] = endpoint[1];
									else
										splinepoint[2] = endpoint[2] - (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];
								}
							}

							MegaUtils.Bez3D(out progession, ref splinepoint, u);

							dif[p].x += progession.x - vert.x;
							dif[p].y += progession.y - vert.y;
							dif[p].z += progession.z - vert.z;
						}
					}
				}
			}
		}

		if ( morphed )
		{
			for ( int i = 0; i < morphMappingFrom.Length; i++ )
				sverts[morphMappingTo[i]] = dif[morphMappingFrom[i]];

			for ( int i = 0; i < nonMorphMappingFrom.Length; i++ )
				sverts[nonMorphMappingTo[i]] = oPoints[nonMorphMappingFrom[i]];
		}
		else
		{
			for ( int i = 0; i < verts.Length; i++ )
				sverts[i] = verts[i];
		}
	}

	// Build compressed data
	public int[]	nonMorphedVerts;
	public int[]	morphedVerts;
	public int[]	morphMappingFrom;
	public int[]	morphMappingTo;
	public int[]	nonMorphMappingFrom;
	public int[]	nonMorphMappingTo;

	[ContextMenu("Compress Morphs")]
	public void BuildCompress()
	{
		bool[]	altered = new bool[oPoints.Length];

		int count = 0;

		for ( int c = 0; c < chanBank.Count; c++ )
		{
			for ( int v = 0; v < oPoints.Length; v++ )
			{
				if ( Changed(v, c) )
					altered[v] = true;
			}
		}

		for ( int i = 0; i < altered.Length; i++ )
		{
			if ( altered[i] )
				count++;
		}

		morphedVerts = new int[count];
		nonMorphedVerts = new int[oPoints.Length - count];

		int mindex = 0;
		int nmindex = 0;

		List<int>	mappedFrom = new List<int>();
		List<int>	mappedTo = new List<int>();

		for ( int i = 0; i < oPoints.Length; i++ )
		{
			if ( altered[i] )
			{
				morphedVerts[mindex++] = i;

				for ( int m = 0; m < mapping.Length; m++ )
				{
					if ( mapping[m] == i )
					{
						mappedFrom.Add(i);
						mappedTo.Add(m);
					}
				}
			}
			else
				nonMorphedVerts[nmindex++] = i;
		}

		morphMappingFrom = mappedFrom.ToArray();
		morphMappingTo = mappedTo.ToArray();

		mappedFrom.Clear();
		mappedTo.Clear();

		for ( int i = 0; i < oPoints.Length; i++ )
		{
			if ( !altered[i] )
			{
				for ( int m = 0; m < mapping.Length; m++ )
				{
					if ( mapping[m] == i )
					{
						mappedFrom.Add(i);
						mappedTo.Add(m);
					}
				}
			}
		}

		nonMorphMappingFrom = mappedFrom.ToArray();
		nonMorphMappingTo = mappedTo.ToArray();

		compressedmem = morphedVerts.Length * chanBank.Count * 12;
	}

	// Threaded version
	Vector3[] _verts;
	Vector3[] _sverts;

	public override void PrepareMT(MegaModifiers mc, int cores)
	{
		PrepareForMT(mc, cores);
	}

	public override void DoWork(MegaModifiers mc, int index, int start, int end, int cores)
	{
		ModifyCompressedMT(mc, index, cores);
	}

	public void PrepareForMT(MegaModifiers mc, int cores)
	{
		if ( setStart == null )
			BuildMorphVertInfo(cores);

		// cycle through channels, searching for ones to use
		mtmorphed = false;

		for ( int i = 0; i < chanBank.Count; i++ )
		{
			MegaMorphChan chan = chanBank[i];
			chan.UpdatePercent();

			float fChannelPercent;

			if ( chan.mUseLimit )
				fChannelPercent = Mathf.Clamp(chan.Percent, chan.mSpinmin, chan.mSpinmax);
			else
				fChannelPercent = Mathf.Clamp(chan.Percent, 0.0f, 100.0f);

			if ( fChannelPercent != 0.0f || (fChannelPercent == 0.0f && chan.fChannelPercent != 0.0f) )
			{
				chan.fChannelPercent = fChannelPercent;

				if ( chan.mTargetCache != null && chan.mTargetCache.Count > 0 && chan.mActiveOverride )
				{
					mtmorphed = true;

					if ( chan.mTargetCache.Count > 1 )
					{
						int totaltargs = chan.mTargetCache.Count;	// + 1;	// + 1;

						chan.fProgression = chan.fChannelPercent;	//Mathf.Clamp(fChannelPercent, 0.0f, 100.0f);
						chan.segment = 1;
						while ( chan.segment <= totaltargs && chan.fProgression >= chan.GetTargetPercent(chan.segment - 2) )
							chan.segment++;

						if ( chan.segment > totaltargs )
							chan.segment = totaltargs;

						chan.p4 = oPoints;

						if ( chan.segment == 1 )
						{
							chan.p1 = oPoints;
							chan.p2 = chan.mTargetCache[0].points;	// mpoints
							chan.p3 = chan.mTargetCache[1].points;
						}
						else
						{
							if ( chan.segment == totaltargs )
							{
								int targnum = totaltargs - 1;

								for ( int j = 2; j >= 0; j-- )
								{
									targnum--;
									if ( targnum == -2 )
										SetVerts(chan, j, oPoints);
									else
										SetVerts(chan, j, chan.mTargetCache[targnum + 1].points);
								}
							}
							else
							{
								int targnum = chan.segment;

								for ( int j = 3; j >= 0; j-- )
								{
									targnum--;
									if ( targnum == -2 )
										SetVerts(chan, j, oPoints);
									else
										SetVerts(chan, j, chan.mTargetCache[targnum + 1].points);
								}
							}
						}
					}
				}
			}
		}

		if ( !mtmorphed )
		{
			for ( int i = 0; i < verts.Length; i++ )
				sverts[i] = verts[i];
		}
	}

	bool mtmorphed;

	public void ModifyCompressedMT(MegaModifiers mc, int tindex, int cores)
	{
		if ( !mtmorphed )
			return;

		int step = morphedVerts.Length / cores;
		int startvert = (tindex * step);
		int endvert = startvert + step;

		if ( tindex == cores - 1 )
			endvert = morphedVerts.Length;

		framenum++;
		Vector3	delt;

		// cycle through channels, searching for ones to use
		bool firstchan = true;

		Vector3[]	endpoint	= new Vector3[4];	// These in channel class
		Vector3[]	splinepoint	= new Vector3[4];
		Vector3[]	temppoint	= new Vector3[2];

		for ( int i = 0; i < chanBank.Count; i++ )
		{
			MegaMorphChan chan = chanBank[i];

			if ( chan.fChannelPercent != 0.0f )
			{
				if ( chan.mTargetCache != null && chan.mTargetCache.Count > 0 && chan.mActiveOverride )	//&& fChannelPercent != 0.0f )
				{
					if ( firstchan )
					{
						firstchan = false;
						for ( int pointnum = startvert; pointnum < endvert; pointnum++ )
						{
							int p = morphedVerts[pointnum];
							dif[p] = oPoints[p];
						}
					}

					if ( chan.mTargetCache.Count == 1 )
					{
						for ( int pointnum = startvert; pointnum < endvert; pointnum++ )
						{
							int p = morphedVerts[pointnum];
							delt = chan.mDeltas[p];

							dif[p].x += delt.x * chan.fChannelPercent;
							dif[p].y += delt.y * chan.fChannelPercent;
							dif[p].z += delt.z * chan.fChannelPercent;
						}
					}
					else
					{
						float targetpercent1 = chan.GetTargetPercent(chan.segment - 3);
						float targetpercent2 = chan.GetTargetPercent(chan.segment - 2);

						float top = chan.fProgression - targetpercent1;
						float bottom = targetpercent2 - targetpercent1;
						float u = top / bottom;

						for ( int pointnum = startvert; pointnum < endvert; pointnum++ )
						{
							int p = morphedVerts[pointnum];
							Vector3 vert = oPoints[p];	//pointnum];

							float length;

							Vector3 progession;

							endpoint[0] = chan.p1[p];
							endpoint[1] = chan.p2[p];
							endpoint[2] = chan.p3[p];
							endpoint[3] = chan.p4[p];

							if ( chan.segment == 1 )
							{
								splinepoint[0] = endpoint[0];
								splinepoint[3] = endpoint[1];
								temppoint[1] = endpoint[2] - endpoint[0];
								temppoint[0] = endpoint[1] - endpoint[0];
								length = temppoint[1].sqrMagnitude;

								if ( length == 0.0f )
								{
									splinepoint[1] = endpoint[0];
									splinepoint[2] = endpoint[1];
								}
								else
								{
									splinepoint[2] = endpoint[1] - (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];
									splinepoint[1] = endpoint[0] + chan.mCurvature * (splinepoint[2] - endpoint[0]);
								}
							}
							else
							{
								if ( chan.segment == chan.mTargetCache.Count )	//chan.totaltargs )
								{
									splinepoint[0] = endpoint[1];
									splinepoint[3] = endpoint[2];
									temppoint[1] = endpoint[2] - endpoint[0];
									temppoint[0] = endpoint[1] - endpoint[2];
									length = temppoint[1].sqrMagnitude;

									if ( length == 0.0f )
									{
										splinepoint[1] = endpoint[0];
										splinepoint[2] = endpoint[1];
									}
									else
									{
										splinepoint[1] = endpoint[1] - (Vector3.Dot(temppoint[1], temppoint[0]) * chan.mCurvature / length) * temppoint[1];
										splinepoint[2] = endpoint[2] + chan.mCurvature * (splinepoint[1] - endpoint[2]);
									}
								}
								else
								{
									temppoint[1] = endpoint[2] - endpoint[0];
									temppoint[0] = endpoint[1] - endpoint[0];
									length = temppoint[1].sqrMagnitude;
									splinepoint[0] = endpoint[1];
									splinepoint[3] = endpoint[2];

									if ( length == 0.0f )
										splinepoint[1] = endpoint[0];
									else
										splinepoint[1] = endpoint[1] + (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];

									temppoint[1] = endpoint[3] - endpoint[1];
									temppoint[0] = endpoint[2] - endpoint[1];
									length = temppoint[1].sqrMagnitude;

									if ( length == 0.0f )
										splinepoint[2] = endpoint[1];
									else
										splinepoint[2] = endpoint[2] - (Vector3.Dot(temppoint[0], temppoint[1]) * chan.mCurvature / length) * temppoint[1];
								}
							}

							MegaUtils.Bez3D(out progession, ref splinepoint, u);

							dif[p].x += progession.x - vert.x;
							dif[p].y += progession.y - vert.y;
							dif[p].z += progession.z - vert.z;
						}
					}
				}
			}
		}

		if ( mtmorphed )
		{
			for ( int i = setStart[tindex]; i < setEnd[tindex]; i++ )
				sverts[morphMappingTo[i]] = dif[morphMappingFrom[i]];

			for ( int i = copyStart[tindex]; i < copyEnd[tindex]; i++ )
				sverts[nonMorphMappingTo[i]] = oPoints[nonMorphMappingFrom[i]];
		}
	}

	int[]	setStart;
	int[]	setEnd;
	int[]	copyStart;
	int[]	copyEnd;

	int Find(int index)
	{
		int f = morphedVerts[index];

		for ( int i = 0; i < morphMappingFrom.Length; i++ )
		{
			if ( morphMappingFrom[i] > f )
				return i;
		}

		return morphMappingFrom.Length - 1;
	}

	void BuildMorphVertInfo(int cores)
	{
		int step = morphedVerts.Length / cores;

		setStart	= new int[cores];
		setEnd		= new int[cores];
		copyStart	= new int[cores];
		copyEnd		= new int[cores];

		int start = 0;
		int fv = 0;

		for ( int i = 0; i < cores; i++ )
		{
			setStart[i] = start;
			if ( i < cores - 1 )
			{
				setEnd[i] = Find(fv + step);
			}
			start = setEnd[i];
			fv += step;
		}

		setEnd[cores - 1] = morphMappingFrom.Length;

		// copys can be simple split as nothing happens to them
		start = 0;
		step = nonMorphMappingFrom.Length / cores;

		for ( int i = 0; i < cores; i++ )
		{
			copyStart[i] = start;
			copyEnd[i] = start + step;
			start += step;
		}

		copyEnd[cores - 1] = nonMorphMappingFrom.Length;
	}

	public void SetAnimTime(float t)
	{
		animtime = t;

		switch ( repeatMode )
		{
			case MegaRepeatMode.Loop:	animtime = Mathf.Repeat(animtime, looptime);	break;
			//case RepeatMode.PingPong: animtime = Mathf.PingPong(animtime, looptime); break;
			case MegaRepeatMode.Clamp:	animtime = Mathf.Clamp(animtime, 0.0f, looptime); break;
		}
		SetAnim(animtime);
	}
}

