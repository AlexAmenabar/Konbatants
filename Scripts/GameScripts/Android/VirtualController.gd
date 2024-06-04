extends Node


var player_controller

# joysticks
var movement_joystick
var ability_joystick

# movement joystick

# Called when the node enters the scene tree for the first time.
func _ready():
	movement_joystick = get_node("VirtualMovementJoystick")
	ability_joystick = get_node("VirtualAbilityJoystick")
	var tip = ability_joystick.get_node("Base/Tip")
	tip.texture = load("res://addons/virtual_joystick/textures/joystick_tip.png")

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if player_controller != null:
		player_controller.jump = 0
		DetectMovement()


func DetectMovement():
	player_controller.hdir = movement_joystick.hdir
	player_controller.vdir = movement_joystick.vdir
	
func DetectAbilityDirection():
	pass


func _on_jump_gui_input(event):
	player_controller.jump = 1
	pass # Replace with function body.


func _on_attack_gui_input(event):
	pass # Replace with function body.
