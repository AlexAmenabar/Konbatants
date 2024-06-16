using Godot;
using System;

public partial class Stole : Interaction
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
		player.Ability = null;
		Position = new Vector3(0, 0.8f, 1);
		UseSound();

		// activate collider
		GetNode("./CollisionShape3D").SetDeferred("disabled", false);

		Finish();
	}

	private async void Finish()
	{
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		QueueFree();
	}

	private void _on_body_entered(Node3D body)
	{
		if(body.IsInGroup("player"))
		{
			GetNode("./CollisionShape3D").SetDeferred("disabled", true);
			PlayerController enemyPlayer = body as PlayerController;
			if(enemyPlayer.Team != player.Team && enemyPlayer.Ability != null)
			{
				StoleAbility(enemyPlayer);
			}
		}
	}

	private async void StoleAbility(PlayerController enemyPlayer)
	{
		GD.Print("Stoling ability");
		// wait, if ability is stolen directly player uses it because ability button is pressed
		await ToSignal(GetTree().CreateTimer(0.4f), "timeout");
		// pass ability to player
		enemyPlayer.Ability.Reparent(player.GetNode("./Ability"));
		player.Ability = (player.GetNode("./Ability").GetChild(0) as Node3D);
		(player.Ability as Ability).SetPlayer(player);
		enemyPlayer.Ability = null;

		GD.Print("Ability stoled");

		// refresh GUI
		player.PlayerGUIController.SetAbilityTexture((player.Ability as Ability).GetTexture());
		enemyPlayer.PlayerGUIController.SetAbilityTexture(null);
	}
}
