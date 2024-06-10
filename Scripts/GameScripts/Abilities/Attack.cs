using Godot;
using System;

public abstract partial class Attack : Node, Ability 
{
	protected PlayerController player;
	protected int damage;
	protected Vector3 pushForce;

	public void InitializeValues(int pDamage, Vector3 pPushForce)
	{
		damage = pDamage;
		pushForce = pPushForce;
	}
	public void SetPlayer(PlayerController pPlayer)
	{
		player = pPlayer;
	}
	public abstract void Use();
}
