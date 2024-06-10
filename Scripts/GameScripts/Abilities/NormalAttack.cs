using Godot;
using System;

public partial class NormalAttack : Attack
{

    public NormalAttack()
    {
        InitializeValues(3, new Vector3(5, 0, 5));
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		InitializeValues(3, new Vector3(10, 0, 10));
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
