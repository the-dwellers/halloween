using Sandbox;
using System;

namespace MyGame;

public class TTAnimator : EntityComponent<TeenageTerry>, ISingletonComponent
{
	public void Tick()
	{
		var helper = new CitizenAnimationHelper( Entity);
		helper.WithVelocity( Entity.Velocity );
		helper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		helper.IsGrounded = Entity.GroundEntity.IsValid();
		helper.WithLookAt(Entity.Velocity);
	}
}

