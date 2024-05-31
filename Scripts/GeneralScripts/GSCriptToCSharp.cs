using Godot;
using System;

public partial class GSCriptToCSharp : Node
{
	// Called when the node enters the scene tree for the first time.
	GDScript currentSessionInfo;
	GDScript playerMenuInfo;
	public override void _Ready()
	{
		// load scripts
		currentSessionInfo = GetNode<GDScript>("/root/CurrentSessionInfo");
		playerMenuInfo = GetNode<GDScript>("/root/PlayerMenu");
	}


	// get IP and PORT (executed by all players
	public String GetHostIp()
	{
		return (String)currentSessionInfo.Get("host_ip");
	}
	public int GetHostPort()
	{
		return (int)currentSessionInfo.Get("host_port");
	}
	public int GetClientPort()
	{
		return (int)playerMenuInfo.Get("private_port");
	}

	// GET session necesary information
	public int GetAmountPlayers()
	{
		return (int)currentSessionInfo.Get("players");
	}
	public bool GetTeams()
	{
		return (bool)currentSessionInfo.Get("teams");
	}

	// get session user names (only server, then broadcast)
	public String[] GetSessionPlayerNames()
	{
		return (String[])currentSessionInfo.Get("players_list");
	}
	public bool GetIsServer()
	{
		return (bool)currentSessionInfo.Get("isServer");
	}
}
