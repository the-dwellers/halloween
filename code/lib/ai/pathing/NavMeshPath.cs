using System.Collections.Generic;
using Sandbox;

namespace lib.Ai.Pathing;

/// <summary>
/// Follows the shortest path between two points via the navmesh.
/// </summary>
public class NavMeshPath : BasePathing
{
	public List<Vector3> NavmeshPoints = new();

	public bool IsEmpty => NavmeshPoints.Count <= 1;

	public override void Update( Vector3 from, Vector3 to )
	{
		var needsBuild = false;

		if ( !TargetPosition.AlmostEqual( to, 5 ) )
		{
			TargetPosition = to;
			needsBuild = true;
		}

		if ( !NavMesh.IsLoaded )
		{
			// navmesh not found, dumb movement
			TargetPosition = from;
			NavmeshPoints = new List<Vector3>
			{
				to,
				from
			};
			return;
		}

		if ( needsBuild )
		{
			var fromFixed = NavMesh.GetClosestPoint( from );
			var toFixed = NavMesh.GetClosestPoint( to );

			if ( fromFixed == null )
				return;

			NavmeshPoints.Clear();

			NavMesh.GetClosestPoint( from );

			NavMesh.BuildPath( fromFixed.Value, toFixed.Value, NavmeshPoints );

		}

		if ( NavmeshPoints.Count <= 1 )
			return;

		var deltaToCurrent = from - NavmeshPoints[0];
		var deltaToNext = from - NavmeshPoints[1];
		var delta = NavmeshPoints[1] - NavmeshPoints[0];
		var deltaNormal = delta.Normal;

		if ( deltaToNext.WithZ( 0 ).Length < 20 )
		{
			NavmeshPoints.RemoveAt( 0 );
			return;
		}

		// If we're in front of this line then
		// remove it and move on to next one
		if ( deltaToNext.Normal.Dot( deltaNormal ) >= 1.0f )
		{
			NavmeshPoints.RemoveAt( 0 );
		}
	}

	public float Distance( int point, Vector3 from )
	{
		if ( NavmeshPoints.Count <= point ) return float.MaxValue;

		return NavmeshPoints[point].WithZ( from.z ).Distance( from );
	}

	public override Vector3 GetDirection( Vector3 position )
	{
		if ( NavmeshPoints.Count <= 0 )
			return position;

		if ( NavmeshPoints.Count == 1 )
		{
			return (NavmeshPoints[0] - position).WithZ( 0 ).Normal;
		}

		return (NavmeshPoints[1] - position).WithZ( 0 ).Normal;
	}
}
