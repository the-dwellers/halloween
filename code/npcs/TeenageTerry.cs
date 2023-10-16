using Sandbox;
using System;
using System.Linq;

namespace MyGame;
public partial class TeenageTerry : AnimatedEntity
{
	protected string State;
	protected Vector3[] Path;
	protected int CurrentPathSegment;
	protected TimeSince TimeSinceGeneratedPath = 0;
	protected float move_speed;
	protected float LastTickPlayerDistance;
	protected float standing_time = 0;
	const float CHASE_DISTANCE = 100f;
	const float MOVEMENT_SPEED = 4f;
	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );
		//SetupPhysicsFromModel(PhysicsMotionType.Dynamic);
	}

	[GameEvent.Tick.Server]
	private void Tick()
	{
		switch ( State )
		{
			case "idle":
				move_speed = MOVEMENT_SPEED * 0.5f;
				PerformStateIdle();
				break;
			case "running_away":
				move_speed = MOVEMENT_SPEED;
				PerformStateAttackingPawn();
				break;
			case "standing":
				PerformStateStanding();
				break;
			default:
				State = "idle";
				break;
		}
	}

	protected void PerformStateStanding()
	{
		
		var Pawn = GetClosestPawn();
		if ( Pawn != null && Pawn.Position.Distance( Position ) <= CHASE_DISTANCE ){
			State = "running_away";
			return;
		}
		move_speed = 0;
		if ( TimeSinceGeneratedPath >= 1.5 )
			GeneratePathIdle();
		TraversePath();
		standing_time += 1;
		if (standing_time > 180)
		{
			State = "idle";
			return;
		}
		
	}
	protected void PerformStateIdle()
	{
		var Pawn = GetClosestPawn();
		if ( Pawn != null && Pawn.Position.Distance( Position ) <= CHASE_DISTANCE ){
			State = "running_away";
			return;
		}
		if ( TimeSinceGeneratedPath >= 1.5 )
			GeneratePathIdle();
		TraversePath();
		var random = new Random();
		if (random.Int(120) == 1){
			State = "standing";
		}
	}

	protected void PerformStateAttackingPawn()
	{
		var Pawn = GetClosestPawn();
		var PawnDist = Pawn.Position.Distance( Position );
		var DistToPlayerChase = PawnDist / CHASE_DISTANCE;
		Log.Info(DistToPlayerChase);
		if ( Pawn == null || PawnDist > CHASE_DISTANCE )
		{
			State = "idle";
			return;
		}

		if ( TimeSinceGeneratedPath >= 0.05 )
			GeneratePathRun( Pawn );

		move_speed *= (1 / DistToPlayerChase);		
		
		LastTickPlayerDistance = PawnDist;
		TraversePath();
		
	}

	protected Pawn GetClosestPawn()
	{
		return All.OfType<Pawn>()
			.OrderByDescending( x => x.Position.Distance( Position ) )
			.FirstOrDefault();
	}

	protected void GeneratePathIdle()
	{
		var random = new Random();
		TimeSinceGeneratedPath = 0;
		Path = NavMesh.PathBuilder( Position )
			.WithMaxClimbDistance( 16f )
			.WithMaxDropDistance( 16f )
			.WithStepHeight( 16f )
			.WithMaxDistance( 200 )
			.WithPartialPaths()
			.Build( Position + random.VectorInSphere(200))
			.Segments
			.Select( x => x.Position )
			.ToArray();

		CurrentPathSegment = 0;
	}
	public virtual void  GeneratePathRun( Pawn target )
	{
		TimeSinceGeneratedPath = 0;
		
		DebugOverlay.Line(Position, Position + (Position.Normal - target.Position.Normal) * 2000);
		Path = NavMesh.PathBuilder( Position )
			.WithMaxClimbDistance( 16f )
			.WithMaxDropDistance( 16f )
			.WithStepHeight( 16f )
			.WithMaxDistance( 64 )
			.WithPartialPaths()
			.Build( Position + (Position.Normal - target.Position.Normal) * 2000 )
			.Segments
			.Select( x => x.Position )
			.ToArray();

		CurrentPathSegment = 0;
	}

	public virtual void TraversePath()
	{
		if ( Path == null )
			return;

		var distanceToTravel = move_speed;
		while ( distanceToTravel > 0 )
		{
			var currentTarget = Path[CurrentPathSegment];
			var distanceToCurrentTarget = Position.Distance( currentTarget );

			if ( distanceToCurrentTarget > distanceToTravel )
			{
				var direction = (currentTarget - Position).Normal;
				Position += direction * distanceToTravel;
				return;
			}
			else
			{
				var direction = (currentTarget - Position).Normal;
				Position += direction * distanceToCurrentTarget;
				distanceToTravel -= distanceToCurrentTarget;
				CurrentPathSegment++;
			}

			if ( CurrentPathSegment == Path.Length )
			{
				Path = null;
				move_speed = 0;
				return;				
			}
		}
	}
}
