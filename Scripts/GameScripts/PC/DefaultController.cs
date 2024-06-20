using Godot;
using System;

/// <summary>
/// This class is used when controller used by Player is physical. It changes PlayerController control variable values to
/// move player, or to attack, jump...
/// </summary>
public partial class DefaultController : Node
{
	private PlayerController playerController;

	public PlayerController PlayerController { get => playerController; set => playerController = value; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetProcess(false);
		SetPhysicsProcess(false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// detect jump, movement, attack and ability use
		DetectMovement();
		DetectJump();
		DetectAttack();
		DetectAbility();
	}

	/// <summary>
	/// Detect if player has to move.
	/// </summary>
	public void DetectMovement()
	{
		if (Input.IsActionPressed("MoveRightDirection"))
			playerController.hdir = 1;
		else if (Input.IsActionPressed("MoveLeftDirection"))
			playerController.hdir = -1;
		else
			playerController.hdir = 0;

		if (Input.IsActionPressed("MoveDownDirection"))
			playerController.vdir = 1;
		else if (Input.IsActionPressed("MoveTopDirection"))
			playerController.vdir = -1;
		else
			playerController.vdir = 0;
	}

	/// <summary>
	/// Detect if player has to jump
	/// </summary>
	public void DetectJump()
	{
		if (Input.IsActionPressed("Jump"))
			playerController.jump = 1;
		else
			playerController.jump = 0;
	}

	/// <summary>
	/// Detect if player has to attack.
	/// </summary>
	public void DetectAttack()
	{
		if(Input.IsActionPressed("Attack"))
			playerController.AttackVar = true;
	}


	/// <summary>
	/// Detect if player has to use an ability
	/// </summary>
	public void DetectAbility()
	{
		if (Input.IsActionPressed("AbilityUsed"))
			playerController.AbilityUsed = true;
	}
}
