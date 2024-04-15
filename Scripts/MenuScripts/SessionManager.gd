extends Node


var teams = false
var players = 2
var color_int = 0.25
var private = false

var player

var list_teams_yes
var list_teams_no
# Called when the node enters the scene tree for the first time.
func _ready():
	var no_button_sprite = get_node("GeneralVBoxContainer/VBoxContainer/TeamsHBox/ButtonsHBoxContainer/NoButton/Sprite2D")	
	no_button_sprite.modulate = Color(no_button_sprite.modulate.r + color_int, no_button_sprite.modulate.g + color_int, no_button_sprite.modulate.b + color_int) 

	list_teams_yes = [4, 6, 8]
	list_teams_no = [2, 3, 4, 5, 6]


	var player_list = get_node("GeneralVBoxContainer/VBoxContainer/PlayersHBox/OptionButton")
	player_list.clear()
	
	for n in list_teams_no:
		player_list.add_item(str(n))				
	

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_return_button_pressed():
	get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/InitialMenu.tscn")


func _on_create_pressed():
	#call server to create a session
	var teams_str
	if teams:
		teams_str = "yes"
	else:
		teams_str = "no"
	
	var private_str
	if private:
		private_str = "yes"
	else:
		private_str = "no"
		
	var s_id = await ServerConnection.create_session(teams_str, str(players), private_str)
	
	#register values in current info singleton
	CurrentSessionInfo.set_values(private, players, teams, s_id)
	
	#load waiting room
	get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/SessionRoom.tscn")
	
func change_button_colors(button):
	var yes_button_sprite = get_node("GeneralVBoxContainer/VBoxContainer/TeamsHBox/ButtonsHBoxContainer/YesButton/Sprite2D")
	var no_button_sprite = get_node("GeneralVBoxContainer/VBoxContainer/TeamsHBox/ButtonsHBoxContainer/NoButton/Sprite2D")
	
	if button:
		no_button_sprite.modulate = Color(no_button_sprite.modulate.r - color_int, no_button_sprite.modulate.g - color_int, no_button_sprite.modulate.b - color_int) 
		yes_button_sprite.modulate = Color(yes_button_sprite.modulate.r + color_int, yes_button_sprite.modulate.g + color_int, yes_button_sprite.modulate.b + color_int) 

	else:
		no_button_sprite.modulate = Color(no_button_sprite.modulate.r + color_int, no_button_sprite.modulate.g + color_int, no_button_sprite.modulate.b + color_int) 
		yes_button_sprite.modulate = Color(yes_button_sprite.modulate.r - color_int, yes_button_sprite.modulate.g - color_int, yes_button_sprite.modulate.b - color_int) 
		
	#refresh players list values 
	var player_list = get_node("GeneralVBoxContainer/VBoxContainer/PlayersHBox/OptionButton")
	player_list.clear()
	
	if teams:
		for n in list_teams_yes:
			player_list.add_item(str(n))

	else:
		for n in list_teams_no:
			player_list.add_item(str(n))				

func _on_yes_button_pressed():
	if !teams:
		teams = true
		change_button_colors(teams)
	
	
func _on_no_button_pressed():
	if teams:
		teams = false
		change_button_colors(teams)
	
func _on_option_button_item_selected(index):
	#get value on the list and set it to players
	var player_list = get_node("GeneralVBoxContainer/VBoxContainer/PlayersHBox/OptionButton")
	var item = player_list.get_item_text(player_list.get_selected_id()) #id is used to get item

	players = int(item) #item is a str, so parse to int


func _on_find_pressed():
	var str_teams
	if teams:
		str_teams = "with teams"
	else:
		str_teams = "withouth teams"
	print("Finding game session with " + str(players) + " players and " + str_teams)



func _on_public_button_pressed():
	private = false


func _on_private_button_pressed():
	private = true


## FIND GAME BY CODE
func _on_find_by_code_pressed():
	var session_code_node = get_node("SessionCode")
	session_code_node.visible = true


func _on_session_code_text_submitted(new_text):
	ServerConnection.find_session_by_code(new_text)
	
	#TODO, CHECK IF THE SESSION EXISTS
	
	CurrentSessionInfo.s_id = new_text
	get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/SessionRoom.tscn")
