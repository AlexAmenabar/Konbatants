extends Node

var hole_puncher
# Called when the node enters the scene tree for the first time.
func _ready():
	pass
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	pass
	
func server_start():
	var player_id = "000000"
	#hole_puncher = get_node("../hole_puncher")

	var hole_puncher = preload('res://addons/Holepunch/holepunch_node.gd').new()
	
	hole_puncher.rendevouz_address = "192.168.1.45"
	hole_puncher.rendevouz_port = 4444

	add_child(hole_puncher)
	print(hole_puncher.name)
	
	hole_puncher.name = "hole_puncherServer"
	
	hole_puncher.start_traversal("0", true, player_id)


	print("In traversal...")

	var result = await hole_puncher.hole_punched
	
	var my_port = result[0]
	print("Server myport = " + str(my_port))
	

func _on_create_session_pressed():
	server_start()
