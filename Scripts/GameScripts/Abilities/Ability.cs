using Godot;
using System;

public interface Ability
{
    public abstract void SetPlayer(PlayerController pPlayer);
    public abstract void Use();
}
