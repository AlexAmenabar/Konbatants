using Godot;
using System;

public abstract partial class Interaction : Area3D, Ability
{
    protected PlayerController player;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void SetPlayer(PlayerController pPlayer)
    {
        player = pPlayer;
    }

    public abstract void Use();

    public void UseSound()
    {
        (GetNode("./UseSound") as AudioStreamPlayer3D).Play();
    }

    public Texture2D GetTexture()
    {
        return (GetNode("./Image") as TextureRect).Texture;
    }
}
