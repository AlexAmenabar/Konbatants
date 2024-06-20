using Godot;
using System;

/// <summary>
/// A special type of attack. It is an ability to use it as the rest, but player can always use it and it is not spawned on ability cubes.
/// </summary>
public partial class NormalAttack : Attack
{
	// area to collide
	private CollisionShape3D collider;
	private int activationTime; // time between uses
	private bool canUse; // set true when activationTime passes


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Player = (PlayerController)GetNode("../");
		InitializeValues(3, 10, 0, ""); // not necesary scene path, it is an especial case

		// not active at start
		collider = (CollisionShape3D)GetNode("./AttackCollisionShape");
		collider.SetDeferred("disabled", true);

		activationTime = 2; // player has to wait two seconds to use this ability after using it
		canUse = true;

		// start freezed
		Freeze = true;
	}

	public override void _Process(double delta)
	{
		// refresh position always locally to player
		if (Player.Attacked)
		{
			Position = new Vector3(0, 1, 1);
			Rotation = new Vector3(0, 0, 0);
		}
	}

	public override void Use()
	{
		if(canUse)
		{
			canUse = false;
			Position = new Vector3(0, 1, 1);
			Rotation = new Vector3(0, 0, 0);
			collider.SetDeferred("disabled", false);
			Freeze = false;
			Desactivate(0.5f);

			// player attack animation
			Player.Attacked = true;

			// play sound
			(Player.SoundNodes.GetNode("./MeleeAttackSound") as AudioStreamPlayer3D).Play();
		}
	}

	/// <summary>
	/// After some time active attack is deactivated.
	/// </summary>
	/// <param name="timeToDesactivate"></param>
	public async void Desactivate(float timeToDesactivate)
	{
		await ToSignal(GetTree().CreateTimer(timeToDesactivate - 0.5f), "timeout");

		Player.Attacked = false;

		collider.SetDeferred("disabled", true);
		Freeze = true;

		// activate in another place to solve a bug
		await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
		Position = new Vector3(100, 100, 100);
		Freeze = false;
		collider.SetDeferred("disabled", false);

		await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
		Freeze = true;
		collider.SetDeferred("disabled", true);
		// buf solving finalized

		// start reactivation timer
		Reactivate();
	}

	/// <summary>
	/// This function gives player the opportunity to attack again after activation time.
	/// </summary>
	public async void Reactivate()
	{
		await ToSignal(GetTree().CreateTimer(activationTime), "timeout");
		canUse = true;
	}

	/// <summary>
	/// When attack collides with a player of another team, push player and do damage.
	/// </summary>
	/// <param name="body_rid"></param>
	/// <param name="body"></param>
	/// <param name="body_shape_index"></param>
	/// <param name="local_shape_index"></param>
	private void _on_body_shape_entered(Rid body_rid, Node body, long body_shape_index, long local_shape_index)
	{
		if (body.IsInGroup("player"))
		{
			PlayerController enemyPlayer = body as PlayerController;
			if (enemyPlayer.Team != Player.Team) // if teams are different do damage
			{
				enemyPlayer.TakeDamage(this);
				Desactivate(0);
			}
		}
	}
}
