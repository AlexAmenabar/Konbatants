using Godot;
using System;

public partial class PlayerInfoInGame : Node
{
	public String name;
	public int multiplayerID;
	public String ip;
	public int port;
	public int myport;

	public bool isServer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// load info from player info

		
		name = "a";
		ip = "192.168.1.37";
		port = 6666;
        //var playerMenu = GetNode<Node>("/root/PlayerMenu");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}
