using Godot;
using System;
using System.Transactions;

public partial class PlayerController : RigidBody3D
{
	// important attributes
	private int vitality;
	private int maxVitality;

	private int speed;

	private int attackDamage;

	private RigidBody3D basicAttack;
	private CollisionShape3D basicAttackCollider;
	//private Ability ability;


	private RayCast3D raycast;

	// Animation Player
	private AnimationPlayer animationPlayer;

	// other attributes
	private int hdir = -1;
	private int vdir = -1;

	private int jump = -1;



	private bool can = false;
	private bool canAttack = false;
	private bool canJump = false;
	private bool canMove = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		vitality = 100;
		maxVitality = 100;
		speed = 4;
		attackDamage = 3;


		basicAttack = (RigidBody3D)GetNode("./BasicAttack");
		basicAttackCollider = (CollisionShape3D)GetNode("./BasicAttack/AttackCollisionShape");
		basicAttackCollider.SetDeferred("disabled", true);

		raycast = (RayCast3D)GetNode("./RayCast3D");

		animationPlayer = (AnimationPlayer)GetNode("./3DGodotRobot/AnimationPlayer");
		animationPlayer.Play("Idle");

		Start();

		ActivateJump();
	}

	// Used to detect input
	public override void _Process(double delta)
	{
		DetectMovement();
		DetectJump();
		
		SetAnimation();

		Attack();
	}

	// used to call physics functions
	public override void _PhysicsProcess(double delta)
	{
		//GD.Print(LinearVelocity);
		if(canJump)
			Move();
		Jump();
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
				//GD.Print("Collision!");
				if(collider.GetGroups().Contains("floor"))
				{
					//GD.Print("Floor collision!");
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
		if (Input.IsActionPressed("Attack"))
		{
			GD.Print("Attack used!");
			basicAttackCollider.SetDeferred("disabled", false);
			//DisableAttack();
		}
	}

	public async void DisableAttack()
	{
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
	}

	public void TakeDamage(int damage)
	{
		vitality -= damage;
	}

	public void Start()
	{
		can = true;
		canMove = true;
		canJump = true;
		canAttack = true;
	}

	public void SetAnimation()
	{
		// move and jump animations
		if ((LinearVelocity.X != 0 || LinearVelocity.Z != 0) && LinearVelocity.Y == 0)
			animationPlayer.Play("Run");
		else if (LinearVelocity.Y > 0)
			animationPlayer.Play("Jump");
		else if (LinearVelocity.Y < 0)
			animationPlayer.Play("Fall");
		else
			animationPlayer.Play("Idle");

		// attack animation


		// take damage animation


		// use ability animation (depends on ability?)


		// win animation


		// die animation
	}
	
	// signal emitted when body shape enters
	private void _on_body_shape_entered(Rid body_rid, Node body, long body_shape_index, long local_shape_index)
	{
		if(body.GetGroups().Contains("basicAttack") ||body.GetGroups().Contains("ability"))
		{
			TakeDamage(3);
		}
	}

}
