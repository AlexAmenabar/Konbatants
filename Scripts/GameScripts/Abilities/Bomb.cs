using Godot;
using System;

public partial class Bomb : Attack
{
    public Bomb()
    {
        InitializeValues(20, new Vector3(20, 20, 20));
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
        throw new NotImplementedException();
    }
}
