using Godot;
using System;

/// <summary>
/// Makes player faster during some seconds.
/// </summary>
public partial class Speed : Buff
{
	int playerOriginalSpeed = 4;
	public Speed()
	{
		InitializeValues(4, 5); // 5 seconds
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeValues(4, 5); // 5 seconds
	}
	public override void Use()
	{
		isUsed = true;

		// player has used the ability, set null
		player.Ability = null;

		//GD.Print("Speed buf");
		player.Speed = playerOriginalSpeed + bufValue;

		// play sound
		UseSound();

		// after duration reset speed
		ResetSpeed();
	}

	/// <summary>
	/// Reset player speed to initial value after buff effect is finished.
	/// </summary>
	/// <param name="playerOriginalSpeed"></param>
	public async void ResetSpeed()
	{
		await ToSignal(GetTree().CreateTimer(duration), "timeout");
		player.Speed = playerOriginalSpeed;
		QueueFree();
	}
}
