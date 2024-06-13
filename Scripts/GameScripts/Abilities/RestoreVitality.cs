using Godot;
using System;

public partial class RestoreVitality : Buff
{
	public RestoreVitality()
	{
		InitializeValues(20, 0); // no duration
	}
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
