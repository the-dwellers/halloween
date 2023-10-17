using System;
using System.Linq;
using lib.Ai.Pathing;
using Sandbox;

namespace lib.Ai.Steer;

/// <summary>
/// Wander around a location
/// </summary>
public class WanderSteer : BaseSteer
{
	protected NavMeshPath Line { get; private set; }

	protected Entity AvoidTarget { get; private set; }

	public WanderSteer()
	{
		Line = new NavMeshPath();
	}

	public override void Tick( Vector3 currentPosition )
	{
		// Random direction
		Output.Direction = Vector3.Random;
		Output.Finished = false;
	}
}
