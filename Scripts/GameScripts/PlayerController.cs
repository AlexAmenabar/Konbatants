using Godot;
using System;
using System.Collections.Generic;
using System.Transactions;

public partial class PlayerController : RigidBody3D
{
	// player data to multiplayer game
	public int id;

	// player attributes
	private int vitality;
	private int maxVitality;
	private int speed;
	private int attackDamage;

	// nodes
	private NormalAttack basicAttack; // normal attack is an especial ability that player can use at any moment
	private RayCast3D raycast;

	// Animation Player
	private AnimationPlayer animationPlayer;

	// MultiplayerSynchonizer
	private MultiplayerSynchronizer multiplayerSync;

	// attributes to control in which direction is player lookin
	private int lookingHDir = 0;
	private int lookingVDir = 0;

	// "temporal" attributes to control movement
	public int hdir = 0;
	public int vdir = 0;
	public int jump = -1;

	// attributes to control player actions
	private bool can = false;
	private bool canAttack = false;
	private bool canJump = false;
	private bool canMove = false;

	// varibales used in animations
	private bool attackVar = false; // set true when player is going to attack
	private bool attacked = false;
	private bool damageReceived = false;

	// ability use
	private Node3D ability;
	private bool abilityUsed;

	// team
	private int team; // 0 or 1

	// game controller
	private GameController gameController;

	private PlayerGUIController playerGUIController;

	GSCriptToCSharp parser;

	private Node3D positionIndicatorArrow;

