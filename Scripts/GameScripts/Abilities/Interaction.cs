using Godot;
using System;

/// <summary>
/// This type of ability are spawned in the map and interact is different ways between players. 
/// </summary>
public abstract partial class Interaction : Area3D, Ability
{
    protected PlayerController player;
    protected bool isUsed;

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

    public bool AbilityIsUsed()
    {
        return isUsed;
    }
}
