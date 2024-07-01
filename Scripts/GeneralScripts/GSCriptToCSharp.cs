using Godot;
using System;

/// <summary>
/// This class parses information from GDSCript to C# 
/// </summary>
public partial class GSCriptToCSharp : Node
{
	/// <summary>
	/// Get session server IP.
	/// </summary>
	/// <returns></returns>
	public String GetHostIp()
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return (String) currentSessionInfo.Get("host_ip");
	}

	/// <summary>
	/// Get session server port.
	/// </summary>
	/// <returns></returns>
	public int GetHostPort()
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return (int)currentSessionInfo.Get("host_port");
	}

	/// <summary>
	/// Get client port (not executed by server).
	/// </summary>
	/// <returns></returns>
	public int GetClientPort()
	{
		var playerMenu = GetNode("/root/PlayerMenu");
		return (int)playerMenu.Get("peer_port");
	}

	/// <summary>
	/// GET session player amount
	/// </summary>
	/// <returns></returns>
	public int GetAmountPlayers()
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return (int)currentSessionInfo.Get("players");
	}

	/// <summary>
	/// Get if there are teams in this session
	/// </summary>
	/// <returns></returns>
	public bool GetTeams()
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return (bool)currentSessionInfo.Get("teams");
	}

	/// <summary>
	/// Get session user names (only server, then broadcast)
	/// </summary>
	/// <returns></returns>
	public String[] GetSessionPlayerNames()
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return (String[])currentSessionInfo.Get("players_list");
	}

	/// <summary>
	/// Get if this process is the server of the game or not.
	/// </summary>
	/// <returns></returns>
	public bool GetIsServer()
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return (bool)currentSessionInfo.Get("is_server");
	}

	/// <summary>
	/// Get Player username.
	/// </summary>
	/// <param name="i">Index of the player</param>
	/// <returns></returns>
	public String GetName(int i)
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		String[] player_names = (String[])currentSessionInfo.Get("players_list");
		return player_names[i];
	}

	/// <summary>
	/// Get this player name.
	/// </summary>
	/// <returns></returns>
	public String GetMyName()
	{
		var playerInfo = GetNode("/root/PlayerMenu");
		return (String)playerInfo.Get("usr_name");
	}

	/// <summary>
	/// Get operative system in which game is being executed.
	/// </summary>
	/// <returns></returns>
	public bool GetOS()
	{
		var currentSessionInfo = GetNode("/root/ConfigurationScript");
		return (bool)currentSessionInfo.Get("mobile"); // return if is mobile version or not
	}

	/// <summary>
	/// Used on testing
	/// </summary>
	/// <param name="i"></param>
	/// <returns></returns>
	public String GetClientIp(int i)
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return ((String[])currentSessionInfo.Get("clients_ips"))[i];
	}

	/// <summary>
	/// Used on testing
	/// </summary>
	/// <param name="i"></param>
	/// <returns></returns>
	public int GetClientPort(int i)
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return ((int[])currentSessionInfo.Get("clients_ports"))[i];
	}

	/// <summary>
	/// Get map name
	/// </summary>
	/// <returns>Map name</returns>
	public String GetMapName()
	{
		var currentSessionInfo = GetNode("/root/CurrentSessionInfo");
		return (String)currentSessionInfo.Get("map_name");
	}
}
