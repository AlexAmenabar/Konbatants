using Godot;
using System;

public interface Ability
{
    public abstract void SetPlayer(PlayerController pPlayer);
    public abstract void Use();

    public abstract void UseSound();

    public abstract Texture2D GetTexture();
}
