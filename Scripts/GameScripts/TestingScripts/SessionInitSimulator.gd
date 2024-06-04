extends Node


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_continue_button_pressed():
	CurrentSessionInfo.host_ip = (get_node("Host IP") as TextEdit).text
	CurrentSessionInfo.host_port = (get_node("Host Port") as TextEdit).text.to_int()
	
	PlayerMenu.private_ip = (get_node("IP") as TextEdit).text
	PlayerMenu.private_port = (get_node("Port") as TextEdit).text
	
	if (get_node("Teams") as TextEdit).text == "a":
		CurrentSessionInfo.teams = true
	else:
		CurrentSessionInfo.teams = false
		
	
	if (get_node("IsServer") as TextEdit).text == "a":
		CurrentSessionInfo.is_server = true
	else:
		CurrentSessionInfo.is_server = false
	
	
	CurrentSessionInfo.set_players((get_node("AmountPlayers") as TextEdit).text.to_int())
	
	get_tree().change_scene_to_file("res://Scenes/GameScenes/PlayGround.tscn")
	
