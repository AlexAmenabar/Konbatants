extends Node

var language_names = ["English", "Spanish", "Basque"]
var language_codes = ["en", "es", "eu"]
# Called when the node enters the scene tree for the first time.
func _ready():
	var languages = get_node("./GeneralVBoxContainer/VBoxContainer/LanguagesHBox/OptionButton")
	languages.clear()
	for language in language_names:
		languages.add_item(language)
		
func _on_option_button_item_selected(index):
	TranslationServer.set_locale(language_codes[index])

func _on_return_button_pressed():
	if ConfigurationScript.mobile:
		get_tree().change_scene_to_file("res://Scenes/MenuScenes/Mobile/InitialMenuMobile.tscn")

	else:
		get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/InitialMenu.tscn")

