using Godot;
using System;

/// <summary>
/// This class is a specific type of attack. Bomb is thrown by player and after colliding with the floor or another player
/// it explodes.
/// </summary>
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

	public override void Use()
	{
		// set player ability to null (player can not use the same ability again)
		Player.Ability = null;
		Visible = true;

		// bomb must be thrown on the same direction as player looking direction
		Vector2 playerLookDirection = new Vector2(Player.LookingHDir, Player.LookingVDir);

		// change bomb position to initial position after throwing it (in front of player)
		GlobalPosition = new Vector3(Player.GlobalPosition.X + playerLookDirection.X * 1.5f, Player.GlobalPosition.Y + 1, Player.GlobalPosition.Z + playerLookDirection.Y);

		// activate collider
		GetNode("CollisionShape3D").SetDeferred("disabled", false);

		// quit Rigidbody3D freeze option
		Freeze = false;

		// node is child of PlayerController, so it follows player. That is not the expected behavior, so set as top level
		(this as Node3D).TopLevel = true;

		// calculate force in each dimension
		float forceX=7, forceZ=7;
		if(playerLookDirection.X != 0 && playerLookDirection.Y != 0) // Y is used as Z in this case, is a Vector2
			forceX = forceZ = (float)Math.Sqrt(49/2); // magnitude is 7, so calculate each dimension force

		ApplyImpulse(new Vector3(playerLookDirection.X * forceX, 15, playerLookDirection.Y * forceZ)); // lookDirection will set to 0 what is needed and control de direction

		UseSound();

		Finish(15); // after 15 seconds destroy the object (withouth this if it do not collide with any player or ground it will be there forever)
	}

	/// <summary>
	/// Signal emitted when bomb collides with a player or with the ground.
	/// </summary>
	/// <param name="body">Collision body</param>
	private void _on_body_entered(Node body)
	{
		if(body.IsInGroup("player") || body.IsInGroup("floor")) // if bomb collides with player or floor blow
		{
			// rigidbody stops detecting collisions
			Freeze = true;
			Rotation = new Vector3(0, 0, 0); // visual detail

			isUsed = true;

			// active explosion area
			(explosionArea.GetNode("CollisionShape3D") as CollisionShape3D).SetDeferred("disabled", false);

			// start explosion animation and active explosion visibility
			explosion.Visible = true;
			(explosion.GetNode("./AnimationPlayer") as AnimationPlayer).Play("Explosion");

			// play sound
			(GetNode("./UseSound") as AudioStreamPlayer3D).Stop();
			(GetNode("./ExplosionSound") as AudioStreamPlayer3D).Play();
			RefreshExplosionPosition();

			// after some seconds remove node
			Finish(3);
		}
	}

	/// <summary>
	/// destroys the object at finish
	/// </summary>
	/// <param name="time">Time before removing object</param>
	private async void Finish(float time)
	{
		await ToSignal(GetTree().CreateTimer(time), "timeout");
		QueueFree();
	}


	/// <summary>
	/// emitted when body entered to explosion area
	/// </summary>
	/// <param name="body"></param>
	private void _on_explosion_area_body_entered(Node3D body)
	{
		if(body.IsInGroup("player"))
			(body as PlayerController).TakeDamage(this);
	}

	/// <summary>
	/// Animation details.
	/// </summary>
	private async void RefreshExplosionPosition()
	{
		while(true)
		{ 
			await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
			explosion.Position -= new Vector3(0, 0.1f, 0);
		}
	}
}
