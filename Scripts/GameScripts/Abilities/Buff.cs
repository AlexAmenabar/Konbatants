using Godot;
using System;

/// <summary>
/// This type of ability is not directly spawned on scene as visual objects. It gives some advantage to player that use it
/// as healing, movement speed, invisibility... Buff finish after some seconds.
/// </summary>
public abstract partial class Buff : Node3D, Ability
{
    protected PlayerController player;
    protected int bufValue;
    protected int duration;
    protected bool isUsed;

    /// <summary>
    /// Intitialize object values.
    /// </summary>
    /// <param name="pBufValue">Value of the buff</param>
    /// <param name="pDuration">Buff duration</param>
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

    public bool AbilityIsUsed()
    {
        return isUsed;
    }
}
