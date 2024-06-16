using Godot;
using System;

public partial class NormalAttack : Attack
{
	private CollisionShape3D collider;
	private int activationTime; // time between uses
	private bool canUse;

	/*public NormalAttack()
	{
		InitializeValues(3, 10, 0);
	}*/
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
			//(this as Node3D).Visible = true;
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

	public async void Desactivate(float timeToDesactivate)
	{
		await ToSignal(GetTree().CreateTimer(timeToDesactivate - 0.5f), "timeout");
		//(this as Node3D).Visible = false;
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

		Reactivate();
	}

	public async void Reactivate()
	{
		await ToSignal(GetTree().CreateTimer(activationTime), "timeout");
		canUse = true;
	}

	private void _on_body_entered(Node body)
	{
		/*if (body.IsInGroup("player"))
		{
			PlayerController enemyPlayer = body as PlayerController;
			if(enemyPlayer.Team != Player.Team)
			{ 
				enemyPlayer.TakeDamage(this);
				Desactivate(0);
			}
		}*/
	}

	private void _on_body_shape_entered(Rid body_rid, Node body, long body_shape_index, long local_shape_index)
	{
		if (body.IsInGroup("player"))
		{
			PlayerController enemyPlayer = body as PlayerController;
			if (enemyPlayer.Team != Player.Team)
			{
				enemyPlayer.TakeDamage(this);
				Desactivate(0);
			}
		}
	}
}
