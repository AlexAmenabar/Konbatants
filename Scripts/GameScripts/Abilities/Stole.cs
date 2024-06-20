using Godot;
using System;

/// <summary>
/// It creates an area in which if there is another player, player ability is stolen.
/// </summary>
public partial class Stole : Interaction
{
	public override void Use()
	{
		isUsed = true;

		player.Ability = null;
		Position = new Vector3(0, 0.8f, 1);
		UseSound();

		// activate collider
		GetNode("./CollisionShape3D").SetDeferred("disabled", false);

		Finish();
	}

	/// <summary>
	/// After some seconds of being activated node is removed.
	/// </summary>
	private async void Finish()
	{
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		QueueFree();
	}

	/// <summary>
	/// If a player enters in the area, ability is stolen.
	/// </summary>
	/// <param name="body"></param>
	private void _on_body_entered(Node3D body)
	{
		if(body.IsInGroup("player"))
		{
			GetNode("./CollisionShape3D").SetDeferred("disabled", true);
			PlayerController enemyPlayer = body as PlayerController;
			if(enemyPlayer.Team != player.Team && enemyPlayer.Ability != null)
				StoleAbility(enemyPlayer);
		}
	}

	/// <summary>
	/// Stole the ability, changing player ability, texture and child.
	/// </summary>
	/// <param name="enemyPlayer">Enemy player to stole the ability</param>
	private async void StoleAbility(PlayerController enemyPlayer)
	{
		// wait, if ability is stolen directly player uses it because ability button is pressed
		await ToSignal(GetTree().CreateTimer(0.4f), "timeout");

		// pass ability from enemy player to this player
		enemyPlayer.Ability.Reparent(player.GetNode("./Ability"));
		player.GetNode("./Ability").MoveChild(enemyPlayer.Ability, 0);
		player.Ability = (player.GetNode("./Ability").GetChild(0) as Node3D);
		(player.Ability as Ability).SetPlayer(player);
		enemyPlayer.Ability = null;

		// refresh GUI
		player.PlayerGUIController.SetAbilityTexture((player.Ability as Ability).GetTexture());
		enemyPlayer.PlayerGUIController.SetAbilityTexture(null);
	}
}
