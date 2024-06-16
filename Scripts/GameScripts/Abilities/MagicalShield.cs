using Godot;
using System;

public partial class MagicalShield : Interaction
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void Use()
	{
		Position = new Vector3(0, 1, 0);
		player.Ability = null;
		UseSound();
		Visible = true;
		player.Can = false;

		// activate collider
		GetNode("./CollisionShape3D").SetDeferred("disabled", false);

		Finish();
	}

	private void _on_body_entered(Node3D body)
	{
		if (body.IsInGroup("Attack"))
		{
			// rebound ability
			Attack attack = body as Attack;

			// now ability parent is this player, change and return
			attack.Player = player;
			attack.ApplyImpulse(new Vector3(-attack.LinearVelocity.X, -attack.LinearVelocity.Y, -attack.LinearVelocity.Z));

			// destroy after colliding with something
			Destroy();
		}
		else if(body.IsInGroup("player"))
		{
			// push player
			PushEnemyPlayer(body as PlayerController);
		}
	}

	public async void Finish()
	{
		await ToSignal(GetTree().CreateTimer(3), "timeout");
		Destroy();
	}

	public async void PushEnemyPlayer(PlayerController enemyPlayer)
	{
		enemyPlayer.Can = false;
		enemyPlayer.ApplyImpulse(new Vector3(-enemyPlayer.LinearVelocity.X * 1.5f, 0, -enemyPlayer.LinearVelocity.Z * 1.5f));

		await ToSignal(GetTree().CreateTimer(1), "timeout");
		enemyPlayer.Can = true;
	}
	public void Destroy()
	{
		player.Can = true;
		QueueFree();
	}
}
