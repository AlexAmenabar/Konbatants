using Godot;
using System;

public abstract partial class Map : Node3D
{
	public abstract void InitializeMap();
	public abstract void FinalMapEvent();

	public abstract Vector3 GetCubePosition();
}
