using Godot;
using System;

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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void Use()
	{
		// set player ability to null
		Player.Ability = null;

		Vector2 playerLookDirection = new Vector2(Player.LookingHDir, Player.LookingVDir);

		// change bomb position
		GlobalPosition = new Vector3(Player.GlobalPosition.X + playerLookDirection.X * 1.5f, Player.GlobalPosition.Y + 1, Player.GlobalPosition.Z + playerLookDirection.Y);

		// activate collider
		GetNode("CollisionShape3D").SetDeferred("disabled", false);

		// quit Rigidbody3D freeze option
		Freeze = false;

		Rotation = new Vector3(90, 0, 0);
		Visible = true;

		// to not follow player
		(this as Node3D).TopLevel = true;

		// calculate force in each dimension
		float forceX = 10, forceZ = 10;

		if (playerLookDirection.X != 0 && playerLookDirection.Y != 0)
		{
			float dimensionForce = (float)Math.Sqrt(50);
			forceX = forceZ = dimensionForce;
		}
		ApplyImpulse(new Vector3(playerLookDirection.X * forceX, VerticalPushForce, playerLookDirection.Y * forceZ));

		UseSound();
	}

	// when it collides with a player
	private void _on_body_entered(Node body)
	{
		if(body.IsInGroup("player"))
		{
			(body as PlayerController).TakeDamage(this);
			QueueFree();
		}
	}
}
