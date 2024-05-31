using Godot;
using System;
using Godot.NativeInterop;

public partial class OnlineTesting : Control
{
	ENetMultiplayerPeer peer;

	// server info
	String ip = "192.168.1.37";
	int port = 6666;

	StringName test1 = new StringName("Test1");
	StringName test2 = new StringName("Test2");

	int attemp=0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	private void CreateServer()
	{
		GD.Print("Server starting");
		//Multiplayer = GetTree().GetMultiplayer();

		// connect signals
		//foreach (Godot.Collections.Dictionary signal in Multiplayer.MultiplayerPeer.GetSignalList())
		//{
		//	GD.Print(signal);
		//}

		//Godot.Callable callable = new Godot.Callable(this, "OnConnectedToServer");
		//Multiplayer.MultiplayerPeer.Connect("peer_connected", callable);

		// create client
		peer = new ENetMultiplayerPeer();
		Error err = peer.CreateServer(port, 2);

		GD.Print(err);
		if(err != Error.Ok)
		{
			GD.Print("An error occurred!");
			return;
		}

		Multiplayer.MultiplayerPeer = peer;

		//GD.Print(Multiplayer.GetPeers().Length);
		GD.Print("Server started!");
	}
	private void ConnectToServer()
	{
		GD.Print("Client starting");
		//Multiplayer = GetTree().GetMultiplayer();

		// create client
		peer = new ENetMultiplayerPeer();
		Error err = peer.CreateClient(ip, port);

		GD.Print(err);


		if (err != Error.Ok)
		{
			GD.Print("An error occurred!");
			return;
		}
		
		Multiplayer.MultiplayerPeer = peer;
		
		GD.Print("Client created");
	}

	/// <summary>
	/// runs when the connection is succesful and only runs on the clients
	/// </summary>
	/// <param name="id"></param>
	private void PeerConnected(long id)
	{
		GD.Print("Player connected:" + id.ToString());
	}

	/// <summary>
	/// runs when a player disconnects and runs on all peers
	/// </summary>
	/// <param name="id"></param>
	private void PeerDisconnected(long id)
	{
		GD.Print("Player disconnected:" + id.ToString());
	}

	/// <summary>
	/// runs when the connection is succesful and only runs on the clients
	/// </summary>
	/// <param name="id"></param>
	private void ConnectedToServer()
	{
		GD.Print("Connected to server!");
	}

	/// <summary>
	/// runs when the connection fails and it runs only on the client
	/// </summary>
	/// <param name="id"></param>
	private void ConnectionFailed()
	{
		GD.Print("Connection to server failed!");
	}

	private void _on_button_pressed()
	{
		// Replace with function body.
		ConnectToServer();
	}

	private void _on_button_2_pressed()
	{
		// Replace with function body.
		CreateServer();
	}

	private void _on_button_3_pressed()
	{
		Rpc("StartGame");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void StartGame()
	{
		// Replace with function body.
		var scene = ResourceLoader.Load<PackedScene>("res://scenery.tscn").Instantiate<Node3D>();
		GetTree().Root.AddChild(scene);
		Hide();
	}
}
