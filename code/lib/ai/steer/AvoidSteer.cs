using System;
using System.Linq;
using lib.Ai.Pathing;
using Sandbox;

namespace lib.Ai.Steer;

/// <summary>
/// Avoid the target entity by trying to navmesh far away from the current location.
/// Remember to use SetAvoidTarget(Entity)
/// </summary>
public class AvoidSteer : BaseSteer
{
	protected NavMeshPath Line { get; private set; }

	protected Entity AvoidTarget { get; private set; }

	public AvoidSteer()
	{
		Line = new NavMeshPath();
	}

	public void SetAvoidTarget( Entity Target )
	{
		AvoidTarget = Target;
	}

	private Vector3 TargetPosition;

	public override void Tick( Vector3 currentPosition )
	{
		// Close to target position, generate a new target
		TargetPosition = currentPosition + (currentPosition.Normal - AvoidTarget.Position.Normal) * 1000;

		Line.Update( currentPosition, TargetPosition );
		DebugOverlay.Line( currentPosition, TargetPosition, Color.Yellow );

		// Final Direction We want to move in
		Output.Direction = Line.GetDirection( currentPosition );
		Output.Finished = false;
	}
}
