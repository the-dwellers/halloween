using System;
using System.Linq;
using lib.Ai.Pathing;
using Sandbox;

namespace lib.Ai.Steer;

/// <summary>
/// Follow a fixed spline path created in hammer
/// </summary>
public class PathSteer : BaseSteer
{
	protected NavMeshPath Line { get; private set; }

	private Action Callback;

	public void SetPath( Vector3 CurrentPosition, GenericPathEntity path, Action callback = null )
	{
		if ( !path.IsValid || path.PathNodes.Count < 2 )
		{
			return;
		}

		Callback = callback;
		LastPosition = CurrentPosition;
		CurrentPath = path;
		CurrentNode = path.PathNodes.IndexOf( path.PathNodes.MinBy( x => x.WorldPosition.Distance( LastPosition ) ) );
	}

	public GenericPathEntity CurrentPath { private set; get; }

	int CurrentNode;

	int NextNode => CurrentPath.PathNodes.Count == CurrentNode ? -1 : CurrentNode + 1;
//
	public PathSteer()
	{
		Line = new NavMeshPath();
	}

	public override void Tick( Vector3 currentPosition )
	{

		CurrentPath.DrawPath(1);

		if ( CurrentPath is null || !CurrentPath.IsValid || Target.IsNaN || Output.Finished )
			return;

		Target = CurrentPath.PathNodes[CurrentNode].WorldPosition;

		if ( currentPosition.Distance( CurrentPath.PathNodes[CurrentNode].WorldPosition ) < 20f )
		{
			// Close enough to end point, get new path target
			CurrentNode = NextNode;
			if ( NextNode == -1 )
			{
				// End of point and no new node.
				Output.Finished = true;
				if ( Callback is not null )
				{
					Callback.Invoke();
				}
				return;
			}
		}

		Line.Update( currentPosition, Target );
		DebugOverlay.Line( currentPosition, Target, Color.Yellow );

		Output.Direction = Line.GetDirection( currentPosition );
		Output.Finished = false;
		Output.CurrentNode = CurrentNode;
		Output.NextNode = NextNode;
	}
}
