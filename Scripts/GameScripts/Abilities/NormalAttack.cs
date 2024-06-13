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
		await ToSignal(GetTree().CreateTimer(timeToDesactivate), "timeout");
		//(this as Node3D).Visible = false;
		Player.Attacked = false;

		collider.SetDeferred("disabled", true);
		Freeze = true;

		Reactivate();
	}

	public async void Reactivate()
	{
		await ToSignal(GetTree().CreateTimer(activationTime), "timeout");
		canUse = true;
	}

	private void _on_body_entered(Node body)
	{
		if (body.IsInGroup("player"))
		{
			PlayerController enemyPlayer = body as PlayerController;
			if(enemyPlayer.Team != Player.Team)
			{ 
				enemyPlayer.TakeDamage(this);
				Desactivate(0);
			}
		}
	}
}
