extends Button

var selected
func _on_mouse_entered():
	#var texture = get_node("TextureRect")
	#print("Heyheyhey")
	#sprite.modulate = Color(sprite.modulate.r + 0.1, sprite.modulate.g + 0.1, sprite.modulate.b + 0.1) 
	pass

func _on_mouse_exited():
	pass
	#var sprite = get_node("Sprite2D")



func _on_pressed():
	var sound = get_node("audio_pressed")
	sound.play()


func _on_focus_entered():
	var sound = get_node("audio_touch")
	sound.play()
