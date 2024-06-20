using Godot;
using System;

/// <summary>
/// Default map implementation. More information about Maps in Map.cs file (abstract class)
/// </summary>
public partial class DefaultMap : Map
{
	Node3D mapNode;
	float nodeScale;

	CollisionShape3D collisionShape3D;
	float colliderScale;

	public override void _Ready()
	{
		InitializeMap();
		mapNode = GetNode("./Map_Scene") as Node3D;
		collisionShape3D = GetNode("./RigidBody3D/CollisionShape3D") as CollisionShape3D;
		nodeScale = mapNode.Scale.X;
		colliderScale = collisionShape3D.Scale.X;

		(GetNode("./MapCamera") as Node3D).TopLevel = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Initialize map. In this case only start playing map music.
	/// </summary>
	public override void InitializeMap()
	{
		// Initialize music
		GetNode("./MapMusic").Set("playing", true);
	}

	/// <summary>
	/// Throw map final event (map gets smaller).
	/// </summary>
	public override void FinalMapEvent()
	{
		DwarfMap();
	}
	public async void DwarfMap()
	{
		// reduce collider and node scale proportionally
		while (mapNode.Scale.X > (nodeScale * 0.25f))
		{
			float mapScaleReduction, colliderScaleReduction;

			mapScaleReduction = mapNode.Scale.X * 0.01f;
			colliderScaleReduction = collisionShape3D.Scale.X * 0.01f;

			mapNode.Scale -= new Vector3(mapScaleReduction, 0, mapScaleReduction);
			collisionShape3D.Scale -= new Vector3(colliderScaleReduction, 0, colliderScaleReduction);

			mapNode.Position += new Vector3(mapScaleReduction / 1.6f, 0, mapScaleReduction / 1.6f);
			collisionShape3D.Position += new Vector3(colliderScaleReduction, 0, colliderScaleReduction);

			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		}
	}

	/// <summary>
	/// Cube can be spawned in any position inside map limits.
	/// </summary>
	/// <returns></returns>
	public override Vector3 GetCubePosition()
	{
		// from x = 2 to x = 22 and z = 2 to z = 22
		Random rnd = new Random();
		return new Vector3(rnd.Next(2,22), 30, rnd.Next(2,22));
	}
}
