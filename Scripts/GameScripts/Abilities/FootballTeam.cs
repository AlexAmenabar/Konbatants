using Godot;
using System;

/// <summary>
/// This ability spawns a ballon that does not do damage but it pushes players and ability cubes
/// </summary>
public partial class FootballTeam : Attack
{
	public FootballTeam()
	{
		InitializeValues(0, 0, 0, "Football");
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeValues(0, 0, 0, "Football");
	}

	public override void Use()
	{
		isUsed = true;

		// set player ability to null
		Player.Ability = null;

		// get player looking direction
		Vector2 playerLookDirection = new Vector2(Player.LookingHDir, Player.LookingVDir);

		// change ball position depending on player position
		GlobalPosition = new Vector3(Player.GlobalPosition.X + playerLookDirection.X * 1.5f, Player.GlobalPosition.Y + 1, Player.GlobalPosition.Z + playerLookDirection.Y);

		// activate collider
		GetNode("CollisionShape3D").SetDeferred("disabled", false);

		// quit Rigidbody3D freeze option
		Freeze = false;

		// nake ability visible
		Visible = true;

		// do not follow player
		(this as Node3D).TopLevel = true;

		// calculate force in each dimension
		float forceX = 12, forceZ = 12;
		if (playerLookDirection.X != 0 && playerLookDirection.Y != 0) // Y is used as Z in this case, is a Vector2
			forceX = forceZ = (float)Math.Sqrt((12*12 + 12*12) / 2); // magnitude is 7, so calculate each dimension force

		// set as linear velocity		
		Vector3 linearVelocity = new Vector3(forceX * playerLookDirection.X, 0, forceZ * playerLookDirection.Y);
		LinearVelocity = linearVelocity;

		UseSound();

		// keep linear velocity
		KeepMoving(linearVelocity);

		Finish(15); // after 15 seconds destory the object
	}

	/// <summary>
	/// Body lose velocity when it collides with another objets, so instead object is active maintain that velocity
	/// </summary>
	/// <param name="linearVelocity"></param>
	private async void KeepMoving(Vector3 linearVelocity)
	{
		while(true)
		{
			await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
			LinearVelocity = new Vector3(linearVelocity.X, LinearVelocity.Y, linearVelocity.Z);
		}
	}
	/// <summary>
	/// destroys the node at finish
	/// </summary>
	/// <param name="time">Time before destroying the node</param>
	private async void Finish(float time)
	{
		await ToSignal(GetTree().CreateTimer(time), "timeout");
		//explosion.Visible = false;

		QueueFree();
	}

}
