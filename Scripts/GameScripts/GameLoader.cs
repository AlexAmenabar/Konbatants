using Godot;
using System;
using System.Collections.Generic;

public partial class GameLoader : Node
{
	ENetMultiplayerPeer peer;

	// server info
	private int amountPlayers;
	private bool teams;
	private String mapName;
	private bool isServer;

	// player info
	private String hostIp;
	private int hostPort;
	private int clientPort;

	// nodes
	Node3D mapNode;

	List<PlayerInfoInGame> gamePlayers;

	GSCriptToCSharp parser;

	int connected_players = 0;
	bool initialized = false;

	PacketPeerUdp socket;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		parser = (GSCriptToCSharp)GetNode("../ParserNode");

		// connect to signals
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;


		// initialize information from another scripts
		mapName = "Default"; // only this map for now
		amountPlayers = parser.GetAmountPlayers();
		teams = parser.GetTeams();
		isServer = parser.GetIsServer();

		hostIp = parser.GetHostIp();
		hostPort = parser.GetHostPort();
		clientPort = parser.GetClientPort();
		//GD.Print("OwnPort: " + clientPort.ToString());
		
		// instantiate map scene
		LoadMap(mapName);

		// start Map music
		GetNode("/root/MusicControllerScene/AudioStreamPlayer2D").Set("playing", false);
		GetNode("/root/MusicControllerScene/" + mapName + "Map").Set("playing", true);

		// spawn players (nodes) in scene using map spawn points
		SpawnPlayers(amountPlayers);

		// set teams (set player teams)
		SetPlayersTeam(teams, amountPlayers);

		// manage GUI visibility and set GUI to player (and set username)
		ManageGUI(amountPlayers, teams);



		//CreateSocket();
		//HolePunching();

		//CreateSocketAndSendMessage();

		
		// create server
		if (isServer == true)
		{
			CreateServer(hostPort);
			GD.Print(parser.GetMyName() + ": Server: hostPort: " + hostPort.ToString());
		}
		// create client
		else
		{
			CreateClient(hostIp, hostPort, clientPort);
			GD.Print(parser.GetMyName() + ": Client: hostPort: " + hostPort.ToString() + ", hostIP: " + hostIp + ", clientPort: " + clientPort.ToString());
		}
		
