using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

/// <summary>
/// This class is used to manage GUI elements that are associated to a player.
/// </summary>
public partial class PlayerGUIController : ColorRect
{
	private PlayerController player;
	private ColorRect actualVitBar;
	private float vitBarInitialSizeX;
	private RichTextLabel textLabel;

	private TextureRect abilityTexture;

	public PlayerController Player { get => player; set => player = value; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// find nodes
		actualVitBar = (ColorRect)GetNode("./PlayerVit/PlayerActualVit");
		vitBarInitialSizeX = 250;

		abilityTexture = (TextureRect)GetNode("./AbilityTexture");

		textLabel = (RichTextLabel)GetNode("./PlayerName");
	}

	/// <summary>
	/// Refresh life bar.
	/// </summary>
	/// <param name="actualVit"></param>
	/// <param name="totalVit"></param>
	public void RefreshLifeBar(int actualVit, int totalVit)
	{
		float per = (float)actualVit / (float)totalVit;
		actualVitBar.Size = new Vector2(per * vitBarInitialSizeX, actualVitBar.Size.Y);
	}

	/// <summary>
	/// Ability has a texture to identify it.
	/// </summary>
	/// <param name="texture"></param>
	public void SetAbilityTexture(Texture2D texture)
	{
		abilityTexture.Texture = texture; // (player.Ability as Ability).GetTexture();
	}

	/// <summary>
	/// Set player username to see it in the game visually.
	/// </summary>
	/// <param name="name"></param>
	public void SetPlayerName(String name)
	{
		textLabel.Text = name;
	}
}
