extends Node

var private=true
var players = 0 # total players
var players_in_room = 0 #players entered session
var teams = false
var s_id = null

var players_list = []

var is_server = false

var host_ip
var host_port
var own_port

var waiting=false #this is used to know when in ServerConnection must be used proccess

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
	
	
func remove_player_by_name(name):
	for i in range(0, players_in_room):
		if players_list[i] == name:
			players_list.remove(i)
			
	players_in_room -= 1

func clear():
	s_id = "null"
	players_list.clear()
