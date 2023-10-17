using System.Linq;
using Sandbox;
namespace MyGame;
using lib.Ai.Steer;

public enum AIState
{
	IDLE,
	FLEE,
	WANDER
}

public partial class BaseNPC : AnimatedEntity
{
	public float BaseSpeed { get; set; } = 200f;
	public float SpeedMultiplier = 1f;

	protected Vector3 WishVelocity = Vector3.Zero;

	public BaseSteer Steer;

	public AIState State;

	private TimeSince LastTargetUpdate;

	private Pawn TargetPlayer;

	private const float ChaseDistance = 600f;

	public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/citizen/citizen.vmdl" );

		// initial state is idle
		State = AIState.IDLE;
	}

	public void UpdateTarget()
	{
		// update closest player
		TargetPlayer =  All.OfType<Pawn>()
			.OrderByDescending( x => x.Position.Distance( Position ) )
			.FirstOrDefault();

		// also update the steer
		if ( Steer is AvoidSteer steer )
		{
			steer.SetAvoidTarget(TargetPlayer);
		}
	}

	[GameEvent.Tick]
	public void Tick()
	{
		if ( Game.IsClient )
		{
			return;
		}
		UpdateTarget();

		if (Position.Distance(TargetPlayer.Position) < ChaseDistance)
		{
			Log.Info(Position.Distance(TargetPlayer.Position));
			State = AIState.FLEE;
		} else {
			State = AIState.IDLE;
		}

		// State machine
		switch ( State )
		{
			case AIState.WANDER:
				// Wander around
				if (Steer is not WanderSteer)
				{
					Steer = new WanderSteer();
				}
				break;
			case AIState.FLEE:
				// Flee from player
				if (Steer is not AvoidSteer)
				{
					Steer = new AvoidSteer();
					UpdateTarget();
				}

				break;
			default:
			case AIState.IDLE:
				// Stand still
				State = AIState.IDLE;
				Steer = null;
				break;
		}

		// Update AI path
		if ( Steer != null )
		{
			if ( LastTargetUpdate > 1 )
			{
				// Update player every second
				LastTargetUpdate = 0;
				UpdateTarget();
			}

			// Run pathing tick
			Steer.Tick( Position );

			if ( Steer.Output.Finished )
			{
				// nav is finished...
				WishVelocity = Vector3.Zero;
				Velocity = Vector3.Zero;
			}
			else
			{
				// update velocity based on pathing
				WishVelocity = Steer.Output.Direction.Normal * BaseSpeed;
				Velocity = Velocity.AddClamped( WishVelocity, BaseSpeed * SpeedMultiplier );
			}
		} else {
			// no AI, so stand still...
			WishVelocity = Vector3.Zero;
			Velocity = Vector3.Zero;
		}

		// Update movement based on velocity
		Move( Time.Delta );
		var walkVelocity = Velocity.WithZ( 0 );
		if ( walkVelocity.Length > 0.5f )
		{
			var turnSpeed = walkVelocity.Length.LerpInverse( 0, 100 );
			var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );
			Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 20.0f );
		}

		// Update animation on final velocity and direction
		NPCAnimationHelper animHelper = new( this );
		animHelper.WithVelocity( Velocity );
		animHelper.WithWishVelocity( WishVelocity );
	}

	protected void Move( float timeDelta )
	{
		var bbox = BBox.FromHeightAndRadius( 16, 4 );

		MoveHelper move = new( Position, Velocity ) { MaxStandableAngle = 50 };
		move.Trace = move.Trace.Ignore( this ).Size( bbox );

		if ( !Velocity.IsNearlyZero( 0.001f ) )
		{
			move.TryUnstuck();
			move.TryMoveWithStep( timeDelta, 30 );
		}

		var tr = move.TraceDirection( Vector3.Down * 10.0f );

		if ( move.IsFloor( tr ) )
		{
			GroundEntity = tr.Entity;

			if ( !tr.StartedSolid )
			{
				move.Position = tr.EndPosition;
			}

			if ( WishVelocity.Length > 0 )
			{
				var movement = move.Velocity.Dot( WishVelocity.Normal );
				move.Velocity -= movement * WishVelocity.Normal;
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
				move.Velocity += movement * WishVelocity.Normal;
			}
			else
			{
				move.ApplyFriction( tr.Surface.Friction * 1.0f, timeDelta );
			}
		}
		else
		{
			GroundEntity = null;
			move.Velocity += Vector3.Down * 900 * timeDelta;
		}

		Position = move.Position;
		Velocity = move.Velocity;
	}

}
