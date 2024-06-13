using Godot;
using System;

public abstract partial class Attack : RigidBody3D, Ability 
{
    private PlayerController player;
    private int damage;
    private int horizontalPushForce;
    private int verticalPushForce;
	protected String scenePath;

    public int Damage { get => damage; set => damage = value; }
    public PlayerController Player { get => player; set => player = value; }
    public int HorizontalPushForce { get => horizontalPushForce; set => horizontalPushForce = value; }
    public int VerticalPushForce { get => verticalPushForce; set => verticalPushForce = value; }

    public void InitializeValues(int pDamage, int pHorizontalForce, int pVerticalForce, String pScenePath)
	{
		damage = pDamage;
		horizontalPushForce = pHorizontalForce;
		verticalPushForce = pVerticalForce;
		scenePath = pScenePath;
	}
	public void SetPlayer(PlayerController pPlayer)
	{
		player = pPlayer;
	}
	public abstract void Use();

    // load scene node
    public Node3D Instantiate()
	{
		GD.Print("Instantiating Bomb");
        Node3D ability = ResourceLoader.Load<PackedScene>("res://Scenes/GameScenes/AbilityScenes/" + scenePath + ".tscn").Instantiate<Node3D>();
		GD.Print("Ability instantiated");
		GD.Print(ability == null);
		GD.Print(ability.Name);
		return ability;
	}

    public void UseSound()
	{
		(GetNode("./UseSound") as AudioStreamPlayer3D).Play();
	}
    public Texture2D GetTexture()
	{
		return (GetNode("./Image") as TextureRect).Texture;
	}
}
