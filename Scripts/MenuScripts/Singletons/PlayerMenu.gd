extends Node

var usr_name
var id
var current_session_id

var private_ip
var private_port

var peer_ip
var peer_port

# Called when the node enters the scene tree for the first time.
func _ready():
	usr_name = ""
	current_session_id = ""

func set_username(p_usr_name):
	usr_name = p_usr_name

func set_id(p_id):
	id = p_id

func set_current_session(s_id):
	current_session_id = s_id

func clear():
	usr_name = ""
	id = -1
	current_session_id = "null"
