
using UnityEngine;
using System.Collections.Generic;

public enum MegaIntegrator
{
	Euler,
	Verlet,
	VerletTimeCorrected,
	MidPoint,
}

public class BaryVert2D
{
	public int		gx;	// Grid position
	public int		gy;
	public Vector2	bary;
}

[System.Serializable]
public class Constraint2D
{
	public bool		active;
	public int		p1;
	public int		p2;
	public float	length;
	public Vector2	pos;
	public int		contype = 0;
	public Transform obj;

	public static Constraint2D CreatePointTargetCon(int _p1, Transform trans)
	{
		Constraint2D con = new Constraint2D();
		con.p1 = _p1;
		con.active = true;
		con.contype = 2;
		con.obj = trans;

		return con;
	}

	public static Constraint2D CreateLenCon(int _p1, int _p2, float _len)
	{
		Constraint2D con = new Constraint2D();
		con.p1 = _p1;
		con.p2 = _p2;
		con.length = _len;
		con.active = true;
		con.contype = 0;

		return con;
	}

	public static Constraint2D CreatePointCon(int _p1, Vector2 _pos)
	{
		Constraint2D con = new Constraint2D();
		con.p1 = _p1;
		con.pos = _pos;
		con.active = true;
		con.contype = 1;

		return con;
	}

	public void Apply(MegaSoft2D soft)
	{
		switch ( contype )
		{
			case 0:	ApplyLengthConstraint2D(soft);	break;
			case 1: ApplyPointConstraint2D(soft); break;
			//case 2: ApplyPointTargetConstraint2D(soft); break;
		}
	}

	// Can have a one that has a range to keep in
	public void ApplyLengthConstraint2D(MegaSoft2D soft)
	{
		if ( active && soft.applyConstraints )
		{
			//calculate direction
			Vector2 direction = soft.masses[p2].pos - soft.masses[p1].pos;

			//calculate current length
			float currentLength = direction.magnitude;

			//check for zero vector
			if ( currentLength != 0.0f )	//direction.x != 0.0f || direction.y != 0.0f || direction.y != 0.0f )
			{
				direction.x /= currentLength;
				direction.y /= currentLength;

				//move to goal positions
				Vector2 moveVector = 0.5f * (currentLength - length) * direction;

				soft.masses[p1].pos.x += moveVector.x;
				soft.masses[p1].pos.y += moveVector.y;

				soft.masses[p2].pos.x += -moveVector.x;
				soft.masses[p2].pos.y += -moveVector.y;
			}
		}
	}

	public void ApplyPointConstraint2D(MegaSoft2D soft)
	{
		if ( active )
			soft.masses[p1].pos = pos;
	}

	public void ApplyAngleConstraint(MegaSoft2D soft)
	{
	}
}

[System.Serializable]
public class Mass2D
{
	public Vector2	pos;
	public Vector2	last;
	public Vector2	force;
	public Vector2	vel;
	public Vector2	posc;
	public Vector2	velc;
	public Vector2	forcec;
	public Vector2	coef1;
	public Vector2	coef2;
	public float	mass;
	public float	oneovermass;

	public Mass2D(float m, Vector2 p)
	{
		mass		= m;
		oneovermass	= 1.0f / mass;
		pos			= p;
		last		= p;
		force		= Vector2.zero;
		vel			= Vector2.zero;
	}
}

[System.Serializable]
public class Spring2D
{
	public int		p1;
	public int		p2;
	public float	restLen;
	public float	ks;
	public float	kd;
	public float	len;

	public Spring2D(int _p1, int _p2, float _ks, float _kd, MegaSoft2D mod)
	{
		p1 = _p1;
		p2 = _p2;
		ks = _ks;
		kd = _kd;
		restLen = Vector2.Distance(mod.masses[p1].pos, mod.masses[p2].pos);
		len = restLen;
	}

	public void doCalculateSpringForce(MegaSoft2D mod)
	{
		Vector2 deltaP = mod.masses[p1].pos - mod.masses[p2].pos;

		float dist = deltaP.magnitude;	//VectorLength(&deltaP); // Magnitude of deltaP

		float Hterm = (dist - restLen) * ks; // Ks * (dist - rest)

		Vector2	deltaV = mod.masses[p1].vel - mod.masses[p2].vel;
		float Dterm = (Vector2.Dot(deltaV, deltaP) * kd) / dist; // Damping Term

		Vector2 springForce = deltaP * (1.0f / dist);
		springForce *= -(Hterm + Dterm);

		mod.masses[p1].force += springForce;
		mod.masses[p2].force -= springForce;
	}

	public void doCalculateSpringForce1(MegaSoft2D mod)
	{
		//get the direction vector
		Vector2 direction = mod.masses[p1].pos - mod.masses[p2].pos;

		//check for zero vector
		if ( direction != Vector2.zero )
		{
			//get length
			float currLength = direction.magnitude;
			//normalize
			direction = direction.normalized;
			//add spring force
			Vector2 force = -ks * ((currLength - restLen) * direction);
			//add spring damping force
			//float v = (currLength - len) / mod.timeStep;

			//force += -kd * v * direction;
			//apply the equal and opposite forces to the objects
			mod.masses[p1].force += force;
			mod.masses[p2].force -= force;
			len = currLength;
		}
	}

