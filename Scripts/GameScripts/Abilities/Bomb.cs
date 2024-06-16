using Godot;
using System;

public partial class Bomb : Attack
{
	private Node3D explosion;
	private Area3D explosionArea;

	public Bomb()
	{
		InitializeValues(20, 50, 20, "Bomb");
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeValues(20, 50, 20, "Bomb");

		explosion = (Node3D)GetNode("./Explosion");
		explosionArea = (Area3D)GetNode("./ExplosionArea");
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

		// to not follow player
		(this as Node3D).TopLevel = true;

		// calculate force in each dimension
		float forceX = 10, forceZ = 10;

		if(playerLookDirection.X != 0 && playerLookDirection.Y != 0)
		{
			float dimensionForce = (float)Math.Sqrt(50);
			forceX = forceZ = dimensionForce;
		}
		ApplyImpulse(new Vector3(playerLookDirection.X * forceX, 15, playerLookDirection.Y * forceZ));

		UseSound();
	}

	private void _on_body_entered(Node body)
	{
		if(body.IsInGroup("player") || body.IsInGroup("floor")) // if bomb collides with player or floor blow
		{
			// rigidbody stop detecting collisions
			Freeze = true;
			Rotation = new Vector3(0, 0, 0);

			// active explosion area
			(explosionArea.GetNode("CollisionShape3D") as CollisionShape3D).SetDeferred("disabled", false);

			// start explosion animation and active explosion visibility
			explosion.Visible = true;
			(explosion.GetNode("./AnimationPlayer") as AnimationPlayer).Play("Explosion");

			// play sound
			(GetNode("./UseSound") as AudioStreamPlayer3D).Stop();
			(GetNode("./ExplosionSound") as AudioStreamPlayer3D).Play();
			RefreshExplosionPosition();

			Finish();
		}
	}

	// destroys the object at finish
	private async void Finish()
	{
		await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
		//explosion.Visible = false;

		QueueFree();
	}


	// emitted when body entered to explosion area
	private void _on_explosion_area_body_entered(Node3D body)
	{
		if(body.IsInGroup("player"))
			(body as PlayerController).TakeDamage(this);
	}

	private async void RefreshExplosionPosition()
	{
		while(true)
		{ 
			await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
			explosion.Position -= new Vector3(0, 0.1f, 0);
		}
	}
}
