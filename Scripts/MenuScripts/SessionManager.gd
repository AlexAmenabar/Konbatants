extends Node

var teams = false
var players = 2
var color_int = 0.15
var private = true

var player

var list_teams_yes
var list_teams_no

# Called when the node enters the scene tree for the first time.
func _ready():
	# initialize current session information
	CurrentSessionInfo.teams = false
	CurrentSessionInfo.private = true
	CurrentSessionInfo.players = 2
	
	# other initialization
	var yes_button_sprite = get_node("GeneralVBoxContainer/VBoxContainer/TeamsHBox/ButtonsHBoxContainer/YesButton/Sprite2D")	
	yes_button_sprite.modulate = Color(yes_button_sprite.modulate.r - color_int, yes_button_sprite.modulate.g - color_int, yes_button_sprite.modulate.b - color_int) 

	var public_button_sprite = get_node("GeneralVBoxContainer/VBoxContainer/GamePrivateHBox/ButtonsHBoxContainer/PublicButton/Sprite2D")
	public_button_sprite.modulate = Color(public_button_sprite.modulate.r - color_int, public_button_sprite.modulate.g - color_int, public_button_sprite.modulate.b - color_int)
	
	
	list_teams_yes = [4, 6, 8]
	list_teams_no = [2, 3, 4, 5, 6]


	var player_list = get_node("GeneralVBoxContainer/VBoxContainer/PlayersHBox/OptionButton")
	player_list.clear()
	
	for n in list_teams_no:
		player_list.add_item(str(n))				
	

## HELPER FUNCTIONS
# change button colors depending on what is selected to give feedback to the user
func change_button_colors(button_selected, button_not_selected):
	button_selected.modulate = Color(button_selected.modulate.r + color_int, button_selected.modulate.g + color_int, button_selected.modulate.b + color_int) 
	button_not_selected.modulate = Color(button_not_selected.modulate.r - color_int, button_not_selected.modulate.g - color_int, button_not_selected.modulate.b - color_int) 

# manage players list options depending if there are teams or not
func manage_teams_list(teams):	
	#refresh players list values 
	var player_list = get_node("GeneralVBoxContainer/VBoxContainer/PlayersHBox/OptionButton")
	player_list.clear()
	
	if teams:
		for n in list_teams_yes:
			player_list.add_item(str(n))
	else:
		for n in list_teams_no:
			player_list.add_item(str(n))				
			

## BUTTON SIGNALS ##
func _on_return_button_pressed():
	if ConfigurationScript.mobile:
		get_tree().change_scene_to_file("res://Scenes/MenuScenes/Mobile/InitialMenuMobile.tscn")
	else:
		get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/InitialMenu.tscn")

# create session button pressed
func _on_create_pressed():
	# call server to create a session	
	var err = await ServerConnection.create_session(str(CurrentSessionInfo.teams), str(CurrentSessionInfo.players), str(CurrentSessionInfo.private))
	
	if err == "ok":
		# load waiting room
		if ConfigurationScript.mobile:
			get_tree().change_scene_to_file("res://Scenes/MenuScenes/Mobile/SessionRoomMobileMenu.tscn")
		else:
			get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/SessionRoom.tscn")
	# there is an error
	else:
		var error_label = get_node("GeneralVBoxContainer/HBoxContainer2/ErrorLabel")
		error_label.text = err
		
		
# find session when button pressed
func _on_find_pressed():
	var err = await ServerConnection.find_session(str(CurrentSessionInfo.teams), str(CurrentSessionInfo.players))
	
	if err != "ok":
		var error_label = get_node("GeneralVBoxContainer/HBoxContainer2/ErrorLabel")
		error_label.text = err
		return

	# else if "ok
	err = await ServerConnection.get_session_users()
	if err == "ok":
		# load waiting room
		if ConfigurationScript.mobile:
			get_tree().change_scene_to_file("res://Scenes/MenuScenes/Mobile/SessionRoomMobileMenu.tscn")
		else:
			get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/SessionRoom.tscn")
	
	else:
		var error_label = get_node("GeneralVBoxContainer/HBoxContainer2/ErrorLabel")
		error_label.text = err


func _on_option_button_item_selected(index):
	#get value on the list and set it to players
	var player_list = get_node("GeneralVBoxContainer/VBoxContainer/PlayersHBox/OptionButton")
	var item = player_list.get_item_text(player_list.get_selected_id()) #id is used to get item
	CurrentSessionInfo.players = int(item) #item is a str, so parse to int

# YES AND NO BUTTONS MANAGE
func _on_yes_button_pressed():
	if !CurrentSessionInfo.teams:
		CurrentSessionInfo.teams = true
		var selected_button = get_node("GeneralVBoxContainer/VBoxContainer/TeamsHBox/ButtonsHBoxContainer/YesButton/Sprite2D")
		var not_selected_button = get_node("GeneralVBoxContainer/VBoxContainer/TeamsHBox/ButtonsHBoxContainer/NoButton/Sprite2D")
		change_button_colors(selected_button, not_selected_button)
		manage_teams_list(CurrentSessionInfo.teams)

func _on_no_button_pressed():
	if CurrentSessionInfo.teams:
		CurrentSessionInfo.teams = false
		var selected_button = get_node("GeneralVBoxContainer/VBoxContainer/TeamsHBox/ButtonsHBoxContainer/NoButton/Sprite2D")
		var not_selected_button = get_node("GeneralVBoxContainer/VBoxContainer/TeamsHBox/ButtonsHBoxContainer/YesButton/Sprite2D")
		change_button_colors(selected_button, not_selected_button)
		manage_teams_list(CurrentSessionInfo.teams)



# PRIVATE AND PUBLIC BUTTONS MANAGE
func _on_public_button_pressed():
	if CurrentSessionInfo.private:
		CurrentSessionInfo.private = false
		var selected_button = get_node("GeneralVBoxContainer/VBoxContainer/GamePrivateHBox/ButtonsHBoxContainer/PublicButton")
		var not_selected_button = get_node("GeneralVBoxContainer/VBoxContainer/GamePrivateHBox/ButtonsHBoxContainer/PrivateButton")
		change_button_colors(selected_button, not_selected_button)


func _on_private_button_pressed():
	if !CurrentSessionInfo.private:
		CurrentSessionInfo.private = true
		var selected_button = get_node("GeneralVBoxContainer/VBoxContainer/GamePrivateHBox/ButtonsHBoxContainer/PrivateButton")
		var not_selected_button = get_node("GeneralVBoxContainer/VBoxContainer/GamePrivateHBox/ButtonsHBoxContainer/PublicButton")
		change_button_colors(selected_button, not_selected_button)
