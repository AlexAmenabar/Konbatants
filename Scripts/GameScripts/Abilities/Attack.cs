using Godot;
using System;

/// <summary>
/// This type of ability is used to push and do damage to other players. This abilities are spawned and are visible on the game.
/// There are destroyed after hitting something or after some time.
/// </summary>
public abstract partial class Attack : RigidBody3D, Ability 
{
    private PlayerController player;
    private int damage;
    private int horizontalPushForce;
    private int verticalPushForce;
	protected String scenePath;
	protected bool isUsed;

    public int Damage { get => damage; set => damage = value; }
    public PlayerController Player { get => player; set => player = value; }
    public int HorizontalPushForce { get => horizontalPushForce; set => horizontalPushForce = value; }
    public int VerticalPushForce { get => verticalPushForce; set => verticalPushForce = value; }

	/// <summary>
	/// Initialize attack values
	/// </summary>
	/// <param name="pDamage">Damage of this attack</param>
	/// <param name="pHorizontalForce">Push in horizontal directions</param>
	/// <param name="pVerticalForce">Push in vertical direction</param>
	/// <param name="pScenePath">Name of the scene to instantiate it when is created</param>
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

    /// <summary>
	/// Load node and return it.
	/// </summary>
	/// <returns>Node of the ability</returns>
    public Node3D Instantiate()
	{
        Node3D ability = ResourceLoader.Load<PackedScene>("res://Scenes/GameScenes/AbilityScenes/" + scenePath + ".tscn").Instantiate<Node3D>();
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

    public bool AbilityIsUsed()
    {
		return isUsed;
    }
}
