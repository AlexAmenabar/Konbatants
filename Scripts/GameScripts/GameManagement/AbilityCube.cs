using Godot;
using System;
using System.Collections.Generic;

public partial class AbilityCube : RigidBody3D
{
	private Ability ability;

	String[] abilityNames;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		abilityNames = new String[4];
		abilityNames[0] = "Bomb";
		abilityNames[1] = "FootballTeam";
		abilityNames[2] = "Speed";
		abilityNames[3] = "RestoreVitality";

		GenerateAbility();
	}

	public void GenerateAbility()
	{
		Random rnd = new Random();
		int index = rnd.Next(4);

		if (index == 0)
			ability = new Bomb();
		else if (index == 1)
			ability = new FootballTeam();
		else if (index == 2)
			ability = new Speed();
		else if (index == 3)
			ability = new RestoreVitality();
	}

	public Ability GetAbility()
	{
		return ability;
	}

	public void Destroy()
	{
		QueueFree();
	}
}
