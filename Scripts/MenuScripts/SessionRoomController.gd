extends Node

var game_started = false
var players_list

# Called when the node enters the scene tree for the first time.
func _ready():
	# read info from CurrentSession singleton and load in menu
	
	# session id
	var session_id
	if ConfigurationScript.mobile:
		session_id = get_node("VBoxMenu/HBoxContainer/SessionId")
	else:
		session_id = get_node("VBoxMenu/HBoxContainer/SessionIdContainer/SessionId")
	
	session_id.text = CurrentSessionInfo.s_id
	
	# set elements in the list of players
	players_list = get_node("VBoxMenu/PlayersVBox/ItemList")
	
	# set text in each item
	for i in range(0, CurrentSessionInfo.players):
		players_list.add_item("---", null, false)
	
	# load information from users list
	for i in range(0, CurrentSessionInfo.players_in_room):
		players_list.set_item_text(i, CurrentSessionInfo.players_list[i])

	# be checking if new players connected
	ask_for_players()


func _process(_delta):
	# start game when all players connected
	if CurrentSessionInfo.players == len(CurrentSessionInfo.players_list) and !game_started:
		# ensure that session is really full (it's possible that some players left and 
		# message lost)
		await ask_for_players_one_time()
		
		# check again if session is full
		if CurrentSessionInfo.players == len(CurrentSessionInfo.players_list) and !game_started:
			CurrentSessionInfo.waiting = false #not waiting more players
			#await get_tree().create_timer(3).timeout # wait until all messages have reached
			game_started = true
			# game already starting
			
			start_game()

func start_game():
	await get_tree().create_timer(2).timeout
	# start hole_punching
	await ServerConnection.start_game()# is_server == true
	
	#change button and label visibility
	var starting_game_label = get_node("VBoxMenu/StartingGameLabel")
	starting_game_label.visible = true
	
	var leave_game_button = get_node("VBoxMenu/ManagementButtonHBox/LeaveGame")
	leave_game_button.visible = false
		

# reload players usernames
func reload_players_list():
	# load information from users list
	for i in range(0, CurrentSessionInfo.players_in_room):
		players_list.set_item_text(i, CurrentSessionInfo.players_list[i])
	for i in range(CurrentSessionInfo.players_in_room, CurrentSessionInfo.players):
		players_list.set_item_text(i, "---")


func ask_for_players():
	while(!game_started):
		# get player list
		var err = await ServerConnection.get_session_users()
		
		# error ocurred, return to initial menu
		if err != "ok":
			# clear local session info and return to initial scene
			CurrentSessionInfo.clear_session()
			
			if ConfigurationScript.mobile:
				get_tree().change_scene_to_file("res://Scenes/MenuScenes/Mobile/InitialMenuMobile.tscn")
		
			else:
				get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/InitialMenu.tscn")
		
		# reload player lsit
		reload_players_list()	
		
		# wait before checking again
		if get_tree() != null:
			await get_tree().create_timer(3).timeout

func ask_for_players_one_time():
	var err = await ServerConnection.get_session_users()
	print(err)
		
	# reload player lsit
	reload_players_list()	
	
func leave_session():
	# if is server session will be deleted, so show message to advertise
	if CurrentSessionInfo.players != len(CurrentSessionInfo.players_list) and !game_started:
			var err
			# if is session server remove session
			if CurrentSessionInfo.is_server:
				err = await ServerConnection.remove_session()
			# if is session normal user leave session
			else:
				err = await ServerConnection.leave_session()
			
			# all went well
			if err == "ok":
				CurrentSessionInfo.clear_session()
				
				if ConfigurationScript.mobile:
					get_tree().change_scene_to_file("res://Scenes/MenuScenes/Mobile/InitialMenuMobile.tscn")
			
				else:
					get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/InitialMenu.tscn")
			
			# handle error
			else:
				print("You can't leave now, game is starting")


func _on_leave_game_pressed():
	leave_session()
