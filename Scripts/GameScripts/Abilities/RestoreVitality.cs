using Godot;
using System;

/// <summary>
/// Restore player vitality (partially).
/// </summary>
public partial class RestoreVitality : Buff
{
	public RestoreVitality()
	{
		InitializeValues(20, 0); // no duration
	}

	public override void Use()
	{
		isUsed = true;

		// player has used the ability, quit
		player.Ability = null;

		UseSound();

		player.Vitality += bufValue;
		if (player.Vitality > player.MaxVitality)
			player.Vitality = player.MaxVitality;

		player.RefreshLifeBar();

		// destroy node
		QueueFree();
	}
}
