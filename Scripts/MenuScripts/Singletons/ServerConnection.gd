extends Node

var server_ip
var server_port
var hole_puncher

#variable to communicate with server
var server_udp = PacketPeerUDP.new()
#TCP server
var server_tcp = StreamPeerTCP.new()

#peer connection
var peer_udp = PacketPeerUDP.new()
var peer_tcp = PacketPeerUDP.new()


const REGISTER_PLAYER = "rp:"
const CREATE_SESSION = "cs:"
const FIND_SESSION = "fs:"
const FIND_SESSION_BY_CODE = "fc:"
const ENTER_SESSION = "es:"
const START_GAME = "sg:"

const SAY_HELLO = "hello:"
const GET_PORT = "gp:"

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

	

func find_session(teams, players):
	var buffer = PackedByteArray()
	
	#create message to send
	var message = FIND_SESSION + PlayerMenu.id + ":" + teams + ":" + players 
	
	#send message
	buffer.append_array((message).to_utf8_buffer())
	server_udp.put_packet(buffer)
	
	#wait to the response
	var timer = 0
	while server_udp.get_available_packet_count() == 0 and timer<4:
		await get_tree().create_timer(0.1).timeout
		timer+=0.1
	
	
	if server_udp.get_available_packet_count() > 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		if packet_string.begins_with(OK):
			print("entered here")
			#load session information
			var s_id = packet_string.split(":")[1]
			CurrentSessionInfo.teams = packet_string.split(":")[2]
			CurrentSessionInfo.private = packet_string.split(":")[3]
			CurrentSessionInfo.players = int(packet_string.split(":")[4])
			
			var players_in_room = int(packet_string.split(":")[4]) #get actual players in room
			print("Players in room: ")
			print(players_in_room)
			print("\n")
			#ask players in room and show it graphically
			#TODO
			
			# load session information
			
			CurrentSessionInfo.players_in_room = players_in_room
			for n in range(0, players_in_room-1):
				print("Adding player to player list")
				print(n)
				CurrentSessionInfo.players_list.append("player" + str(n))
				
				
			# check if session is complete or if player have to wait
			if players_in_room < CurrentSessionInfo.players:
				CurrentSessionInfo.waiting = true
	
			return s_id
	

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
			
			var players_in_room = int(packet_string.split(":")[4]) #get actual players in room
			print("Players in room: ")
			print(players_in_room)
			print("\n")
			#ask players in room and show it graphically
			#TODO
			
			# load session information
			
			CurrentSessionInfo.players_in_room = players_in_room
			for n in range(0, players_in_room-1):
				print("Adding player to player list")
				print(n)
				CurrentSessionInfo.players_list.append("player" + str(n))
				
				
			# check if session is complete or if player have to wait
			if players_in_room < CurrentSessionInfo.players:
				CurrentSessionInfo.waiting = true
			
#hole punching
func start_game():
	#var hole_puncher = preload('res://addons/Holepunch/holepunch_node.gd').new()
	#hole_puncher.rendevouz_address = server_ip
	#hole_puncher.rendevouz_port = server_port

	CurrentSessionInfo.waiting = false
	
	print("is_server")
	print(CurrentSessionInfo.is_server)
	
	hole_punching()
	'''
	add_child(hole_puncher)
	
	hole_puncher.start_traversal(CurrentSessionInfo.s_id, CurrentSessionInfo.is_server, PlayerMenu.id)


	print("Starting traversal...")

	var result = await hole_puncher.hole_punched


	print("Hole punching finalized")
	
	if CurrentSessionInfo.is_server:
		var my_port = result[0]
		print("Server myport = " + str(my_port))
		CurrentSessionInfo.own_port = my_port
	
	else:
		var host_ip = result[2]
		var host_port = result[1]
		var own_port = result[0]
		print("Client info")
		
		CurrentSessionInfo.host_ip = host_ip
		CurrentSessionInfo.host_port = host_port
		CurrentSessionInfo.own_port = own_port
'''
func hole_punching():
	if CurrentSessionInfo.is_server:
		var peers_contacted = 0
		
		var buffer = PackedByteArray()
		#create message to send
		var message = GET_PORT + CurrentSessionInfo.s_id
		#send message
		buffer.append_array((message).to_utf8_buffer())
		server_udp.put_packet(buffer)
	
		#wait to the response
		var timer = 0
		while server_udp.get_available_packet_count() == 0 or timer<20:
			await get_tree().create_timer(0.1).timeout
			timer+=0.1
	
	
		if server_udp.get_available_packet_count() > 0:
			var array_bytes = server_udp.get_packet()
			var packet_string = array_bytes.get_string_from_ascii()
			print("packet string")
			print(packet_string)
			
			if packet_string.begins_with(OK):
				var own_port = int(packet_string.split(":")[1])
				server_udp.close()
		
				peer_udp.bind(own_port, "*")
		
				#wait until all clients have contacted server
				while peers_contacted < CurrentSessionInfo.players-1:
					if peer_udp.get_available_packet_count() > 0:
						print("client message reached")
						array_bytes = peer_udp.get_packet()
						packet_string = array_bytes.get_string_from_ascii()
						
						#send response to client
						var client_ip = peer_udp.get_packet_ip()
						var client_port = peer_udp.get_packet_port()
						
						peer_udp.set_dest_address(client_ip, client_port)
					
						buffer.append_array(("Hey client").to_utf8_buffer())
						peer_udp.put_packet(buffer)
	
						
								
	else: #server will send his 
		var buffer = PackedByteArray()
	
		#create message to send
		var message = START_GAME + CurrentSessionInfo.s_id + ":" + PlayerMenu.id
		#send message
		buffer.append_array((message).to_utf8_buffer())
		server_udp.put_packet(buffer)
	
		#wait to the response
		var timer = 0
		while server_udp.get_available_packet_count() == 0 or timer<20:
			await get_tree().create_timer(0.1).timeout
			timer+=0.1
	
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		print("packet string")
		print(packet_string)
		
		#packet reached well
		if packet_string.begins_with(OK):
			var game_server_ip = packet_string.split(":")[1]
			var game_server_port = packet_string.split(":")[2]
			var own_port = packet_string.split(":")[3]
			server_udp.close()

			#try to communicate with server
			peer_udp.set_dest_address(game_server_ip, int(game_server_port))
			message = SAY_HELLO + ":" + own_port
			buffer.append_array((message).to_utf8_buffer())
			peer_udp.put_packet(buffer)
	
			own_port = int(own_port)

			
				#wait to the response
			timer = 0
			while peer_udp.get_available_packet_count() == 0 or timer<20:
				await get_tree().create_timer(0.1).timeout
				timer+=0.1
	
			if timer > 20:
				print("timeout")

			if peer_udp.get_available_packet_count() > 0:
				print("server confimation received")
				array_bytes = peer_udp.get_packet()
				packet_string = array_bytes.get_string_from_ascii()
				print("packet string")
				print(packet_string)
		
