using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;

/// <summary>
/// Class that controls player actions.
/// </summary>
public partial class PlayerController : RigidBody3D
{
	/* Player data to multiplayer game (MultiplayerAPI id) */
	private int id; 

	/* Player attributes */
	private int vitality;
	private int maxVitality;
	private int speed;

	/* Basic attack node */
	private NormalAttack basicAttack; // normal attack is an especial ability that player can use at any moment
	
	/* RayCast to detect when player hit the ground */ 
	private RayCast3D raycast;

	/* Animation Player used to play animations */
	private AnimationPlayer animationPlayer;

	/* MultiplayerSynchonizer used to synchronize properties with other players */
	private MultiplayerSynchronizer multiplayerSync;

	/* Control attributes */
	// attributes to control in which direction player is lookin
	private int lookingHDir = 0;
	private int lookingVDir = 0;

	// "temporal" attributes to control movement (this variables are changed by the controller)
	public int hdir = 0;
	public int vdir = 0;
	public int jump = -1;

	// attributes to control player actions
	private bool isAlive = true;
	private bool can = false;
	private bool canAttack = false;
	private bool canJump = false;
	private bool canMove = false;
	private bool winner = false;

	// varibales used in animations
	private bool attackVar = false; // set true when player is going to attack (changed by the controller)
	private bool attacked = false; // used when player attacked
	private bool damageReceived = false;

	/* Ability related attributes*/
	private Node3D ability;
	private bool abilityUsed; // control when ability is used

	/* player team */
	private int team; // 0 or 1 (for now)

	/* GameController reference */
	private GameController gameController;

	/* Player specific GUI reference to refresh vitality bar and set username */
	private PlayerGUIController playerGUIController;

	/* Parser to get info from GDSCript when neccesary */
	private GSCriptToCSharp parser;

	/* Arrow above player to indicate which model is player controlling and teams */
	private Node3D positionIndicatorArrow;

	/* Node with sounds to be played depending on player actions */
	private Node3D soundNodes;


