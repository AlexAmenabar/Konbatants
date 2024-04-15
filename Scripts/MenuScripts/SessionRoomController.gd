extends Node


var player_count = 0
var game_started = false
# Called when the node enters the scene tree for the first time.
func _ready():
	
	#read info from CurrentSession singleton and load in menu
	
	#session id
	var session_id = get_node("VBoxMenu/HBoxContainer/SessionIdContainer/SessionId")
	session_id.text = CurrentSessionInfo.s_id
	
	#set elements in the list of players
	var players_list = get_node("VBoxMenu/PlayersVBox/ItemList")
	
	#set text in each item
	for i in range(0, CurrentSessionInfo.players):
		players_list.add_item("---", null, false)
	
	# the first player is this player, the server
	players_list.set_item_text(0, PlayerMenu.usr_name)
	CurrentSessionInfo.players_list.append(PlayerMenu.usr_name)


func _process(_delta):
	# load player name in the list when new player is connected

	print(len(CurrentSessionInfo.players_list))
	# start game when all players connected
	if CurrentSessionInfo.players == len(CurrentSessionInfo.players_list) and !game_started:
		CurrentSessionInfo.waiting = false #not waiting more players
		
		# start hole_punching
		ServerConnection.start_game()# is_server == true
		
		#change button and label visibility
		var starting_game_label = get_node("VBoxMenu/StartingGameLabel")
		starting_game_label.visible = true
		
		var leave_game_button = get_node("VBoxMenu/ManagementButtonHBox/LeaveGame")
		leave_game_button.visible = false
		
		# game already starting
		game_started = true
