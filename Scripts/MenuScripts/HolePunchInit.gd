extends Node


# Called when the node enters the scene tree for the first time.
func _ready():
	pass
	'''var hole_puncher = preload('res://addons/Holepunch/holepunch_node.gd').new()
	
	hole_puncher.rendevouz_address = "192.168.1.45"
	hole_puncher.rendevouz_port = 4444

	add_child(hole_puncher)
	print(hole_puncher.name)
	hole_puncher.name = "hole_puncher"
	var children = get_child_count()
	print(children)
	for n in get_children():
		print(n.name)'''
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
		


func _on_exit_pressed():
	get_tree().quit()


func _on_create_session_pressed():
	get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/CreateSessionMenu.tscn")


func _on_settings_pressed():
	pass # Replace with function body.
	#load settings menu
