using Godot;
using System;
using System.Collections.Generic;

public partial class GameController : Node
{
	ENetMultiplayerPeer peer;
	int connectedPlayers = 0;

	// game general information
	private int amountPlayers;
	private bool teams;
	private String mapName;

	// game nodes
	private Map map;

	// players information
//	List<String> playerNames;
//	List<PlayerController> players;

	// player information
//	private String playerName;
//	private PlayerController player; // player that is controlled by this system
	private int clientPort;
	private bool isServer;

	// host information
	private String hostIp;
	private int hostPort;
		
	// parser
	private GSCriptToCSharp parser;

	// game start
	private bool gameStarted = false;


	/* Game management variables */
	private double timer; // seconds
	private int time; // seconds
	private Label timerLabel;

	// how much abilities are implemented
	int abilityAmount = 8;

	// Getters and setters
	public int AmountPlayers { get => amountPlayers; set => amountPlayers = value; }
	public bool Teams { get => teams; set => teams = value; }
	public string MapName { get => mapName; set => mapName = value; }
	public Map Map { get => map; set => map = value; }
//	public string[] PlayerNames { get => playerNames; set => playerNames = value; }
//	public PlayerController[] Players { get => players; set => players = value; }
//	public string PlayerName { get => playerName; set => playerName = value; }
//	public PlayerController Player { get => player; set => player = value; }
	public int ClientPort { get => clientPort; set => clientPort = value; }
	public bool IsServer { get => isServer; set => isServer = value; }
	public string HostIp { get => hostIp; set => hostIp = value; }
	public int HostPort { get => hostPort; set => hostPort = value; }
	public GSCriptToCSharp Parser { get => parser; set => parser = value; }
	public bool GameStarted { get => gameStarted; set => gameStarted = value; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		timerLabel = (Label)GetNode("./GUI/Timer");
		LoadBasicInformation();
		LoadGame();
		InitializeMultiplayerAPI();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (connectedPlayers + 1 == amountPlayers && !gameStarted)
		{
			gameStarted = true;
			StartGame();

			// only server manages map events and ability spawn
			if(isServer)
			{
				NextAbilitySpawn();
			}
		}
		if(gameStarted)
		{
			ManageGameTimer(delta);
		}
	}
	public void LoadBasicInformation()
	{
		// load parser
		parser = (GSCriptToCSharp)GetNode("./ParserNode");

		// load info from CurrentSessionInfo and PlayerMenu
		mapName = "Default";
		amountPlayers = parser.GetAmountPlayers();
		teams = parser.GetTeams();
		isServer = parser.GetIsServer();

		hostIp = parser.GetHostIp();
		hostPort = parser.GetHostPort();
		clientPort = parser.GetClientPort();

		//String[] playerNamesList = parser.GetSessionPlayerNames();
		//for(int i = 0; i < playerNamesList.Length; i++)
		//	playerNames.Add(playerNamesList[i]);

		//playerName = parser.GetMyName();
	}

	public void LoadGame()
	{
		GameLoader gameLoader = (GameLoader)GetNode("./GameLoader");
		gameLoader.LoadGame(this);
	}

	public async void StartGame()
	{ 
		// quit loading panel
		// initialize players
		for (int i = 0; i < amountPlayers; i++)
			(GetNode("./Players/player" + i.ToString()) as PlayerController).Initialize();

		await ToSignal(GetTree().CreateTimer(1), "timeout"); // CREATE A VISUAL TIMER BEFORE STARTING THE GAME
		(GetNode("./GUI/LoadingPanel") as ColorRect).Visible = false;

		// Create a visual timer

		// start 
		for (int i = 0; i < amountPlayers; i++)
			(GetNode("./Players/player" + i.ToString()) as PlayerController).Start();

	}


	// Network functions
	private void InitializeMultiplayerAPI()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;