	/* Getter and setters */
	public int Team { get => team; set => team = value; }
	public PlayerGUIController PlayerGUIController { get => playerGUIController; set => playerGUIController = value; }
	public bool AttackVar { get => attackVar; set => attackVar = value; }
	public Node3D PositionIndicatorArrow { get => positionIndicatorArrow; set => positionIndicatorArrow = value; }
	public Node3D SoundNodes { get => soundNodes; set => soundNodes = value; }
	public Node3D Ability { get => ability; set => ability = value; }
	public bool Attacked { get => attacked; set => attacked = value; }
	public Node3D SoundNodes1 { get => soundNodes; set => soundNodes = value; }
	public int MaxVitality { get => maxVitality; set => maxVitality = value; }
	public int Vitality { get => vitality; set => vitality = value; }
	public int Speed { get => speed; set => speed = value; }
	public bool AbilityUsed { get => abilityUsed; set => abilityUsed = value; }
	public MultiplayerSynchronizer MultiplayerSync { get => multiplayerSync; set => multiplayerSync = value; }
	public int LookingHDir { get => lookingHDir; set => lookingHDir = value; }
	public int LookingVDir { get => lookingVDir; set => lookingVDir = value; }
	public bool Can { get => can; set => can = value; }
	public int Id { get => id; set => id = value; }
	public bool Winner { get => winner; set => winner = value; }


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// player can not move until game starts
		SetProcess(false);
		SetPhysicsProcess(false);
	}

	/// <summary>
	/// Initialize player variables and find nodes in player scene tree. 
	/// Also set multiplayer authority to each playerController node (not only this).
	/// This function is called from GameController when spawning Player Nodes.
	/// </summary>
	public void Initialize()
	{
		Variant vId = GetMeta("id");

		// initialize stats
		vitality = 100;
		maxVitality = 100;
		speed = 4;

		// initialize nodes related to player
		basicAttack = (NormalAttack)GetNode("./BasicAttack");
		raycast = (RayCast3D)GetNode("./RayCast3D");
		animationPlayer = (AnimationPlayer)GetNode("./AnimationPlayer");
		parser = (GSCriptToCSharp)GetNode("../../ParserNode");
		soundNodes = (Node3D)GetNode("./SoundNodes");
		gameController = (GameController)GetNode("../..");

		// get multiplayer sync
		multiplayerSync = (MultiplayerSynchronizer)GetNode("./MultiplayerSynchronizer");

		// player authority is set in order to multiplayer ids (setted in connection order)
		// array with all peer id-s
		List<int> allpeers;
		int myId = Multiplayer.GetUniqueId(); // get my  multiplayer id

		allpeers = new List<int>();
		for (int i = 0; i < Multiplayer.GetPeers().Length; i++)
			allpeers.Add(Multiplayer.GetPeers()[i]);

		allpeers.Add(myId);
		allpeers.Sort();

		int playerIndex = this.Name.ToString().Split("r")[1].ToInt();

		id = allpeers[playerIndex];

		// SetMultiplayerAuthority(id);
		CallDeferred("set_multiplayer_authority", id);
	}

	/// <summary>
	/// Called by GameController in all PlayerController nodes to start game.
	/// After this call players can start moving, jumping... 
	/// Also controller is activated.
	/// </summary>
	public async void Start()
	{
		await ToSignal(GetTree().CreateTimer(1), "timeout");

		// if this is not multiplayerAuthority nothing
		if (IsMultiplayerAuthority())
		{
			// activate option to move, jump and attack
			can = true;
			canMove = true;
			canJump = true;
			canAttack = true;
			ActivateJump(); // controls when player can jump again
			SetPhysicsProcess(true);
			SetProcess(true);

			// activate the controller that will be used
			if (parser.GetOS()) // Android (activate virtualController)
			{
				Control virtualController = (Control)GetNode("../../Controllers/VirtualController");
				virtualController.Visible = true;
				virtualController.Set("player_controller", this);
			}
			else // PC
			{
				DefaultController defaultController = (DefaultController)GetNode("../../Controllers/DefaultController");
				defaultController.PlayerController = this;
				defaultController.SetProcess(true);
				defaultController.SetPhysicsProcess(true);
			}
		}
	}

	/// <summary>
	/// This function is called each frame and is used to manage some deatils as position indicator arrow position and 
	/// current animation.
	/// </summary>
	/// <param name="delta">Time between the last and this frame</param>
	public override void _Process(double delta)
	{
		// move player indicator arrow in each frame
		positionIndicatorArrow.Position = new Vector3(Position.X, Position.Y + 0.5f, Position.Z);

		// set animation each frame
		if(IsMultiplayerAuthority())
			SetAnimation();
	}

	/// <summary>
	/// Used to call physics functions, this method is called depending on Physics Engine configurations. Time between exections is constant.
	/// </summary>
	/// <param name="delta"></param>
	public override void _PhysicsProcess(double delta)
	{
		// player can variable is used to control if it can move
		if(IsMultiplayerAuthority() && can)
		{
			if (canJump)
				Move();
			Jump();

			// normal attack
			if (AttackVar) // attackVar is changed by controller scripts
			{
				AttackVar = false;
				Attack();
			}

			// use ability
			if (abilityUsed && ability != null) // abilityUsed is controlled by controller scripts
			{
				abilityUsed = false;

				// ability use is not synchronized by multiplayer sync, so call RCP function to inform all clients
				int myIndex = Name.ToString().Split("r")[1].ToInt(); // get player index
				gameController.AbilityUsed(myIndex, lookingHDir, lookingVDir);

				// set ability texture
				playerGUIController.SetAbilityTexture(null); // quit texture
				
				(ability as Ability).Use(); // use ability
			}
			else
				abilityUsed = false;
		}
	}

	/// <summary>
	/// Move player using control variables.
	/// </summary>
	public void Move()
	{
		// looking direction
		if (hdir != 0)
			lookingHDir = hdir;
		if (vdir != 0)
			lookingVDir = vdir;
		if (hdir != 0 && vdir == 0)
			lookingVDir = 0;
		if (hdir == 0 && vdir != 0)
			lookingHDir = 0;

		// movement speed in each direction depends on speed
		float velx = hdir * speed;
		float velz = vdir * speed;

		// resulting speed must be always speed variable
		if(hdir != 0 && vdir != 0) 
		{
			// velx == velz -> true
			float x = (float)(Math.Sqrt((speed * speed) / 2));
			velx = velz = x;

			// set direction
			velx *= hdir; 
			velz *= vdir;
		}
		LinearVelocity = new Vector3(velx, LinearVelocity.Y, velz);

		// if player is moving play sound
		if (vdir != 0 || hdir != 0)
			(soundNodes.GetNode("./RunSound") as AudioStreamPlayer3D).Play();
		else
			(soundNodes.GetNode("./RunSound") as AudioStreamPlayer3D).Stop();

		// look always on looking direction
		if (hdir!=0 || vdir != 0)
			LookAt(new Vector3(-hdir * 200, Position.Y, -vdir*200), Vector3.Up);
	}

	/// <summary>
	/// Player jump depending on control variable values.
	/// </summary>
	public void Jump()
	{
		int mlt = 50;
		if(jump==1 && canJump) // jump is set to 1 by controllers
		{
			// play sound
			(soundNodes.GetNode("./JumpSound") as AudioStreamPlayer3D).Play();

			float velz;
			float velx;
			
			if (hdir == 1) velx = speed;
			else if (hdir == 0) velx = -10;
			else velx = 0;
			
			if (vdir == 1) velz = -10;
			else if (vdir == 0) velz = 10;
			else velz = 0;

			ApplyCentralImpulse(new Vector3(velx, 6 * mlt, velz));
			jump = 0;
			canJump = false; // player can not jump until time pass
		}
	}

	/// <summary>
	/// Set can jump to true when:
	///		player touches the ground
	///		time to activate canJump passed
	/// </summary>
	public async void ActivateJump()
	{
		while(true)
		{ 
			Node3D collider = (Node3D)raycast.GetCollider(); // wait until raycast collides with floor
			if(collider != null)
			{
				if(collider.GetGroups().Contains("floor"))
				{
					canJump = true;
					LinearVelocity = new Vector3(LinearVelocity.X, 0, LinearVelocity.Z);
					await ToSignal(GetTree().CreateTimer(0.75f), "timeout");
				}
			}
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		}
	}

	/// <summary>
	/// Player uses normal attack depending on control variables and attack cooldown.
	/// </summary>
	public void Attack()
	{
		if (canAttack)
			basicAttack.Use();
	}

	/// <summary>
	/// Player takes damage and is pushed depending on attack damage and push force.
	/// </summary>
	/// <param name="attack">attack that collided with player</param>
	public void TakeDamage(Attack attack)
	{
		// play sound
		(soundNodes.GetNode("./TakeDamageSound") as AudioStreamPlayer3D).Play();
				
		// reduce vitality and run animation
		vitality -= attack.Damage;
		RefreshDamageReceivedAnimation();
		
		// push player using attack push force
		PushPlayer(attack);

		// refresh vitality bar
		RefreshLifeBar();

		// check if player died
		if(vitality <= 0)
			Die();
	}

	/// <summary>
	/// Called when player falls from the map or vitality <= 0.
	/// A player that died can not do anything
	/// </summary>
	public void Die()
	{
		SetPhysicsProcess(false);
		SetProcess(false);
		Freeze = true;
		isAlive = false;

		gameController.PlayerDied(this);
	}

	/// <summary>
	/// Pushes player using attack push force.
	/// </summary>
	/// <param name="attack">attack that collided with player</param>
	public void PushPlayer(Attack attack)
	{
		// player can not do anything until push effect finished
		can = false;

		// calculate force in each dimension
		float forceX = 0, forceZ = 0;

		// calculate distance between attack and player
		Vector3 distance = Position - attack.Position;
		int xdir = 0, zdir = 0;

		// push force magnitude must be the same in all directions
		if (Math.Abs(distance.X) > 0.1f && Math.Abs(distance.Z) > 0.1f)
			// distribute HorizontalPushForce in X and Z
			forceX = forceZ = (float)Math.Sqrt(attack.HorizontalPushForce * attack.HorizontalPushForce / 2);
		
		else if (Math.Abs(distance.X) > 0.1f)
			forceX = attack.HorizontalPushForce;
		
		else // distanceZ > 0.1
			forceZ = attack.HorizontalPushForce;

		// set direction to push
		if (distance.X > 0) xdir = 1;
		else if (distance.X < 0) xdir = -1;

		if (distance.Z > 0) zdir = 1;
		else if (distance.Z < 0) zdir = -1;

		ApplyImpulse(new Vector3(forceX * 10 * xdir, attack.VerticalPushForce * 10, forceZ * 10 * zdir));
	}

	/// <summary>
	/// Calls PlayerGUIController function to refresh vitality bar
	/// </summary>
	public void RefreshLifeBar()
	{
		playerGUIController.RefreshLifeBar(vitality, maxVitality);
	}

	/// <summary>
	/// Controls player behavior when it collides with an attack.
	/// </summary>
	public async void RefreshDamageReceivedAnimation()
	{
		can = false;
		damageReceived = true;
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		damageReceived = false;
		can = true;
	}

	/// <summary>
	/// Sets player animations in each frame depending on control variables values.
	/// </summary>
	public void SetAnimation()
	{
		if (winner)
			animationPlayer.Play("Emote2");
		else if (!isAlive)
			animationPlayer.Play("Hurt");

		// attack animation
		else if (attacked)
			animationPlayer.Play("Attack1");

		// take damage animation
		else if (damageReceived)
			animationPlayer.Play("Crouch");

		// movement, jump, fall and idle dependes on velocities
		else if ((LinearVelocity.X != 0 || LinearVelocity.Z != 0) && LinearVelocity.Y == 0)
			animationPlayer.Play("Run");
		else if (LinearVelocity.Y > 0)
			animationPlayer.Play("Jump");
		else if (LinearVelocity.Y < 0)
			animationPlayer.Play("Fall");
		else
			animationPlayer.Play("Idle");

		// ability animation?
	}

	/// <summary>
	/// Emitted when player colides with ability cube. It sets an ability to player.
	/// </summary>
	/// <param name="body_rid"></param>
	/// <param name="body">Body that collided with player</param>
	/// <param name="body_shape_index"></param>
	/// <param name="local_shape_index"></param>
	private void _on_body_shape_entered(Rid body_rid, Node body, long body_shape_index, long local_shape_index)
	{
		// check if body is an ability cube
		if (body.IsInGroup("AbilityCube"))
		{
			// play sound of getting an ability cube
			(GetNode("./SoundNodes/GetCubeSound") as AudioStreamPlayer3D).Play();

			// get ability node and make map child
			AbilityCube abilityCube = (AbilityCube)body;
			Node3D abilityTempNode = abilityCube.GetAbility(); 
			if (abilityTempNode != null) // sometimes player collides with the same cube more than ones, so check if ability cube has an ability
			{
				// if player has an ability remove (and he does not used it) from child list
				if (ability != null && !(ability as Ability).AbilityIsUsed())
				{
					GetNode("./Ability").GetChild(0).QueueFree();
					abilityTempNode.Reparent(GetNode("./Ability")); // set player as new parent
				}

				// if player has an ability but is used (it will be deleted automatically) add child in 0 position (actual ability position)
				else if (ability != null) // and ability is used
				{
					abilityTempNode.Reparent(GetNode("./Ability")); // set player as new parent
					GetNode("./Ability").MoveChild(abilityTempNode, 0);
				}

				// ability is null
				else
					abilityTempNode.Reparent(GetNode("./Ability")); // set player as new parent

				SetAbility(abilityTempNode);

				// Destroy abilityCube
				abilityCube.Destroy();
			}
		}
	}

	/// <summary>
	/// Sets and ability to player
	/// </summary>
	/// <param name="ability"></param>
	public void SetAbility(Node3D ability)
	{
		// set player ability and ability player
		this.ability = ability;

		// set playerController to ability
		(ability as Ability).SetPlayer(this);

		// set ability texture on GUI 
		playerGUIController.SetAbilityTexture((ability as Ability).GetTexture());

	}
}
