using Godot;
using System;

public partial class GSCriptToCSharp : Node
{
	// get IP and PORT (executed by all players
	public String GetHostIp()
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return (String) currentSessionInfo.Get("host_ip");
	}
	public int GetHostPort()
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return (int)currentSessionInfo.Get("host_port");
	}
	public int GetClientPort()
	{
		var playerMenu = GetNode("/root/PlayerMenu");
		return (int)playerMenu.Get("peer_port");
	}

	// GET session necesary information
	public int GetAmountPlayers()
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return (int)currentSessionInfo.Get("players");
	}
	public bool GetTeams()
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return (bool)currentSessionInfo.Get("teams");
	}

	// get session user names (only server, then broadcast)
	public String[] GetSessionPlayerNames()
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return (String[])currentSessionInfo.Get("players_list");
	}
	public bool GetIsServer()
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return (bool)currentSessionInfo.Get("is_server");
	}
	public String GetName(int i)
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		String[] player_names = (String[])currentSessionInfo.Get("players_list");
		return player_names[i];
	}
	public String GetMyName()
	{
		var playerInfo = GetNode("/root/PlayerMenu");
		return (String)playerInfo.Get("usr_name");
	}
	public bool GetOS()
	{
		var currentSessionInfo = GetNode("/root/ConfigurationScript");
		return (bool)currentSessionInfo.Get("mobile"); // return if is mobile version or not
	}
	public String GetClientIp(int i)
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return ((String[])currentSessionInfo.Get("clients_ips"))[i];
	}
	public int GetClientPort(int i)
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return ((int[])currentSessionInfo.Get("clients_ports"))[i];
	}
}
