using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Ability cube in which abilities are stored.
/// </summary>
public partial class AbilityCube : RigidBody3D
{
	String[] abilitySceneNames;
	String childName;


	/// <summary>
	/// Spawn ability node and set as cube child
	/// </summary>
	/// <param name="abilityIndex">Index of the ability to spawn</param>
	public void GenerateAbility(int abilityIndex)
	{
		abilitySceneNames = new String[8];
		abilitySceneNames[0] = "Bomb";
		abilitySceneNames[1] = "Football";
		abilitySceneNames[2] = "RestoreVitality";
		abilitySceneNames[3] = "Speed";
		abilitySceneNames[4] = "Invisibility";
		abilitySceneNames[5] = "Punch";
		abilitySceneNames[6] = "Stole";
		abilitySceneNames[7] = "MagicalShield";

		childName = abilitySceneNames[abilityIndex];

		// instantiate ability and set as a child of the ability cube
		var abilityScene = ResourceLoader.Load<PackedScene>("res://Scenes/GameScenes/AbilityScenes/" + childName + ".tscn").Instantiate<Node3D>();
		AddChild(abilityScene);
	}

	/// <summary>
	/// return child node (ability)
	/// </summary>
	/// <returns>Child node, node of the ability</returns>
	public Node3D GetAbility()
	{
		return (Node3D)GetNode(childName);
	}

	/// <summary>
	/// Destroys the ability cube
	/// </summary>
	public void Destroy()
	{
		QueueFree();
	}
}
