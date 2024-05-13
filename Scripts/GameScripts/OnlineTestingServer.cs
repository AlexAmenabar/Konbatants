using Godot;
using Godot.NativeInterop;
using System;

public partial class OnlineTestingServer : Node
{
	MultiplayerApi multiplayer;

	ENetMultiplayerPeer peer;

	// server info
	String ip = "127.0.0.1";
	int port = 6666;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Server starting");
		multiplayer = GetTree().GetMultiplayer();

		// connect signals
		foreach(Godot.Collections.Dictionary signal in multiplayer.MultiplayerPeer.GetSignalList())
		{
			GD.Print(signal);
		}
		
		Godot.Callable callable = new Godot.Callable(this, "OnConnectedToServer");
		multiplayer.MultiplayerPeer.Connect("peer_connected", callable);

		// create client
		peer = new ENetMultiplayerPeer();
		Error err = peer.CreateServer(port, 2);

		GD.Print(err);

		multiplayer.MultiplayerPeer = peer;

		GD.Print(multiplayer.GetPeers().Length);
		GD.Print("Server started!");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GD.Print(multiplayer.GetPeers().Length);
		foreach(int peer in multiplayer.GetPeers())
		{
			GD.Print(peer);
		}
	}

	private void OnConnectedToServer()
	{
		GD.Print("Client connected!");
	}
}
