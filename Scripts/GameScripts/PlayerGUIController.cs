using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

public partial class PlayerGUIController : Node
{
	private PlayerController player;
	private ColorRect actualVitBar;
	private int vitBarInitialSizeX;
	private RichTextLabel textLabel;

	private TextureRect abilityTexture;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// find nodes
		actualVitBar = (ColorRect)GetNode("./PlayerVit/PlayerActualVit");
		vitBarInitialSizeX = 300;

		abilityTexture = (TextureRect)GetNode("./AbilityTexture");

		textLabel = (RichTextLabel)GetNode("./PlayerName");
	}


	public void RefreshLifeBar(int actualVit, int totalVit)
	{
		int per = actualVit / totalVit;
		actualVitBar.Size = new Vector2(per * vitBarInitialSizeX, actualVitBar.Size.Y);
	}
	public void RefreshAbilityContainer(bool hasAbility)
	{
		if (hasAbility)
			// set abiltity texture
			GD.Print("Not implemented yet");

		else
			// set texture null
			abilityTexture.Texture = null;
	}
	public void SetPlayerName(String name)
	{
		textLabel.Text = name;
	}
}
