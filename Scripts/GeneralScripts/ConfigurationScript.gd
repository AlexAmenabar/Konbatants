extends Node

# this is used to determine what code have to be run
var device
var mobile

# Called when the node enters the scene tree for the first time.
func _ready():
	device = OS.get_name()
	if device == "Android":
		mobile = true
	else:
		mobile = false
