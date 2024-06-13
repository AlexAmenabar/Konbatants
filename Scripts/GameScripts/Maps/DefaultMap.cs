using Godot;
using System;

public partial class DefaultMap : Map
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeMap();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void InitializeMap()
	{
		// Initialize music
		GetNode("./MapMusic").Set("playing", true);
	}

	public override void FinalMapEvent()
	{
		GD.Print("Final event running...");
	}

	public override Vector3 GetCubePosition()
	{
		// from x = 2 to x = 22 and z = 2 to z = 22
		Random rnd = new Random();
		return new Vector3(rnd.Next(2,22), 30, rnd.Next(2,22));
	}
}
