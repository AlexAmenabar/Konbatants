using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

/// <summary>
/// This class controls the game proggession: start timer, generate ability cubes, control when game must finish...
/// </summary>
public partial class GameController : Node
{
	/* Player info */
	private String myName;
	private int myIndex;

	// player (client) port
	private int clientPort;
	private bool isServer; // if player is server

	// host information (server use this directly)
	private String hostIp;
	private int hostPort;

	// all players
	private List<PlayerController> players;

	// game general information
	private int amountPlayers;
	private bool teams;
	private String mapName;
	private List<int> deadPlayersCounter; // each element of the list represent a team, if no teams then list only has one element
	private bool allPlayersConnected = false;

	// game nodes
	private Map map;
		
	// parser
	private GSCriptToCSharp parser;

	// game start
	private bool gameStarted = false;
	private bool timerFinished = false;

	/* Game management variables */
	private double timer; // seconds
	private RichTextLabel timerLabel;

	// how much abilities are implemented
	private int abilityAmount = 8;
	private bool gameFinished = false;

	// multiplayer manager
	private MultiplayerManager multiplayerManager;


	// Getters and setters
	public int AmountPlayers { get => amountPlayers; set => amountPlayers = value; }
	public bool Teams { get => teams; set => teams = value; }
	public string MapName { get => mapName; set => mapName = value; }
	public Map Map { get => map; set => map = value; }
	public int ClientPort { get => clientPort; set => clientPort = value; }
	public bool IsServer { get => isServer; set => isServer = value; }
	public string HostIp { get => hostIp; set => hostIp = value; }
	public int HostPort { get => hostPort; set => hostPort = value; }
	public GSCriptToCSharp Parser { get => parser; set => parser = value; }
	public bool GameStarted { get => gameStarted; set => gameStarted = value; }
	public List<PlayerController> Players { get => players; set => players = value; }
	public string MyName { get => myName; set => myName = value; }
	public bool AllPlayersConnected { get => allPlayersConnected; set => allPlayersConnected = value; }
	public MultiplayerManager MultiplayerManager { get => multiplayerManager; set => multiplayerManager = value; }
	public int MyIndex { get => myIndex; set => myIndex = value; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// load information from GDScripts and look for some nodes
		LoadBasicInformation();

		// load game (gameloader)
		LoadGame();

		// Initialize multiplayer API (create server and clients)
		//InitializeMultiplayerAPI();
		multiplayerManager.InitializeNetworking(this);


		// create player list and add players
		players = new List<PlayerController>();
		for(int i = 0; i<amountPlayers; i++)
			players.Add(GetNode("./Players/player" + i.ToString()) as PlayerController);

		// now only can be two teams
		deadPlayersCounter = new List<int>();
		deadPlayersCounter.Add(0);
		if (!teams)
			deadPlayersCounter.Add(0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// when all players connected to server start
		if (allPlayersConnected && !gameStarted)
		{
			gameStarted = true;
			StartGame();

			// only server manages map events and ability spawn (then inform the rest of player by RCP functions)
			if(isServer)
				NextAbilitySpawn();
		}

		// manage timer
		if(gameStarted && !timerFinished)
			ManageGameTimer(delta);
	}

	/// <summary>
	/// Load game basic information: information from GDSCripts and load some nodes from PlayGround Scene.
	/// </summary>
	public void LoadBasicInformation()
	{
		// load parser
		parser = (GSCriptToCSharp)GetNode("./ParserNode");

		// load player information
		myName = parser.GetMyName(); // username

		// load info from CurrentSessionInfo and PlayerMenu
		mapName = "Default";
		amountPlayers = parser.GetAmountPlayers();
		teams = parser.GetTeams();
		isServer = parser.GetIsServer();

		hostIp = parser.GetHostIp();
		hostPort = parser.GetHostPort();
		clientPort = parser.GetClientPort();

		// get neccesary nodes
		timerLabel = (RichTextLabel)GetNode("./GUI/Timer");

		multiplayerManager = (MultiplayerManager)GetNode("./NetworkingNode");

		// get my index
		for (int i = 0; i < amountPlayers; i++)
			if (parser.GetName(i) == myName)
				myIndex = i;
	}

	/// <summary>
	/// Call GetLoader to load game (instantiate map, players...)
	/// </summary>
	public void LoadGame()
	{
		GameLoader gameLoader = (GameLoader)GetNode("./GameLoader");
		gameLoader.LoadGame(this);
	}

	/// <summary>
	/// WHen all is prepared start game. 
	/// </summary>
	public async void StartGame()
	{ 
		// initialize players
		for (int i = 0; i < amountPlayers; i++)
			(GetNode("./Players/player" + i.ToString()) as PlayerController).Initialize();

		await ToSignal(GetTree().CreateTimer(1), "timeout"); // CREATE A VISUAL TIMER BEFORE STARTING THE GAME

		// quit loading panel
		(GetNode("./GUI/LoadingPanel") as ColorRect).Visible = false;

		// countdown
		RichTextLabel countDownLabel = GetNode("./GUI/CountDownLabel") as RichTextLabel;
		countDownLabel.Visible = true;
		int countDown = 3;

		(GetNode("./Sounds/CountDown") as AudioStreamPlayer3D).Play(); // play count down sound

		while (countDown > 0)
		{
			countDownLabel.Text = "[center][i][b]" + countDown.ToString();
			await ToSignal(GetTree().CreateTimer(1), "timeout");
			countDown -= 1;
		}
		countDownLabel.Text = "[center][i][b]Start!!!";

		// Call start function in players to start game 
		for (int i = 0; i < amountPlayers; i++)
			(GetNode("./Players/player" + i.ToString()) as PlayerController).Start();

		// initialize game timer and set visible
		(GetNode("./GUI/Timer") as RichTextLabel).Visible = true;
		timer = 0;

		// hide start label
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		countDownLabel.Visible = false;
	}

	/// <summary>
	/// Count player as dead and check if game finished.
	/// </summary>
	/// <param name="player">Player that died</param>
	public void PlayerDied(PlayerController player)
	{
		if (!teams)
		{
			deadPlayersCounter[0] += 1;
			if (deadPlayersCounter[0] == (amountPlayers - 1))
				FinishGame();
		}
		else
		{
			deadPlayersCounter[player.Team] += 1;
			if (deadPlayersCounter[0] == (amountPlayers / 2) || deadPlayersCounter[1] == (amountPlayers / 2))
				FinishGame();
		}
	}

	/// <summary>
	/// Function called when game must finish. Sets the winner player or team.
	/// </summary>
	public async void FinishGame()
	{
		String winnerText = "[center][i][b]";
		RichTextLabel winnerLabel = (GetNode("./GUI/WinnerLabel") as RichTextLabel);

		if (!teams)
			winnerText += "Winner: ";
		else
			winnerText += "Winners: ";
	
		for(int i = 0; i<amountPlayers; i++)
			if (players[i].Vitality > 0)
			{
				players[i].SetProcess(false);
				players[i].SetPhysicsProcess(false);
				players[i].Freeze = false;
				players[i].Winner = true;
				players[i].SetAnimation();

				gameFinished = true;

				winnerText += parser.GetName(i) + " ";
			}

		winnerLabel.Visible = true;
		winnerLabel.Text = winnerText;

		// play sound
		(GetNode("./Sounds/FinishSound") as AudioStreamPlayer3D).Play();

		// wait some seconds and then load InitialScene
		await ToSignal(GetTree().CreateTimer(5), "timeout");

		// stop game music and start menu music
		GetNode("/root/MusicControllerScene/AudioStreamPlayer2D").Set("playing", true);
		GetNode("/root/MusicControllerScene/DefaultMap").Set("playing", false);


		// finalize multiplayer things
		Multiplayer.MultiplayerPeer.Close();

		String scenePath;
		if (parser.GetOS()) // mobile
			scenePath = "res://Scenes/MenuScenes/Mobile/InitialMenuMobile.tscn";
		else
			scenePath = "res://Scenes/MenuScenes/PC/InitialMenu.tscn";
		GetTree().ChangeSceneToFile(scenePath);
	}

	/// <summary>
	/// Spawn next ability cube in the map. Only server runs this, so must inform the rest clients.
	/// </summary>
	public async void NextAbilitySpawn()
	{
		// set when next ability will be spawned.
		Random rnd = new Random();
		int timeToNextAbility;
		Variant buffPosvariant;

		while (true)
		{ 
			timeToNextAbility = rnd.Next(10);
			if (timeToNextAbility < 3) timeToNextAbility = 3;

			// await time until next ability is spawned
			await ToSignal(GetTree().CreateTimer(timeToNextAbility), "timeout");

			// decide what ability must be generated (server must decide, and inform the rest about that)
			Variant abilityIndex = rnd.Next(abilityAmount);

			// generate buff on all players with RPC function
			buffPosvariant = Map.GetCubePosition();

			// inform rest players about what ability is spawned an where ability cube must be spawned
			multiplayerManager.Rpc("GenerateAbilityCube", buffPosvariant, abilityIndex);
		}
	}

	/// <summary>
	/// Kill a player when it enters in this are (falling)
	/// </summary>
	/// <param name="body"></param>
	private void _on_dead_area_body_entered(Node3D body)
	{
		// kill player
		if (body.IsInGroup("player"))
		{
			(body as PlayerController).Vitality = 0;
			(body as PlayerController).RefreshLifeBar();
			(body as PlayerController).Die();
		}
	}

	/// <summary>
	/// When player uses an ability has to inform the rest about that. Ability is not synchronized by multiplayer synchronizer,
	/// so inform about it
	/// </summary>
	/// <param name="playerIndex">Index of the player in player list (nodes)</param>
	/// <param name="hdir">Attack direction in x axis</param>
	/// <param name="vdir">Attack direction in z axis</param>
	public void AbilityUsed(int playerIndex, int hdir, int vdir)
	{
		multiplayerManager.Rpc("UseOtherPlayerAbility", playerIndex, hdir, vdir);
	}

	/// <summary>
	/// Manage what will be printed next in timer label
	/// </summary>
	/// <param name="delta"></param>
	public void ManageGameTimer(double delta)
	{
		timer += delta;
		int timerAsInt = (int)timer;

		int minutes = (int)(timerAsInt / 60);
		int seconds = (int)(timerAsInt % 60);

		// when time finished call map final event and print something in the screen
		if (minutes == 0 && seconds == 10)
		{
			Map.FinalMapEvent();
			timerFinished = true;

			// visualize final event label
			ManageFinalEventLabel();
		}

		// print time left
		String minutesLeft = (2 - minutes).ToString();
		String secondsLeft = (60 - seconds).ToString();
		if (60 - seconds < 10) secondsLeft = "0" + secondsLeft;

		timerLabel.Text = "[i][b]" + minutesLeft + ":" + secondsLeft;
	}

	/// <summary>
	/// Manage when final event label appears and disappears
	/// </summary>
	public  async void ManageFinalEventLabel()
	{
		if (!gameFinished)
		{
			(GetNode("./GUI/FinalEventLabel") as RichTextLabel).Visible = true;
			await ToSignal(GetTree().CreateTimer(3), "timeout");
			(GetNode("./GUI/FinalEventLabel") as RichTextLabel).Visible = false;
		}
	}
}
