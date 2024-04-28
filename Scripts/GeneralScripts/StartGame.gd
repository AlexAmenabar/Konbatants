extends Node

# Called when the node enters the scene tree for the first time.
func _ready():
	print(DisplayServer.window_get_size())
	print(DisplayServer.screen_get_size())
	
	var debug_label = get_node("DebugLabel")
	debug_label.text = "Window size: " + str(DisplayServer.window_get_size()) + "\nScreen size: " + str(DisplayServer.screen_get_size())
	
	DisplayServer.window_set_size(Vector2i(DisplayServer.screen_get_size()[1], DisplayServer.screen_get_size()[0]))
	get_tree().set_auto_accept_quit(false)
	
# set username when INTRO pressed
func _on_line_edit_text_submitted(new_text):
	register_player(new_text)

# set username when button pressed
func _on_custom_button_pressed():
	register_player(get_node("./Control/VBoxContainer/UsernameText").text)
	
func register_player(username):
	PlayerMenu.set_username(username)
	var res = await ServerConnection.register_player()
	# all ok
	if res == "ok":
		get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/InitialMenu.tscn")
	# print error
	else: 
		var error_label = get_node("Control/VBoxContainer/ErrorsLabel")
		error_label.text = res


func _on_continue_button_pressed():
	register_player(get_node("./Control/VBoxContainer/UsernameText").text)
