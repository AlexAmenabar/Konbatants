using Godot;
using System;
using System.Collections.Generic;

public partial class Session
{
	private int ownerId;
	private bool teams;
	private int playerCount;
	private int connectedPlayerCount;
	private List<int> connectedPlayers;

	public Session(int ownerId, bool teams, int playerCount)
	{
		this.ownerId = ownerId;
		this.teams = teams;
		this.playerCount = playerCount;
		this.connectedPlayerCount = 1; //owner is already connected to the server
	}

	private int PlayerConnected(int playerId)
	{
		if (connectedPlayers.Contains(playerId))
		{
			return 0; //player is already in the list --> (CHANGE BY AN EXCEPTION???)
		}

		//add the player to the list
		connectedPlayerCount++;
		connectedPlayers.Add(playerId); 
		return 1; //player added succesfully
	}
	private bool PlayerDisconnected(int playerId)
	{
		return connectedPlayers.Remove(playerId);
	}
}
