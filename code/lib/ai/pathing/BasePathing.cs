namespace lib.Ai.Pathing;

/// <summary>
/// Return a direction to get between two vectors. May include pathing around
/// objects, etc.
/// </summary>
public abstract class BasePathing
{
	public Vector3 TargetPosition;

	public abstract void Update( Vector3 from, Vector3 to );

	public abstract Vector3 GetDirection( Vector3 position );
}
