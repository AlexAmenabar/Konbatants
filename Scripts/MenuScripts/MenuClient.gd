extends Node

var hole_puncher
# Called when the node enters the scene tree for the first time.
func _ready():
	pass
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	pass

func client_start():
	pass


func _on_find_session_pressed():
	var hole_puncher = preload('res://addons/Holepunch/holepunch_node.gd').new()
	
	hole_puncher.rendevouz_address = "192.168.1.45"
	hole_puncher.rendevouz_port = 4444

	add_child(hole_puncher)
	print(hole_puncher.name)
	
	hole_puncher.name = "hole_puncher"


	var player_id = "000001"
	#hole_puncher = get_node("../hole_puncher")
	hole_puncher.start_traversal("0", false, player_id)

	print("Client connecting...")

	var result = await hole_puncher.hole_punched
	
	var host_ip = result[2]
	var host_port = result[1]
	var own_port = result[0]


