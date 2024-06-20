using Godot;
using System;

/// <summary>
/// Abstract class to use all maps as one type 
/// </summary>
public abstract partial class Map : Node3D
{
	/// <summary>
	/// Initialize map spawning necesary nodes and initializing values
	/// </summary>
	public abstract void InitializeMap();
	
	/// <summary>
	/// After game timer map final event is called, something that helps finishing the game faster
	/// </summary>
	public abstract void FinalMapEvent();

	/// <summary>
	/// Returns the position where next cube must be spawned
	/// </summary>
	/// <returns></returns>
	public abstract Vector3 GetCubePosition();
}
