using Godot;
using System;

public abstract partial class Buff : Node3D, Ability
{
    protected PlayerController player;
    protected int bufValue;
    protected int duration;

    public void InitializeValues(int pBufValue, int pDuration)
    {
        bufValue = pBufValue;
        duration = pDuration;
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
