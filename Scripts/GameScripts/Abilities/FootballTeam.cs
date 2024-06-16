using Godot;
using System;

public partial class FootballTeam : Attack
{
	private Vector3 linearVelocity;

	public FootballTeam()
	{
		InitializeValues(0, 0, 0, "Football");
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeValues(0, 0, 0, "Football");
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

		Visible = true;

		// to not follow player
		(this as Node3D).TopLevel = true;

		// calculate force in each dimension
		float forceX = 15, forceZ = 15;

		if (playerLookDirection.X != 0 && playerLookDirection.Y != 0)
		{
			float dimensionForce = (float)Math.Sqrt(50);
			forceX = forceZ = dimensionForce;
		}
		//ApplyImpulse(new Vector3(playerLookDirection.X * forceX, 0, playerLookDirection.Y * forceZ));
		linearVelocity = new Vector3(20 * playerLookDirection.X, 0, 20 * playerLookDirection.Y);
		LinearVelocity = linearVelocity;

		UseSound();

		KeepMoving();
	}

	private async void KeepMoving()
	{
		while(true)
		{
			await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
			LinearVelocity = linearVelocity;
		}
	}
}
