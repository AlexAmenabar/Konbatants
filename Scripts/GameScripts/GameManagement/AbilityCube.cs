using Godot;
using System;
using System.Collections.Generic;

public partial class AbilityCube : RigidBody3D
{
	private Ability ability;
	String[] abilitySceneNames;
	String childName;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Spawn ability node and set as cube child
	public void GenerateAbility(int abilityIndex)
	{
		abilitySceneNames = new String[4];
		abilitySceneNames[0] = "Bomb";
		abilitySceneNames[1] = "Football";
		abilitySceneNames[2] = "RestoreVitality";
		abilitySceneNames[3] = "Speed";

		childName = abilitySceneNames[abilityIndex];
		var abilityScene = ResourceLoader.Load<PackedScene>("res://Scenes/GameScenes/AbilityScenes/" + childName + ".tscn").Instantiate<Node3D>();
		AddChild(abilityScene);

		/*if (abilityIndex == 0)
		{
			ability = new Bomb();
		}
		else if (abilityIndex == 1)
		{
			ability = new FootballTeam();
		}
		else if (abilityIndex == 2)
		{
			ability = new Speed();
		}
		else if (abilityIndex == 3)
		{
			ability = new RestoreVitality();
		}*/
	}

	// return child node
	public Node3D GetAbility()
	{
		return (Node3D)GetNode(childName);
		//return ability;
	}

	public void Destroy()
	{
		QueueFree();
	}
}
