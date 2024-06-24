extends Node

func _ready():
	ServerConnection.init_sockets()


# exit game
func _on_exit_pressed():
	ServerConnection.exit_game()

# load create session menu
func _on_create_session_pressed():
	CurrentSessionInfo.clear_session()
	get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/CreateSessionMenu.tscn")

# load find session menu
func _on_find_sessin_button_pressed():
	CurrentSessionInfo.clear_session()
	get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/FindSessionMenu.tscn")

# load settings menu
func _on_settings_pressed():
	print("Settings menu not implemented")


# FIND SESSION BY CODE FUNCTIONS

# load session by code
func _on_find_session_by_code_button_pressed():
	CurrentSessionInfo.clear_session()
	get_node("SessionCodeInput").visible = true

# if clicked in the screen where there is not session code input, make it invisible and restart 
func _on_gui_input(event):
	if event is InputEventMouseButton:
		get_node("SessionCodeInput").visible = false
		get_node("SessionCodeInput").text = ""

func _on_session_code_input_text_submitted(new_text):
	var err = await ServerConnection.find_session_by_code(new_text)
	
	if err != "ok":
		var error_label = get_node("VBoxMenu/HBoxContainer/ErrorLabel")
		error_label.text = err
		return	

	# else ok
	CurrentSessionInfo.s_id = new_text
	
	err = await ServerConnection.get_session_users()
	
	if err == "ok":
		# load waiting room
		get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/SessionRoom.tscn")
	
	else:
		var error_label = get_node("VBoxMenu/HBoxContainer/ErrorLabel")
		error_label.text = err




	
