namespace lib.Ai.Steer;

/// <summary>
/// Standard AI pathing. Takes in a current position and returns a wish direction.
/// </summary>
public abstract class BaseSteer
{
	protected Vector3 LastPosition;

	public abstract void Tick( Vector3 currentPosition );

	public Vector3 Target { get; set; }

	public NavSteerOutput Output;

}

public struct NavSteerOutput
{
	public bool Finished;
	public Vector3 Direction;
	public int CurrentNode;
	public int NextNode;
}
