using Godot;
using System;

public partial class Invisibility : Buff
{
	public Invisibility()
	{
		InitializeValues(0, 4);
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeValues(0, 4);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public override void Use()
	{
		// player has used the ability
		player.Ability = null;

		// player who is controlling this must see his model
		if(!player.IsMultiplayerAuthority())
		{ 
			(player.GetNode("3DGodotRobot") as Node3D).Visible = false;
			(player.GetNode("Arrow") as Node3D).Visible = false;
		}

		// play sound
		UseSound();

		//player.AddChild(this);

		// after duration reset visibility
		ResetVisibility();
	}

	public async void ResetVisibility()
	{
		await ToSignal(GetTree().CreateTimer(duration), "timeout");
		(player.GetNode("3DGodotRobot") as Node3D).Visible = true;
		(player.GetNode("Arrow") as Node3D).Visible = true; 
		QueueFree();
	}
}