	public void doCalculateSpringForce2(MegaSoft2D mod)
	{
		Vector2 deltaP = mod.masses[p1].pos - mod.masses[p2].pos;

		float dist = deltaP.magnitude;	//VectorLength(&deltaP); // Magnitude of deltaP

		float Hterm = (dist - restLen) * ks; // Ks * (dist - rest)

		//Vector2	deltaV = mod.masses[p1].vel - mod.masses[p2].vel;
		float v = (dist - len);	// / mod.timeStep;

		float Dterm = (v * kd) / dist; // Damping Term

		Vector2 springForce = deltaP * (1.0f / dist);
		springForce *= -(Hterm + Dterm);

		mod.masses[p1].force += springForce;
		mod.masses[p2].force -= springForce;
	}
}

// Want verlet for this as no collision will be done, solver type enum
// need to add contact forces for weights on bridge
[System.Serializable]
public class MegaSoft2D
{
	public List<Mass2D>			masses = new List<Mass2D>();
	public List<Spring2D>		springs = new List<Spring2D>();
	public List<Constraint2D>	constraints = new List<Constraint2D>();

	public Vector2 gravity = new Vector2(0.0f, -9.81f);
	public float airdrag = 0.999f;
	public float friction = 1.0f;
	public float timeStep = 0.01f;
	public int	iters = 4;
	public MegaIntegrator	method = MegaIntegrator.Verlet;
	public bool	applyConstraints = true;

	void doCalculateForceseuler()
	{
		for ( int i = 0; i < masses.Count; i++ )
		{
			masses[i].force = masses[i].mass * gravity;
			masses[i].force += masses[i].forcec;
		}

		for ( int i = 0; i < springs.Count; i++ )
			springs[i].doCalculateSpringForce(this);
	}

	void doCalculateForces()
	{
		for ( int i = 0; i < masses.Count; i++ )
		{
			masses[i].force = masses[i].mass * gravity;
			masses[i].force += masses[i].forcec;
		}

		for ( int i = 0; i < springs.Count; i++ )
			springs[i].doCalculateSpringForce1(this);
	}

	void doIntegration1(float dt)
	{
		doCalculateForceseuler();	// Calculate forces, only changes _f

		/*	Then do correction step by integration with central average (Heun) */
		for ( int i = 0; i < masses.Count; i++ )
		{
			masses[i].last = masses[i].pos;
			masses[i].vel += dt * masses[i].force * masses[i].oneovermass;
			masses[i].pos += masses[i].vel * dt;
			masses[i].vel *= friction;
		}

		DoConstraints();
	}

	public float floor = 0.0f;
	void DoCollisions(float dt)
	{
		for ( int i = 0; i < masses.Count; i++ )
		{
			if ( masses[i].pos.y < floor )
				masses[i].pos.y = floor;
		}
	}

	// Change the base code over to Skeel or similar
	//public bool UseVerlet = false;

	// Can do drag per point using a curve
	// perform the verlet integration step
	void VerletIntegrate(float t, float lastt)
	{
		doCalculateForces();	// Calculate forces, only changes _f

		float t2 = t * t;
		/*	Then do correction step by integration with central average (Heun) */
		for ( int i = 0; i < masses.Count; i++ )
		{
			Vector2 last = masses[i].pos;
			masses[i].pos += airdrag * (masses[i].pos - masses[i].last) + masses[i].force * masses[i].oneovermass * t2;	// * t;
			masses[i].last = last;
		}

		DoConstraints();
	}

	// Pointless
	void VerletIntegrateTC(float t, float lastt)
	{
		doCalculateForces();	// Calculate forces, only changes _f

		float t2 = t * t;
		float dt = t / lastt;

		/*	Then do correction step by integration with central average (Heun) */
		for ( int i = 0; i < masses.Count; i++ )
		{
			Vector2 last = masses[i].pos;
			masses[i].pos += airdrag * (masses[i].pos - masses[i].last) * dt + (masses[i].force * masses[i].oneovermass) * t2;	// * t;
			masses[i].last = last;
		}

		DoConstraints();
	}

	void MidPointIntegrate(float t)
	{
	}

	// Satisfy constraints
	public void DoConstraints()
	{
		for ( int i = 0; i < iters; i++ )
		{
			for ( int c = 0; c < constraints.Count; c++ )
			{
				constraints[c].Apply(this);
			}
		}
	}

	public float lasttime = 0.0f;
	public float simtime = 0.0f;

	public void Update()
	{
		if ( masses == null )
			return;

		simtime += Time.deltaTime;	// * fudge;

		if ( Time.deltaTime == 0.0f )
		{
			simtime = 0.01f;
		}

		if ( timeStep <= 0.0f )
			timeStep = 0.001f;

		float ts = 0.0f;

		if ( lasttime == 0.0f )
			lasttime = timeStep;

		while ( simtime > 0.0f)	//timeStep )	//0.0f )
		{
			simtime -= timeStep;
			ts = timeStep;

			switch ( method )
			{
				case MegaIntegrator.Euler:
					doIntegration1(ts);
					break;

				case MegaIntegrator.Verlet:
					VerletIntegrate(ts, lasttime);	//timeStep);
					break;

				case MegaIntegrator.VerletTimeCorrected:
					VerletIntegrateTC(ts, lasttime);	//timeStep);
					break;

				case MegaIntegrator.MidPoint:
					MidPointIntegrate(ts);	//timeStep);
					break;
			}

			lasttime = ts;
		}
	}
}