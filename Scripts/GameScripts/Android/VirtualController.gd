extends Node

# Virtual controller to use in mobiles

var player_controller

# joysticks and buttons
var movement_joystick # this is a plugin to simulate a virtual joystick, more information in virtual_joystick_scene.gd
var ability_joystick
var ability_used

# movement joystick

# Called when the node enters the scene tree for the first time.
func _ready():
	movement_joystick = get_node("VirtualMovementJoystick")
	ability_joystick = get_node("VirtualAbilityJoystick")
	var tip = ability_joystick.get_node("Base/Tip")
	tip.texture = load("res://addons/virtual_joystick/textures/joystick_tip.png")

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	if player_controller != null:
		player_controller.jump = 0
		DetectMovement()

# change player control variables
func DetectMovement():
	player_controller.hdir = movement_joystick.hdir
	player_controller.vdir = movement_joystick.vdir
	
func DetectAbilityDirection():
	pass


func _on_jump_gui_input(_event):
	player_controller.jump = 1

func _on_attack_gui_input(_event):
	player_controller.attackVar = true


func _on_ability_gui_input(_event):
	player_controller.abilityUsed = true
