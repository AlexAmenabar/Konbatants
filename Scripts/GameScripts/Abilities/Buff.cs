using Godot;
using System;

public abstract partial class Buff : Node, Ability
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
}
