using Godot;
using System;

/// <summary>
/// Attack that throws a punch. It is used to hit other players, but it can push ability cubes too.
/// Punch is thorwn in the direction player is looking.
/// </summary>
public partial class Punch : Attack
{
	public Punch()
	{
		InitializeValues(25, 30, 0, "Punch");
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeValues(25, 30, 0, "Punch");
	}

	public override void Use()
	{
		isUsed = true;
	
		// set player ability to null
		Player.Ability = null;

		// get player looking direction
		Vector2 playerLookDirection = new Vector2(Player.LookingHDir, Player.LookingVDir);

		// change punch position depending on player position
		GlobalPosition = new Vector3(Player.GlobalPosition.X + playerLookDirection.X * 1.5f, Player.GlobalPosition.Y + 1, Player.GlobalPosition.Z + playerLookDirection.Y);

		// activate collider
		GetNode("CollisionShape3D").SetDeferred("disabled", false);

		// quit Rigidbody3D freeze option
		Freeze = false;

		// visual detail
		Rotation = new Vector3(90, 0, 0);
		Visible = true;

		// to not follow player
		(this as Node3D).TopLevel = true;

		// calculate force in each dimension
		float forceX = 12, forceZ = 12;
		if (playerLookDirection.X != 0 && playerLookDirection.Y != 0) // Y is used as Z in this case, is a Vector2
			forceX = forceZ = (float)Math.Sqrt((forceX * forceX + forceZ * forceZ) / 2); // magnitude is forceX == forceZ, so calculate each dimension force

		ApplyImpulse(new Vector3(playerLookDirection.X * forceX, VerticalPushForce, playerLookDirection.Y * forceZ));

		UseSound();

		Finish(15); // after 15 seconds destroy the object
	}

	/// <summary>
	/// When it collides with a player call player TakeDamage and remove node.
	/// </summary>
	/// <param name="body">Body that collided</param>
	private void _on_body_entered(Node body)
	{
		if(body.IsInGroup("player"))
		{
			(body as PlayerController).TakeDamage(this);
			QueueFree();
		}
	}

	/// <summary>
	/// Destroys the node.
	/// </summary>
	/// <param name="time"></param>
	private async void Finish(float time)
	{
		await ToSignal(GetTree().CreateTimer(time), "timeout");
		QueueFree();
	}
}
