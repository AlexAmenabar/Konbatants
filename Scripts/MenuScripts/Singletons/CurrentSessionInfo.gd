extends Node

var private=false
var players = 0 # total players
var players_in_room = 0 #players entered session
var teams = false
var s_id

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
