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
	private Area3D basicAttack;
	private CollisionShape3D basicAttackCollider;
	//private Ability ability;
	private RayCast3D raycast;

	// Animation Player
	private AnimationPlayer animationPlayer;

	// MultiplayerSynchonizer
	private MultiplayerSynchronizer multiplayerSync;

	// "temporal" attributes to control
	private int hdir = -1;
	private int vdir = -1;
	private int jump = -1;

	// attributes to control player actions
	private bool can = false;
	private bool canAttack = false;
	private bool canJump = false;
	private bool canMove = false;

	// varibales used in animations
	private bool attacked = false;
	private bool damageReceived = false;


	public override void _EnterTree()
	{

	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Variant vId = GetMeta("id");
		
		id = vId.AsInt32();

		GD.Print(id);
		vitality = 100;
		maxVitality = 100;
		speed = 4;
		attackDamage = 3;

		basicAttack = (Area3D)GetNode("./BasicAttack");
		basicAttackCollider = (CollisionShape3D)GetNode("./BasicAttack/AttackCollisionShape");
		basicAttackCollider.SetDeferred("disabled", true);
		basicAttack.Visible = false;

		raycast = (RayCast3D)GetNode("./RayCast3D");

		animationPlayer = (AnimationPlayer)GetNode("./3DGodotRobot/AnimationPlayer");
		animationPlayer.Play("Idle");

		// set multiplayer authority
		multiplayerSync = (MultiplayerSynchronizer)GetNode("./MultiplayerSynchronizer");

		// get this player node index and unique id to set authority

		GD.Print("Printing peers");
		foreach(int peer in Multiplayer.GetPeers())
		{
			GD.Print(peer.ToString());
		}

		// player authority is set in order to multiplayer ids
		
		// array with all peers id-s
		List<int> allpeers;
		int myId = Multiplayer.GetUniqueId();

		allpeers = new List<int>();
		for(int i = 0; i<Multiplayer.GetPeers().Length; i++)
		{
			if(myId > Multiplayer.GetPeers()[i])
			{
				allpeers.Add(Multiplayer.GetPeers()[i]);
				allpeers.Add(myId);
			}
			else
			{
				allpeers.Add(myId);
				allpeers.Add(Multiplayer.GetPeers()[i]);
			}
		}

		int playerIndex = this.Name.ToString().Split("r")[1].ToInt();
		id = allpeers[playerIndex];
		

		//multiplayerSync.SetMultiplayerAuthority(id);
		//multiplayerSync.CallDeferred("set_multiplayer_authority", id);

		GD.Print("Server " + Multiplayer.IsServer().ToString() + ", Authority id is " + id);

		//SetMultiplayerAuthority(id);
		CallDeferred("set_multiplayer_authority", id);
		
		Start();
	}

	// Used to detect input
	public override void _Process(double delta)
	{
		//SetMultiplayerAuthority(id);
		if(IsMultiplayerAuthority())
		{
			//GD.Print("In Node " + Name + " id is " + id.ToString() + " is multiplayer authority for this node");
		}
		if(IsMultiplayerAuthority())//if (multiplayerSync.GetMultiplayerAuthority() == id)
		{
			DetectMovement();
			DetectJump();

			SetAnimation();
			Attack();
		}
	}

	// used to call physics functions
	public override void _PhysicsProcess(double delta)
	{
		
		if(IsMultiplayerAuthority())//if (multiplayerSync.GetMultiplayerAuthority() == id)
		{
			if (canJump)
				Move();
			Jump();
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
	}
	public void Move()
	{
		// resulting velocity must be velocity variable
		float vely = LinearVelocity.Y;
		float velz = LinearVelocity.Z;
		float velx = LinearVelocity.X;
		
		if (hdir == 1) velx = 4;
		else if (hdir == -1) velx = -4;
		else velx = 0;

		if (vdir == 1) velz = 4;
		else if (vdir == -1) velz = -4;
		else velz = 0;


		if(hdir != 0 && vdir != 0) // calculate velocity in each dimension to resultant be 4
		{
			velx = (float)Math.Sqrt(8.0f) * hdir;
			velz = (float)Math.Sqrt(8.0f) * vdir;
		}

		LinearVelocity = new Vector3(velx, LinearVelocity.Y, velz);

		// rotation
		if(hdir!=0 || vdir != 0)
			LookAt(new Vector3(-hdir * 200, Position.Y, -vdir*200), Vector3.Up);

		GD.Print("Moving player: (" + velx.ToString() + ", " + vely.ToString() + ", " + velz.ToString());
		
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
		if (Input.IsActionPressed("Attack") && canAttack)
		{
			GD.Print("Attack used!");
			attacked = true;
			RefreshAttacketAnimation();
			basicAttackCollider.SetDeferred("disabled", false);
			canAttack = false;
			basicAttack.Visible = true;
			DisableAttack();
		}
	}

	private async void RefreshAttacketAnimation()
	{
		await ToSignal(GetTree().CreateTimer(1.1f), "timeout");
		attacked = false;
	}

	public async void DisableAttack()
	{
		await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
		basicAttack.Visible = false;
		basicAttackCollider.SetDeferred("disabled", true); // desactive attack collider
		await ToSignal(GetTree().CreateTimer(0.75f), "timeout");
		canAttack = true;
	}

	public void TakeDamage(int damage)
	{
		vitality -= damage;
		damageReceived = true;
		RefreshDamageReceivedAnimation();
	}

	private async void RefreshDamageReceivedAnimation()
	{
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		damageReceived = false;
	}

	public async void Start()
	{
		await ToSignal(GetTree().CreateTimer(5), "timeout");
		if(IsMultiplayerAuthority())
		{ 
			can = true;
			canMove = true;
			canJump = true;
			canAttack = true;
			ActivateJump();
		}
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
	
	// signal emitted when body shape enters
	private void _on_basic_attack_body_entered(Node3D body)
	{
		if (body.GetGroups().Contains("player"))
		{
			PlayerController enemyPlayer = (PlayerController)body;
			enemyPlayer.TakeDamage(attackDamage);
			basicAttackCollider.SetDeferred("disabled", true);
			GD.Print("Damage taken\n");
		}
	}
}
