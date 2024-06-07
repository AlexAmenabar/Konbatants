extends Node

# session info
var private=true
var players = 0 # total players
var players_in_room = 0 #players entered session
var teams = false
var s_id = null

# player names
var players_list = []

# user is server or not
var is_server = false

# client own port
var own_port
var waiting=false #this is used to know when in ServerConnection must be used proccess

# clients store here session host info (stored after 
var host_ip
var host_port

# server store here all clients ip and ports
var clients_ips = []
var clients_ports = []


func set_values(p_private, p_players, p_teams, p_id):
	private = p_private
	players = p_players
	teams = p_teams
	s_id = p_id

func clear_session():
	s_id = null
	players_in_room = 0
	players = 0
	players_list.clear()
	
	
func remove_player_by_name(p_name):
	for i in range(0, players_in_room):
		if players_list[i] == p_name:
			players_list.remove(i)
			
	players_in_room -= 1

func clear():
	s_id = "null"
	players_list.clear()
	
func get_players():
	return players
	
func set_players(p):
	players = p
	print(p)
