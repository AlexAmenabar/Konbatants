using Godot;
using System;

/// <summary>
/// Player that use this ability will be invisible for some seconds.
/// </summary>
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

	public override void Use()
	{
		isUsed = true;

		// player has used the ability
		player.Ability = null;

		// player who is controlling this must see his model, so become invisible only in other players (feedback to invisible player?)
		if(!player.IsMultiplayerAuthority())
		{ 
			(player.GetNode("3DGodotRobot") as Node3D).Visible = false;
			(player.GetNode("Arrow") as Node3D).Visible = false;
		}

		// play sound
		UseSound();

		// after duration reset visibility
		ResetVisibility();
	}

	/// <summary>
	/// Set visible to true when buff time finish.
	/// </summary>
	public async void ResetVisibility()
	{
		await ToSignal(GetTree().CreateTimer(duration), "timeout");
		(player.GetNode("3DGodotRobot") as Node3D).Visible = true;
		(player.GetNode("Arrow") as Node3D).Visible = true; 
		QueueFree();
	}
}
