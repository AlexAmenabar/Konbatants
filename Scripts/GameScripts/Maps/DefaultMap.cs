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
		throw new NotImplementedException();
	}
}
