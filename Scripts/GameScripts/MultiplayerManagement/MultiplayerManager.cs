using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// This class is responsible of managing the multiplayer when games are started.
/// </summary>
public partial class MultiplayerManager : Node
{
    private ENetMultiplayerPeer peer;
    private int connectedPlayers = 0;
    
    // Game Controller
    private GameController gameController;

    public override void _Process(double delta)
    {
        // say gameController that all players are connected
        if (connectedPlayers + 1 == gameController.AmountPlayers && !gameController.AllPlayersConnected)
            AllPlayersConnected();
    }

    /// <summary>
    /// Initialize MultiplayerAPI creating server and connecting client there.
    /// GameController calls this method to create server and clients
    /// </summary>
    public void InitializeNetworking(GameController gameController)
    {
        this.gameController = gameController;
         
        // connect to signals
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;

        // create server
        if (gameController.IsServer)
            CreateServer(gameController.HostPort);
        // create client
        else
            CreateClient(gameController.HostIp, gameController.HostPort, gameController.ClientPort);
    }

    /// <summary>
    /// Create a Server using Godot MultiplayerAPI
    /// </summary>
    /// <param name="port">Port in which server will be hearing</param>
    private void CreateServer(int port)
    {
        // create client
        peer = new ENetMultiplayerPeer();

        // check if server is correctly created
        Error err = peer.CreateServer(port, gameController.AmountPlayers - 1); // max clients + 1 (server)

        if (err != Error.Ok)
            return;

        // store peer in MultiplayerAPI singleton
        Multiplayer.MultiplayerPeer = peer;
    }

    /// <summary>
    /// Create client and connect to server.
    /// </summary>
    /// <param name="hostIp">IP address in which server is waiting for requests</param>
    /// <param name="hostPort">Port in which server is waiting for requests</param>
    /// <param name="localPort">Client port</param>
    private void CreateClient(String hostIp, int hostPort, int localPort)
    {
        // create client
        peer = new ENetMultiplayerPeer();

        // check there are no errors
        Error err = peer.CreateClient(hostIp, hostPort, 0, 0, 0, localPort); // communicate using localPort (almost hole punched)

        if (err != Error.Ok)
            return;

        // store peer in multiplayerAPI sinleton.
        Multiplayer.MultiplayerPeer = peer;
    }

    private void AllPlayersConnected()
    {
        gameController.AllPlayersConnected = true;
    }


    /* Signals */
    /// <summary>
    /// runs when the connection is succesful and only runs on the clients
    /// </summary>
    /// <param name="id"></param>
    private void PeerConnected(long id)
    {
        GD.Print(gameController.MyName + ": Player connected:" + id.ToString());
        connectedPlayers = Multiplayer.GetPeers().Length;
    }

    /// <summary>
    /// runs when a player disconnects and runs on all peers
    /// </summary>
    /// <param name="id"></param>
    private void PeerDisconnected(long id)
    {
        GD.Print(gameController.MyName + ": Player disconnected:" + id.ToString());
    }

    /// <summary>
    /// runs when the connection is succesful and only runs on the clients
    /// </summary>
    /// <param name="id"></param>
    private void ConnectedToServer()
    {
        GD.Print(gameController.MyName + ": Connected to server!");
    }

    /// <summary>
    /// runs when the connection fails and it runs only on the client
    /// </summary>
    /// <param name="id"></param>
    private void ConnectionFailed()
    {
        GD.Print(gameController.MyName + ": Connection to server failed!");
    }

    /* RPC functions */
    /// <summary>
    /// RPC function run in all clients that spawns an ability cube.
    /// </summary>
    /// <param name="position">Position in which ability cube will be spawned</param>
    /// <param name="abilityIndex">Index of the ability spawned</param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, CallLocal = true)]
    public void GenerateAbilityCube(Vector3 position, int abilityIndex)
    {
        var abilityCube = ResourceLoader.Load<PackedScene>("res://Scenes/GameScenes/AbilityCube/AbilityCube.tscn").Instantiate<AbilityCube>();
        abilityCube.GenerateAbility(abilityIndex);
        abilityCube.Position = position;
        gameController.Map.AddChild(abilityCube);
    }

    /// <summary>
    /// Uses ability in all clients.
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="hdir"></param>
    /// <param name="vdir"></param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, CallLocal = false)]
    public void UseOtherPlayerAbility(int playerIndex, int hdir, int vdir)
    {
        // get player and use ability
        PlayerController pPlayer = (PlayerController)GetNode("../Players/player" + playerIndex.ToString());
        pPlayer.LookingHDir = hdir;
        pPlayer.LookingVDir = vdir;
        ((GetNode("../Players/player" + playerIndex.ToString()) as PlayerController).Ability as Ability).Use();

        // quit ability texture
        (GetNode("../Players/player" + playerIndex.ToString()) as PlayerController).PlayerGUIController.SetAbilityTexture(null);
    }

}
