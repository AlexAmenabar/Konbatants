using Godot;
using System;

/// <summary>
/// Generates a shield that protects player from another attack. When an attack collides with a shield attack
/// will be rebound.
/// Shield dissapears after some seconds or when an attack collides with it.
/// </summary>
public partial class MagicalShield : Interaction
{
	public override void Use()
	{
		isUsed = true;

		Position = new Vector3(0, 1, 0);
		player.Ability = null;
		UseSound();
		Visible = true;
		player.Can = false;

		// activate collider
		GetNode("./CollisionShape3D").SetDeferred("disabled", false);

		Finish();
	}

	/// <summary>
	/// Signal emtted when body collides with the shield.
	/// </summary>
	/// <param name="body"></param>
	private void _on_body_entered(Node3D body)
	{
		// if body is an attack rebound it (an ability owner will be shield owner)
		if (body.IsInGroup("Attack"))
		{
			// rebound ability
			Attack attack = body as Attack;

			// now ability parent is this player, change and return
			attack.Player = player;
			attack.ApplyImpulse(new Vector3(-attack.LinearVelocity.X * 2f, -attack.LinearVelocity.Y * 2f, -attack.LinearVelocity.Z * 2f));

			// destroy after colliding with something
			player.Can = true;
			QueueFree();
		}
		// if it is a player push it
		else if(body.IsInGroup("player"))
		{
			if((body as PlayerController).Id != player.Id)
				// push player
				PushEnemyPlayer(body as PlayerController);
		}
	}

	/// <summary>
	/// After some seconds shield dissapears.
	/// </summary>
	public async void Finish()
	{
		await ToSignal(GetTree().CreateTimer(3), "timeout");
		player.Can = true;
		QueueFree();
	}

	/// <summary>
	/// If player collides with the shield push it.
	/// </summary>
	/// <param name="enemyPlayer"></param>
	public async void PushEnemyPlayer(PlayerController enemyPlayer)
	{
		enemyPlayer.Can = false;
		enemyPlayer.ApplyImpulse(new Vector3(-enemyPlayer.LookingHDir * 500f, 0, -enemyPlayer.LookingVDir * 500f));
		await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
		enemyPlayer.Can = true;
	}
}
