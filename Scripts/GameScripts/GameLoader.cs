using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// It loads all needed to play the game.
/// </summary>
public partial class GameLoader : Node
{
	/* Nodes */
	GSCriptToCSharp parser;
	
	/* Live bar and arrow colors */
	Color BLUE = new Color(0, 0, 1, 1);
	Color GREEN = new Color(0, 1, 0, 1);
	Color RED = new Color(1, 0, 0, 1);

	/// <summary>
	/// This function loads in PlayGround scene all nodes needed in the game using session information.
	/// Loads Players (scenes), map, set teams...
	/// </summary>
	/// <param name="gameController">GameController node (used to get and store information)</param>
	public void LoadGame(GameController gameController)
	{
		// loads parser
		parser = (GSCriptToCSharp)GetNode("../ParserNode");

		// instantiate map scene
		LoadMap(gameController, gameController.MapName);

		// stop menu music
		GetNode("/root/MusicControllerScene/AudioStreamPlayer2D").Set("playing", false);

		// spawn players (nodes) in scene using map spawn points
		SpawnPlayers(gameController.Map, gameController.AmountPlayers);

		// set player teams
		SetPlayersTeam(gameController.Teams, gameController.AmountPlayers);

		// manage GUI visibility and set GUI to player (and set username)
		ManageGUI(gameController.AmountPlayers);
	}

	/// <summary>
	/// Instantiates map for this game
	/// </summary>
	/// <param name="gameController">Session gameController</param>
	/// <param name="mapName">Name of the map that must instantiate</param>
	private void LoadMap(GameController gameController, String mapName)
	{
		// load map scene and instantiate
		var scene = ResourceLoader.Load<PackedScene>("res://Scenes/GameScenes/Maps/" + mapName +".tscn").Instantiate<Map>();
		GetNode("../Map").AddChild(scene);

		// store map node in gameController
		gameController.Map = (Map)GetNode("../Map/" + mapName);
	}

	/// <summary>
	/// Spawn player scenes for this game
	/// </summary>
	/// <param name="map">Map node is used to know where players must be spawned</param>
	/// <param name="amountPlayers">Amount of players in this game</param>
	private void SpawnPlayers(Map map, int amountPlayers)
	{
		for(int i=0; i<amountPlayers; i++)
			SpawnPlayer(map, i);
	}

	/// <summary>
	/// Spawn a player
	/// </summary>
	/// <param name="map">Map node used to know where player must be spawned</param>
	/// <param name="i">Spawned player index</param>
	private void SpawnPlayer(Map map, int i)
	{
		// get spawn point from map scene/node
		Node3D spawnPointNode = (Node3D)map.GetNode("./SpawnPoints").GetChild(i);
		Vector3 spawnPoint = spawnPointNode.GlobalPosition;

		// spawn a player 
		var player = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn").Instantiate<RigidBody3D>();
		player.Name = "player" + i.ToString(); // set spawned player name correctly (player + i)

		// set player position on spawnpoint
		player.GlobalPosition = spawnPoint;

		// add player node to PlayGround scene as child of Players node (all players nodes)
		GetNode("../Players").AddChild(player); 
	}

	/// <summary>
	/// Set each player team
	/// </summary>
	/// <param name="teams">A boolean value that says if there are teams or not (in the future change to int to allow more than 2 teams)</param>
	/// <param name="amountPlayers">Amount of players in the game</param>
	private void SetPlayersTeam(bool teams, int amountPlayers)
	{
		// set team for each player
		for(int i=0; i<amountPlayers; i++)
		{
			PlayerController p = GetNode("../Players/player" + i.ToString()) as PlayerController;
			if (teams) // if (teams == 2)
			{
				if (i % 2 == 0)
					p.Team = 0;
				else
					p.Team = 1;
			}
			else // each player has a different team (if teams == 1)
			{
				p.Team = i;
			}
		}
	}

	/// <summary>
	/// Tie each PlayerController with one GUI element
	/// </summary>
	/// <param name="players">Amount of players</param>
	/// <param name="teams">If there are teams or not</param>
	private void ManageGUI(int players)
	{
		// get all players names
		String[] playerNames = parser.GetSessionPlayerNames();

		// get MY name (name of this user)
		String playerName = parser.GetMyName();
		
		// get index of this player in player list
		int index = 0;
		for (int i = 0; i < players; i++)
			if (playerNames[i] == playerName)
				index = i;

		// get team number of this player
		int team = (GetNode("../Players/player" + index.ToString()) as PlayerController).Team;

		// for each player set GUI
		for (int i = 0; i < players; i++)
		{
			// get PlayerController and GUI element
			PlayerController p = GetNode("../Players/player" + i.ToString()) as PlayerController;
			PlayerGUIController pGUI = GetNode("../GUI/p" + i.ToString()) as PlayerGUIController;
			ColorRect nodeGUI = (ColorRect)GetNode("../GUI/p" + i.ToString());

			// set GUI to PlayerController and PlayerController to GUI
			p.PlayerGUIController = pGUI;
			pGUI.Player = p;

			// change GUI bar and arrow color and visibility
			Node3D arrowNode;
			arrowNode = (Node3D)p.GetNode("./Arrow");

			// if player is controlled by this usser set GREEN color to GUI elements
			if (parser.GetMyName().Equals(parser.GetName(i)))
			{
				(nodeGUI.GetNode("./PlayerVit/PlayerActualVit") as ColorRect).Color = GREEN;
				(arrowNode.GetNode("./greenArrow") as Node3D).Visible = true;
			}
			// if PlayerController is of the same team as PlayerController of this user set BLUE
			else if (p.Team == team)
			{
				(nodeGUI.GetNode("./PlayerVit/PlayerActualVit") as ColorRect).Color = BLUE;
				(arrowNode.GetNode("./blueArrow") as Node3D).Visible = true;
			}
			// if team is different set RED color
			else
			{
				(nodeGUI.GetNode("./PlayerVit/PlayerActualVit") as ColorRect).Color = RED;
				(arrowNode.GetNode("./redArrow") as Node3D).Visible = true;
			}

			// change arrow visibility and position (and set as top level to avoid following rotation)
			arrowNode.Visible = true;
			arrowNode.TopLevel = true;
			p.PositionIndicatorArrow = arrowNode;

			// set username
			pGUI.SetPlayerName(parser.GetName(i));

			// set GUI visible
			nodeGUI.Visible = true;
		}
	}
}
