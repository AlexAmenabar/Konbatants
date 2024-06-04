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

		// spawn players (nodes) in scene using map spawn points
		SpawnPlayers(amountPlayers);

		// set teams (set player teams)
		SetPlayersTeam(teams, amountPlayers);

		// manage GUI visibility and set GUI to player (and set username)
		ManageGUI(amountPlayers, teams);


		// create server
		if (isServer == true)
		{
			CreateServer(hostPort);
			GD.Print("Server: hostPort: " + hostPort.ToString());
		}
		// create client
		else
		{
			CreateClient(hostIp, hostPort, clientPort);
			GD.Print("Client: hostPort: " + hostPort.ToString() + ", hostIP: " + hostIp + ", clientPort: " + clientPort.ToString());
		}

		// activate virtual controller
		//if(parser.GetOS())
			//(GetNode("../Controllers/Virtual Joystick") as Control).Visible = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(connected_players + 1 == amountPlayers && !initialized)
		{
			initialized = true;
			// initialize players
			for (int i = 0; i < amountPlayers; i++)
			{
				//int team = i / (amountPlayers / 2);

				(GetNode("../Players/player" + i.ToString()) as PlayerController).Initialize();
			}

			// quit loading panel
			(GetNode("../CanvasLayer/LoadingPanel") as ColorRect).Visible = false;
		}
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
		for (int i = 0; i < players; i++)
		{
			PlayerController p = GetNode("../Players/player" + i.ToString()) as PlayerController;
			PlayerGUIController pGUI = GetNode("../CanvasLayer/p" + i.ToString()) as PlayerGUIController;

			p.PlayerGUIController = pGUI;

			ColorRect nodeGUI = (ColorRect)GetNode("../CanvasLayer/p" + i.ToString());

			// username
			pGUI.SetPlayerName(parser.GetName(i));
			
			nodeGUI.Visible = true;
		}
	}

	/// <summary>
	/// Create a Server using Godot MultiplayerAPI
	/// </summary>
	/// <param name="port">Port in which server will be hearing</param>
	private void CreateServer(int port)
	{
		GD.Print("Server starting");
	   
		// create client
		peer = new ENetMultiplayerPeer();
		Error err = peer.CreateServer(port, 2);

		if (err != Error.Ok)
		{
			GD.Print("An error occurred!");
			GD.Print(err);
			return;
		}

		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Server started!");
	}

	private void CreateClient(String hostIp, int hostPort, int localPort)
	{
		GD.Print("Client starting");
		peer = new ENetMultiplayerPeer();
		Error err = peer.CreateClient(hostIp, hostPort, 0, 0, 0, localPort); // communicate using localPort (almost hole punched)

		if (err != Error.Ok)
		{
			GD.Print("An error occurred!");
			GD.Print(err);
			return;
		}

		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Client created");
	}

	private void StartGame()
	{

	}

	// for now simulate, then load from playerInfo (GDSCRIPT)
	private void LoadPLayersInfo()
	{
		for(int i=0; i<amountPlayers; i++)
		{
			LoadPlayerInfo(i);

			//for now simulate
			PlayerInfoInGame playerInfo = new PlayerInfoInGame();

			// load this information from singleton
			playerInfo.name = i.ToString();
			playerInfo.ip = "192.168.1.37";
			playerInfo.port = 6666;
		}
	}

	private void LoadPlayerInfo(int index)
	{
		GD.Print("loading player information...\n");
	}



	/** Signals */

	/// <summary>
	/// runs when the connection is succesful and only runs on the clients
	/// </summary>
	/// <param name="id"></param>
	private void PeerConnected(long id)
	{
		GD.Print("Player connected:" + id.ToString());
		connected_players = Multiplayer.GetPeers().Length;
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

}