		if (isServer == true)
		{
			CreateServer(hostPort);
			//GD.Print(parser.GetMyName() + ": Server: hostPort: " + hostPort.ToString());
		}
		// create client
		else
		{
			CreateClient(hostIp, hostPort, clientPort);
			//GD.Print(parser.GetMyName() + ": Client: hostPort: " + hostPort.ToString() + ", hostIP: " + hostIp + ", clientPort: " + clientPort.ToString());
		}

	}

	public async void NextAbilitySpawn()
	{
		Random rnd = new Random();
		int timeToNextAbility;
		Variant buffPosvariant;

		while (true)
		{ 
			timeToNextAbility = rnd.Next(10);
			if (timeToNextAbility < 3) timeToNextAbility = 3;

			await ToSignal(GetTree().CreateTimer(timeToNextAbility), "timeout");

			// decide what ability must be generated (server must decide, and inform the rest about that)
			Variant abilityIndex = rnd.Next(abilityAmount);

			// generate buff on all players with RPC function
			buffPosvariant = Map.GetCubePosition();

			Rpc("GenerateAbilityCube", buffPosvariant, abilityIndex);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, CallLocal = true)]
	public void GenerateAbilityCube(Vector3 position, int abilityIndex)
	{
		var abilityCube = ResourceLoader.Load<PackedScene>("res://Scenes/GameScenes/AbilityCube/AbilityCube.tscn").Instantiate<AbilityCube>();
		abilityCube.GenerateAbility(abilityIndex);
		abilityCube.Position = position;
		Map.AddChild(abilityCube);
	}

	public void AbilityUsed(int playerIndex, int hdir, int vdir)
	{
		Rpc("UseOtherPlayerAbility", playerIndex, hdir, vdir);
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, CallLocal = false)]
	public void UseOtherPlayerAbility(int playerIndex, int hdir, int vdir)
	{
		// get player and use ability
		PlayerController pPlayer = (PlayerController)GetNode("./Players/player" + playerIndex.ToString());
		pPlayer.LookingHDir = hdir;
		pPlayer.LookingVDir = vdir;
		((GetNode("./Players/player" + playerIndex.ToString()) as PlayerController).Ability as Ability).Use();

		// quit ability texture
		(GetNode("./Players/player" + playerIndex.ToString()) as PlayerController).PlayerGUIController.SetAbilityTexture(null);
	}

	public void ManageGameTimer(double delta)
	{
		timer += delta;
		int timerAsInt = (int)timer;

		int minutes = (int)(timerAsInt / 60);
		int seconds = (int)(timerAsInt % 60);

		// when time finished call map final event
		if (minutes == 3 && seconds == 0)
		{
			Map.FinalMapEvent();
		}

		timerLabel.Text = (2 - minutes).ToString() + ":" + (60 - seconds).ToString();
	}

	// instantiate ability
	/*public void InstantiateAbility(Attack attack)
	{
		GD.Print("Before calling attack.Instantiate in gameController");
		Node3D ability = (Attack)attack.Instantiate();
		GD.Print("On GameConstroller after instantiating");
		ability.GlobalPosition = new Vector3(10, 25, 15);
		GD.Print("Bomb name: " + ability.Name);
		Map.AddChild(ability);
	}*/


	/// <summary>
	/// Create a Server using Godot MultiplayerAPI
	/// </summary>
	/// <param name="port">Port in which server will be hearing</param>
	private void CreateServer(int port)
	{
		//GD.Print(parser.GetMyName() + ": Server starting");

		// create client
		peer = new ENetMultiplayerPeer();
		Error err = peer.CreateServer(port, parser.GetAmountPlayers() - 1);

		if (err != Error.Ok)
		{
			//GD.Print(parser.GetMyName() + ": An error occurred!");
			//GD.Print(err);
			return;
		}

		Multiplayer.MultiplayerPeer = peer;
		//GD.Print(parser.GetMyName() + ": Server started!");
	}

	private void CreateClient(String hostIp, int hostPort, int localPort)
	{
		//GD.Print(parser.GetMyName() + ": Client starting");
		peer = new ENetMultiplayerPeer();
		Error err = peer.CreateClient(hostIp, hostPort, 0, 0, 0, localPort); // communicate using localPort (almost hole punched)

		if (err != Error.Ok)
		{
			//GD.Print(parser.GetMyName() + ": An error occurred!");
			//GD.Print(err);
			return;
		}

		Multiplayer.MultiplayerPeer = peer;
		//GD.Print(parser.GetMyName() + ": Client created");
	}


	/** Signals */

	/// <summary>
	/// runs when the connection is succesful and only runs on the clients
	/// </summary>
	/// <param name="id"></param>
	private void PeerConnected(long id)
	{
		GD.Print(parser.GetMyName() + ": Player connected:" + id.ToString());
		connectedPlayers = Multiplayer.GetPeers().Length;
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
}
