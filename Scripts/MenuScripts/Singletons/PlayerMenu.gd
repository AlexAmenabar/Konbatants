extends Node

var usr_name
var id

# private port and ip
var private_ip
var private_port

#var peer_ip
# public port
var peer_port

# Called when the node enters the scene tree for the first time.
func _ready():
	usr_name = ""


func set_username(p_usr_name):
	usr_name = p_usr_name

func set_id(p_id):
	id = p_id

func clear():
	usr_name = ""
	id = -1
