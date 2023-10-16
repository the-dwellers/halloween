namespace MyGame;
using Sandbox;
using System.Linq;

public class TerryJr : TeenageTerry
{
	protected int sprintBar = 256;
	protected float sprintSpeed = 1.3f;
	const float MOVEMENT_SPEED = 5f;
	public override void  TraversePath()
	{
		if ( Path == null )
			return;
		
		var distanceToTravel = move_speed;
		while ( distanceToTravel > 0 )
		{
			if (sprintBar > 0){
				move_speed *= sprintSpeed;
			}
			else{
				move_speed = MOVEMENT_SPEED;
			}
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
				return;				
			}
		}
	}
}

