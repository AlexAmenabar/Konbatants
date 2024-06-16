using Godot;
using System;

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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void Use()
	{
		// player has used the ability, set null
		player.Ability = null;

		//GD.Print("Speed buf");
		player.Speed = playerOriginalSpeed + bufValue;

		// play sound
		UseSound();

		//player.AddChild(this);

		// after duration reset speed
		ResetSpeed(playerOriginalSpeed);
	}


	public async void ResetSpeed(int playerOriginalSpeed)
	{
		// await until buf time finish
		await ToSignal(GetTree().CreateTimer(duration), "timeout");
		player.Speed = playerOriginalSpeed;
		//GD.Print("Setting player original speed (" + playerOriginalSpeed.ToString() +")");
		QueueFree();
	}
}
