extends Node

var server_ip
var server_port
var hole_puncher

#variable to communicate with server
var server_udp = PacketPeerUDP.new()
#TCP server
var server_tcp = StreamPeerTCP.new()

const REGISTER_PLAYER = "rp:"
const CREATE_SESSION = "cs:"
const FIND_SESSION = "fs:"
const FIND_SESSION_BY_CODE = "fc:"
const ENTER_SESSION = "es:"

const OK = "ok:"
const ER1 = "e1:"

const NP = "np:" # when a new player enters to the session, server sends this message

# Called when the node enters the scene tree for the first time.
func _ready():
	server_ip = "192.168.1.45"
	server_port = 4444
	server_udp.set_dest_address(server_ip, server_port)
	#server_tcp.connect_to_host(server_ip, server_port)

func _process(delta):
	#check if there is any package to process
	if server_udp.get_available_packet_count() > 0 and CurrentSessionInfo.waiting: #use this only if player is waiting on session room
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		if packet_string.begins_with(NP): #new player to register
			var name = packet_string.split(":")[1] #get id
			
			print("player joined session\n")
			#add player to current session list
			CurrentSessionInfo.players_list.append(name)
			
	
	
	
func register_player():
	var buffer = PackedByteArray()
	buffer.append_array((REGISTER_PLAYER+PlayerMenu.usr_name).to_utf8_buffer())
	server_udp.put_packet(buffer)
	
	#wait to the response
	var timer = 0
	while server_udp.get_available_packet_count() == 0 or timer<2:
		await get_tree().create_timer(0.1).timeout
		timer+=0.1
	
	if server_udp.get_available_packet_count() > 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		if packet_string.begins_with(OK):
			var id = packet_string.split(":")[1] #get id
			PlayerMenu.id = id #set player id
			return 0 #ended correctly
		return 1
	else:
		return 1 #exception
		
func create_session(teams, players, private):
	var buffer = PackedByteArray()
	
	#create message to send
	var message = CREATE_SESSION + PlayerMenu.id + ":" + teams + ":" + players + ":" + private 
	
	#send message
	buffer.append_array((message).to_utf8_buffer())
	server_udp.put_packet(buffer)
	
	#wait to the response
	var timer = 0
	while server_udp.get_available_packet_count() == 0 or timer<4:
		await get_tree().create_timer(0.1).timeout
		timer+=0.1
	
	if server_udp.get_available_packet_count() > 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		if packet_string.begins_with(OK):
			var session_id = packet_string.split(":")[1] #get id
			
			CurrentSessionInfo.is_server = true # who created the session is the server
			CurrentSessionInfo.waiting = true # wait to new players
			
			print(session_id)
			return session_id #ended correctly
		return 1
	else:
		return 1 #exception

	

func find_session():
	pass

func find_session_by_code(code):
	var buffer = PackedByteArray()
	
	#create message to send
	var message = FIND_SESSION_BY_CODE + PlayerMenu.id + ":" + code
	
	#send message
	buffer.append_array((message).to_utf8_buffer())
	server_udp.put_packet(buffer)
	
	#wait to the response
	var timer = 0
	while server_udp.get_available_packet_count() == 0 or timer<4:
		await get_tree().create_timer(0.1).timeout
		timer+=0.1
		
	
	if server_udp.get_available_packet_count() > 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		if packet_string.begins_with(OK):
			print("entered here")
			#load session information
			CurrentSessionInfo.teams = packet_string.split(":")[1]
			CurrentSessionInfo.private = packet_string.split(":")[2]
			CurrentSessionInfo.players = int(packet_string.split(":")[3])
			
			var players_in_room = packet_string.split(":")[4] #get id
			#ask players in room and show it graphically
			#TODO
			
			# load session information
			
			
			CurrentSessionInfo.players_in_room = players_in_room
			for player in players_in_room:
				CurrentSessionInfo.players_list.append("player")
				
		
#hole punching
func start_game():
	var hole_puncher = preload('res://addons/Holepunch/holepunch_node.gd').new()
	hole_puncher.rendevouz_address = server_ip
	hole_puncher.rendevouz_port = server_port

	CurrentSessionInfo.waiting = false
	
	print("is_server")
	print(CurrentSessionInfo.is_server)
	add_child(hole_puncher)
	
	hole_puncher.start_traversal(CurrentSessionInfo.s_id, CurrentSessionInfo.is_server, PlayerMenu.id)


	print("Starting traversal...")

	var result = await hole_puncher.hole_punched
	
	print("result reached")
	
	if CurrentSessionInfo.is_server:
		var my_port = result[0]
		print("Server myport = " + str(my_port))
		CurrentSessionInfo.own_port = my_port

		
	
	else:
		var host_ip = result[2]
		var host_port = result[1]
		var own_port = result[0]
		print("Client reached")
		
		CurrentSessionInfo.host_ip = host_ip
		CurrentSessionInfo.host_port = host_port
		CurrentSessionInfo.own_port = own_port
