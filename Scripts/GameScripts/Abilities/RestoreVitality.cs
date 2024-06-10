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
        throw new NotImplementedException();
    }
}