	private Node3D soundNodes;


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
	public int AttackDamage { get => attackDamage; set => attackDamage = value; }
	public bool AbilityUsed { get => abilityUsed; set => abilityUsed = value; }
	public MultiplayerSynchronizer MultiplayerSync { get => multiplayerSync; set => multiplayerSync = value; }
	public int LookingHDir { get => lookingHDir; set => lookingHDir = value; }
	public int LookingVDir { get => lookingVDir; set => lookingVDir = value; }


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetProcess(false);
		SetPhysicsProcess(false);
	}

	public void Initialize()
	{
		Variant vId = GetMeta("id");

		// initialize stats
		vitality = 100;
		maxVitality = 100;
		speed = 4;
		attackDamage = 3;

		// initialize nodes related to player
		basicAttack = (NormalAttack)GetNode("./BasicAttack");
		//basicAttackCollider = (CollisionShape3D)GetNode("./BasicAttack/AttackCollisionShape");
		//basicAttackCollider.SetDeferred("disabled", true);
		//(basicAttack as Node3D).Visible = false;

		raycast = (RayCast3D)GetNode("./RayCast3D");

		animationPlayer = (AnimationPlayer)GetNode("./AnimationPlayer");
		animationPlayer.Play("Idle");

		// get this player node index and unique id to set authority

		// get multiplayer sync
		multiplayerSync = (MultiplayerSynchronizer)GetNode("./MultiplayerSynchronizer");

		// player authority is set in order to multiplayer ids (setted in connection order)
		// array with all peers id-s
		List<int> allpeers;
		int myId = Multiplayer.GetUniqueId();

		allpeers = new List<int>();
		for (int i = 0; i < Multiplayer.GetPeers().Length; i++)
			allpeers.Add(Multiplayer.GetPeers()[i]);

		allpeers.Add(myId);
		allpeers.Sort();

		int playerIndex = this.Name.ToString().Split("r")[1].ToInt();

		id = allpeers[playerIndex];


		// set player GUI
		//playerGUIController = GetNode<PlayerGUIController>("../../CanvasLayer/p" + playerIndex.ToString());

		// SetMultiplayerAuthority(id);
		CallDeferred("set_multiplayer_authority", id);

		parser = (GSCriptToCSharp)GetNode("../../ParserNode");
		soundNodes = (Node3D)GetNode("./SoundNodes");

		gameController = (GameController)GetNode("../..");
	}
	public async void Start()
	{
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		if (IsMultiplayerAuthority())
		{
			can = true;
			canMove = true;
			canJump = true;
			canAttack = true;
			ActivateJump();
			SetPhysicsProcess(true);
			SetProcess(true);

			// set controller player
			parser = (GSCriptToCSharp)GetNode("../../ParserNode");
			if (parser.GetOS()) // Android
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

	// Used to detect input
	public override void _Process(double delta)
	{
		// move arrow that indicates of which team is each player
		positionIndicatorArrow.Position = new Vector3(Position.X, Position.Y + 0.5f, Position.Z);

		if(IsMultiplayerAuthority())//if (multiplayerSync.GetMultiplayerAuthority() == id)
		{
			//DetectMovement();
			//DetectJump();
			//DetectAttack();

			SetAnimation();
			//Attack();
		}
	}

	// used to call physics functions
	public override void _PhysicsProcess(double delta)
	{
		if(IsMultiplayerAuthority() && can)//if (multiplayerSync.GetMultiplayerAuthority() == id)
		{
			if (canJump)
				Move();
			Jump();

			// normalAttack
			if (AttackVar)
			{
				AttackVar = false;
				Attack();
			}

			// use ability
			if (abilityUsed && ability != null)
			{
				//GD.Print("Ability used!");
				abilityUsed = false;
				(ability as Ability).Use();

				// ability use is not synchronized by multiplayer sync, so call RCP function
				String nodeName = Name;
				int myIndex = nodeName.Split("r")[1].ToInt();
				gameController.AbilityUsed(myIndex, lookingHDir, lookingVDir);

				// set ability texture
				playerGUIController.SetAbilityTexture(null); // quit texture
			}
			else
				abilityUsed = false;
		}
	}

	public void DetectMovement()
	{
		if (Input.IsActionPressed("MoveRightDirection"))
			hdir = 1;
		else if (Input.IsActionPressed("MoveLeftDirection"))
			hdir = -1;
		else
			hdir = 0;
		if (Input.IsActionPressed("MoveDownDirection"))
			vdir = 1;
		else if (Input.IsActionPressed("MoveTopDirection"))
			vdir = -1;
		else
			vdir = 0;

		if (vdir != 0 || hdir != 0)
			(soundNodes.GetNode("./RunSound") as AudioStreamPlayer3D).Play();
		else
			(soundNodes.GetNode("./RunSound") as AudioStreamPlayer3D).Stop();
	}
	public void Move()
	{
		// resulting velocity must be velocity variable
		float vely = LinearVelocity.Y;
		float velz = LinearVelocity.Z;
		float velx = LinearVelocity.X;

		// looking direction
		if (hdir != 0)
			lookingHDir = hdir;
		if (vdir != 0)
			lookingVDir = vdir;
		if (hdir != 0 && vdir == 0)
			lookingVDir = 0;
		if (hdir == 0 && vdir != 0)
			lookingHDir = 0;

		// movement
		velx = hdir * speed;
		velz = vdir * speed;


		if(hdir != 0 && vdir != 0) // calculate velocity in each dimension to resultant be 4
		{
			// velx == velz

			float x = (float)(Math.Sqrt((speed * speed) / 2));

			//velx = (float)Math.Sqrt(8.0f) * hdir;
			//velz = (float)Math.Sqrt(8.0f) * vdir;

			velx = velz = x;
			velx *= hdir;
			velz *= vdir;
		}

		LinearVelocity = new Vector3(velx, LinearVelocity.Y, velz);

		// rotation
		if(hdir!=0 || vdir != 0)
			LookAt(new Vector3(-hdir * 200, Position.Y, -vdir*200), Vector3.Up);

		//GD.Print("Moving player: (" + velx.ToString() + ", " + vely.ToString() + ", " + velz.ToString());
	}

	public void DetectJump()
	{
		if (Input.IsActionPressed("Jump"))
			jump = 1;
		else
			jump = 0;
	}

	public void Jump()
	{
		int mlt = 50;
		if(jump==1 && canJump) // and raycast floor 
		{
			// souns
			(soundNodes.GetNode("./JumpSound") as AudioStreamPlayer3D).Play();

			float velz = LinearVelocity.Z;
			float velx;
			if (hdir == 1)
				velx = 10;
			else if (hdir == 0)
				velx = -10;
			else
				velx = 0;

			if (vdir == 1) velz = -10;
			else if (vdir == 0) velz = 10;
			else velz = 0;

			ApplyCentralImpulse(new Vector3(velx, 6 * mlt, velz));
			jump = 0;
			canJump = false;
		}
	}

	public async void ActivateJump()
	{
		while(true)
		{ 
			Node3D collider = (Node3D)raycast.GetCollider();
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

	public void Attack()
	{
		if (canAttack)
		{
			basicAttack.Use();
		}
	}

	public void TakeDamage(Attack attack)
	{
		// play sound
		(soundNodes.GetNode("./TakeDamageSound") as AudioStreamPlayer3D).Play();
		
		//GD.Print("Damage taken! Max vitality = " + maxVitality + ", Vitality = " + vitality);
		
		// reduce vitality and run animation
		vitality -= attack.Damage;

		// push player
		PushPlayer(attack);

		RefreshDamageReceivedAnimation();

		// refresh vitality bar
		RefreshLifeBar();
	}
	public void PushPlayer(Attack attack)
	{
		// can not do anything
		can = false;

		float forceX = 0, forceZ = 0;

		// calculate force in each dimension

		// calculate distance between attack and player
		Vector3 distance = Position - attack.Position;
		if(Math.Abs(distance.X) > 0.1f && Math.Abs(distance.Z) > 0.1f)
		{
			// distribute HorizontalPushForce in X and Z
			forceX = forceZ = (float)Math.Sqrt(attack.HorizontalPushForce * attack.HorizontalPushForce / 2);
		}
		else if (Math.Abs(distance.X) > 0.1f)
		{
			forceX = attack.VerticalPushForce;
		}
		else // distanceZ > 0.1
		{
			forceZ = attack.HorizontalPushForce;
		}

		ApplyImpulse(new Vector3(forceX * 10, attack.VerticalPushForce * 10, forceZ * 10));
	}

	public void RefreshLifeBar()
	{
		playerGUIController.RefreshLifeBar(vitality, maxVitality);
	}

	public async void RefreshDamageReceivedAnimation()
	{
		can = false;
		damageReceived = true;
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		damageReceived = false;
		can = true;
	}

	public void SetAnimation()
	{
		// move and jump animations

		// attack animation
		if (attacked)
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

		// use ability animation (depends on ability?)


		// win animation


		// die animation
	}

	// emitted when player colides with ability cube. It sets an ability to player
	private void _on_body_shape_entered(Rid body_rid, Node body, long body_shape_index, long local_shape_index)
	{
		if(body.IsInGroup("AbilityCube"))
		{
			// play sound
			(GetNode("./SoundNodes/GetCubeSound") as AudioStreamPlayer3D).Play();

			//GD.Print("Ability cube collided");
			AbilityCube abilityCube = (AbilityCube)body;

			// get ability node and make map child
			Node3D abilityTempNode = abilityCube.GetAbility(); // sometimes player collides with the same cube more than one time
			if (abilityTempNode != null)
			{
				//GD.Print("Ability is null? " + (ability == null).ToString());

				// if player has an ability delete (child node)
				if(ability != null)
				{
					GetNode("./Ability").GetChild(0).QueueFree();
				}

				abilityTempNode.Reparent(GetNode("./Ability")); // set player as new parent

				// set player ability and ability player
				this.ability = abilityTempNode;

				// set ability playerController
				(ability as Ability).SetPlayer(this);

				// set ability texture
				playerGUIController.SetAbilityTexture((ability as Ability).GetTexture());

				// Destroy abilityCube
				abilityCube.Destroy();
			}
		}
	}
}