		// activate virtual controller
		//if(parser.GetOS())
		//(GetNode("../Controllers/Virtual Joystick") as Control).Visible = true;
	}

	public void CreateSocket()
	{
		socket = new PacketPeerUdp();
		if(isServer)
		{
			GD.Print("Server bind on " + hostPort.ToString() + " port, and set dest in (" + parser.GetClientIp(0) + ", " + parser.GetClientPort(0).ToString() + ")");
			socket.SetDestAddress(parser.GetClientIp(0), parser.GetClientPort(0));
			Error err = socket.Bind(hostPort);
			GD.Print("Server: error " + err.ToString());
		}
		else
		{
			GD.Print("Client bind on " + clientPort.ToString() + " port, and set dest in (" + hostIp + ", " + hostPort.ToString() + ")");
			socket.SetDestAddress(hostIp, hostPort);
			Error err = socket.Bind(clientPort);
			GD.Print("Client: error " + err.ToString());
		}
	}
	public async void CreateSocketAndSendMessage()
	{
		// check if it is possible to communicate with others
		socket = new PacketPeerUdp();
		if (isServer)
		{
			GD.Print("Server bind on " + hostPort.ToString() + " port, and set dest in (" + parser.GetClientIp(0) + ", " + parser.GetClientPort(0).ToString() + ")");
			socket.SetDestAddress(parser.GetClientIp(0), parser.GetClientPort(0));
			Error err = socket.Bind(hostPort);
			GD.Print("Server: error " + err.ToString());

			await ToSignal(GetTree().CreateTimer(3), "timeout");

			socket.PutPacket("Yeah".ToAsciiBuffer());
			GD.Print("Server: Message send");
		}
		else
		{
			GD.Print("Client bind on " + clientPort.ToString() + " port, and set dest in (" + hostIp + ", " + hostPort.ToString() + ")");
			socket.SetDestAddress(hostIp, hostPort);
			Error err = socket.Bind(clientPort);
			GD.Print("Client: error " + err.ToString());

			await ToSignal(GetTree().CreateTimer(3), "timeout");

			socket.PutPacket("Yeah".ToAsciiBuffer());
			GD.Print("Client: Message send");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(connected_players + 1 == amountPlayers && !initialized)
		{
			initialized = true;
			StartGame();
		}
		/*if(socket.GetAvailablePacketCount() > 0)
		{
			String data = socket.GetPacket().GetStringFromAscii();
			GD.Print("IsServer = " + isServer.ToString() + ", Data: " + data);
		}*/
	}

	public async void StartGame()
	{
		await ToSignal(GetTree().CreateTimer(3), "timeout"); // CREATE A VISUAL TIMER BEFORE STARTING THE GAME
		// quit loading panel
		(GetNode("../CanvasLayer/LoadingPanel") as ColorRect).Visible = false;

		// initialize players
		for (int i = 0; i < amountPlayers; i++)
			(GetNode("../Players/player" + i.ToString()) as PlayerController).Initialize();

	}

	private void LoadMap(String mapName)
	{
		// load map scene
		var scene = ResourceLoader.Load<PackedScene>("res://Scenes/GameScenes/Maps/" + mapName +".tscn").Instantiate<Node3D>();
		GetNode("../Map").AddChild(scene);
		mapNode = (Node3D)GetNode("../Map/" + mapName);
	}

	private void SpawnPlayers(int amountPlayers)
	{
		for(int i=0; i<amountPlayers; i++)
			SpawnPlayer(i);
	}

	private void SpawnPlayer(int i)
	{
		// get spawn point from map scene
		Node3D spawnPointNode = (Node3D)mapNode.GetNode("./SpawnPoints").GetChild(i);
		Vector3 spawnPoint = spawnPointNode.GlobalPosition;

		// spawn a player in that position
		var player = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn").Instantiate<RigidBody3D>();
		player.Name = "player" + i.ToString(); // set spawned player name correctly

		// set player position on spawnpoint
		player.GlobalPosition = spawnPoint;

		// add player to player list
		GetNode("../Players").AddChild(player); 
	}

	private void SetPlayersTeam(bool teams, int amountPlayers)
	{
		for(int i=0; i<amountPlayers; i++)
		{
			PlayerController p = GetNode("../Players/player" + i.ToString()) as PlayerController;
			if (teams)
			{
				if (i < amountPlayers / 2)
					p.Team = 0;
				else
					p.Team = 1;
			}
			else // each player has a different team
			{
				p.Team = i;
			}
		}
	}

	private void ManageGUI(int players, bool teams)
	{
		if (teams)
		{
			// set one team GUI
			int nGUI = 0;
			for(int i = 0; i < players-1; i+=2)
			{
				GD.Print("Entered on teams");

				PlayerController p = GetNode("../Players/player" + i.ToString()) as PlayerController;
				PlayerGUIController pGUI = GetNode("../CanvasLayer/p" + (nGUI).ToString()) as PlayerGUIController;

				ColorRect nodeGUI = (ColorRect)GetNode("../CanvasLayer/p" + nGUI.ToString());

				// username
				pGUI.SetPlayerName(parser.GetName(i));
				nodeGUI.Visible = true;

				nGUI++;
			}
			for(int i = 1; i<players; i+=2)
			{
				PlayerController p = GetNode("../Players/player" + i.ToString()) as PlayerController;
				PlayerGUIController pGUI = GetNode("../CanvasLayer/p" + (nGUI).ToString()) as PlayerGUIController;

				ColorRect nodeGUI = (ColorRect)GetNode("../CanvasLayer/p" + nGUI.ToString());

				// username
				pGUI.SetPlayerName(parser.GetName(i));
				nodeGUI.Visible = true;

				nGUI++;
			}
		}
		else
		{
			for (int i = 0; i < players; i++)
			{
				GD.Print(i.ToString() + " GUI initializiging");
				PlayerController p = GetNode("../Players/player" + i.ToString()) as PlayerController;
				PlayerGUIController pGUI = GetNode("../CanvasLayer/p" + i.ToString()) as PlayerGUIController;

				p.PlayerGUIController = pGUI;

				ColorRect nodeGUI = (ColorRect)GetNode("../CanvasLayer/p" + i.ToString());

				// username
				pGUI.SetPlayerName(parser.GetName(i));

				nodeGUI.Visible = true;
			}
		}
	}

	/// <summary>
	/// Create a Server using Godot MultiplayerAPI
	/// </summary>
	/// <param name="port">Port in which server will be hearing</param>
	private void CreateServer(int port)
	{
		GD.Print(parser.GetMyName() + ": Server starting");
	   
		// create client
		peer = new ENetMultiplayerPeer();
		Error err = peer.CreateServer(port, parser.GetAmountPlayers() - 1);

		if (err != Error.Ok)
		{
			GD.Print(parser.GetMyName() + ": An error occurred!");
			GD.Print(err);
			return;
		}

		Multiplayer.MultiplayerPeer = peer;
		GD.Print(parser.GetMyName() + ": Server started!");
	}

	private void CreateClient(String hostIp, int hostPort, int localPort)
	{
		GD.Print(parser.GetMyName() + ": Client starting");
		peer = new ENetMultiplayerPeer();
		Error err = peer.CreateClient(hostIp, hostPort, 0, 0, 0, localPort); // communicate using localPort (almost hole punched)

		if (err != Error.Ok)
		{
			GD.Print(parser.GetMyName() + ": An error occurred!");
			GD.Print(err);
			return;
		}

		Multiplayer.MultiplayerPeer = peer;
		GD.Print(parser.GetMyName() + ": Client created");
	}

	/** Signals */

	/// <summary>
	/// runs when the connection is succesful and only runs on the clients
	/// </summary>
	/// <param name="id"></param>
	private void PeerConnected(long id)
	{
		GD.Print(parser.GetMyName() + ": Player connected:" + id.ToString());
		connected_players = Multiplayer.GetPeers().Length;
	}

	/// <summary>
	/// runs when a player disconnects and runs on all peers
	/// </summary>
	/// <param name="id"></param>
	private void PeerDisconnected(long id)
	{
		GD.Print(parser.GetMyName() + ": Player disconnected:" + id.ToString());
	}

	/// <summary>
	/// runs when the connection is succesful and only runs on the clients
	/// </summary>
	/// <param name="id"></param>
	private void ConnectedToServer()
	{
		GD.Print(parser.GetMyName() + ": Connected to server!");
	}

	/// <summary>
	/// runs when the connection fails and it runs only on the client
	/// </summary>
	/// <param name="id"></param>
	private void ConnectionFailed()
	{
		GD.Print(parser.GetMyName() + ": Connection to server failed!");
	}


	public async void HolePunching()
	{
		bool responseReceived = false;
		bool ACKreceived = false;
		String message = "message";
		String ACKmessage = "ACK";
		while(!responseReceived || !ACKreceived)
		{
			GD.Print("Sending message");
			socket.PutPacket(message.ToAsciiBuffer());

			await ToSignal(GetTree().CreateTimer(0.15f), "timeout");

			// if message reached
			if(socket.GetAvailablePacketCount()>0)
			{
				if(socket.GetPacket().GetStringFromAscii().StartsWith("message"))
				{
					GD.Print("message received");
					responseReceived = true;

					//send ACK
					socket.PutPacket(ACKmessage.ToAsciiBuffer());
				}
				if(socket.GetPacket().GetStringFromAscii().StartsWith("ACK"))
				{
					GD.Print("ACK received");
					ACKreceived = true;
				}
			}
		}

		GD.Print("Hole Punched");
	}
}
