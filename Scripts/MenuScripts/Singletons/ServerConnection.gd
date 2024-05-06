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
var local_peer_udp = PacketPeerUDP.new()

# cosnt variables
const REGISTER_PLAYER = "rp:"
const CREATE_SESSION = "cs:"
const FIND_SESSION = "fs:"
const FIND_SESSION_BY_CODE = "fc:"
const ENTER_SESSION = "es:"
const START_GAME = "sg:"
const SESSION_USER="su:"
const LEAVE_SESSION = "ls:" # only session clients
const REMOVE_SESSION = "rs:" # only session server
const INVITE_TO_SESSION = "iu:" # only session server
const DROP_PLAYER = "du:" # only session server
const EXIT_GAME = "eg:"

const SAY_HELLO = "hello:"
const GET_PORT = "gp:"
const ACK = "ac"
const ACK_LOST = "al"

const OK = "ok:"
const ER = "er:"
const ER1 = "e1:"

const NEW_PLAYER = "np:" # when a new player enters to the session, server sends this message

const TIMEOUT = 200
var resend_count = 0
const MAX_RESEND_COUNT = 1000

# CONSTANTS for testing
const GET_ALL_USERS = "tu:"
const GET_ALL_SESSION = "ts:"
const GET_USERS_IN_SESSION = "us:"
const CLEAR_SERVER = "cl:"

# Called when the node enters the scene tree for the first time.
func _ready():
	#server_ip = "34.170.146.101" # cloud server
	server_ip = "192.168.1.36" # local server
	server_port = 4444
	server_udp.set_dest_address(server_ip, server_port)
	
	# start listening in ports and initialize info in PlayerMenu 
	var port_number = start_peer_udp(server_udp, 5125)
	server_udp.connect_to_host(server_ip, server_port)
		
	port_number = start_peer_udp(peer_udp, port_number)
	PlayerMenu.peer_port = peer_udp.get_local_port()
	
	port_number = start_peer_udp(local_peer_udp, port_number)
	PlayerMenu.private_port = local_peer_udp.get_local_port()

	# get machine private ip
	PlayerMenu.private_ip = get_private_ip()

func _process(delta):
	# check if there is any package to process when client is waiting (to another users) in session room
	'''if server_udp.get_available_packet_count() > 0 and CurrentSessionInfo.waiting: #use this only if player is waiting on session room
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		if packet_string.begins_with(NEW_PLAYER): #new player to register
			var name = packet_string.split(":")[1] #get id
			
			# add player to current session list
			print("PAM...")

			print("await finished!")
			CurrentSessionInfo.players_list.append(name)'''
	
	
	'''if server_udp.get_available_packet_count() > 0 and CurrentSessionInfo.waiting:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		print(packet_string)
		if packet_string.begins_with(LEAVE_SESSION): # player left session
			var user_left_name = packet_string.split(":")[1] # get username 
			
			# refresh players list
			CurrentSessionInfo.remove_player_by_name(user_left_name)'''
			


# HELPER FUNCTIONS
func get_private_ip():
	for ip in IP.get_local_addresses():
		if ip.begins_with("192.168.1"):
			return ip


func start_peer_udp(socket_udp, port_number):
	var err = 1
	while err != 0:
		err = socket_udp.bind(port_number)
		port_number += 1
	return port_number
	
func wait_message(peer_udp):
	var timer = 0
	while peer_udp.get_available_packet_count() == 0 and timer<TIMEOUT:
		await get_tree().create_timer(0.1).timeout
		timer+=0.1
	
	# timeout
	if timer>TIMEOUT:
		return 1 
	
	# response reached
	if peer_udp.get_available_packet_count() > 0:
		return 0 

# send message and return response
func send_message(message, peer_udp):
	var buffer = PackedByteArray()
	buffer.append_array(message.to_utf8_buffer())
	var res = 1
	
	# loop while message response is not reached
	while res==1 and resend_count < MAX_RESEND_COUNT:
		peer_udp.put_packet(buffer)
		res = await wait_message(peer_udp)
		resend_count += 1
	
	# can't contact server
	if resend_count >= MAX_RESEND_COUNT:
		return 2
	
	return res # 0

# if return -1, timoeut, else return index of peer that get a message
func communicate_with_another_client(message, peers_udp, timeout):
	var finished = false
	
	var message_from_peer_received = false
	var confirmation_from_peer_received = false
	
	var res = -1
	var timer = 0
	while (!message_from_peer_received and !confirmation_from_peer_received) and timer < timeout:
		# send message if peer confimation not received
		var buffer = PackedByteArray()
		if !confirmation_from_peer_received:
			print("Sending message!")
			buffer.append_array(message.to_utf8_buffer())
			
			# send message from all sockets (usually 2)
			for peer_udp in peers_udp:
				peer_udp.put_packet(buffer)
		
		# wait 
		await get_tree().create_timer(0.5).timeout
		timer+=0.5

		# see if client send any message
		for i in len(peers_udp):
			peer_udp = peers_udp[i]
			if peer_udp.get_available_packet_count() > 0:
				print("MESSAGE REACHED TO " + str(i) + " SOCKET!!!!!")
				
				if i<res or res==-1:
					res = i
				var array_bytes = peer_udp.get_packet()
				var packet_string = array_bytes.get_string_from_ascii()
				#print("packet string")
				print(packet_string)
				
				# peer message received
				# confirmation received
				if packet_string.find(ACK) > -1:
					confirmation_from_peer_received = true
					print("ACK RECEIVED")
					
				# hello message received
				if packet_string.begins_with(SAY_HELLO):
					message_from_peer_received = true
					print("HELLO RECEIVED")
					
					# send ack
					await get_tree().create_timer(0.5).timeout
					
					var ack_message = ACK
					buffer.append_array(ack_message.to_utf8_buffer())
					peer_udp.put_packet(buffer)

	if timer > timeout:
		return res
	# while host message not reached
	'''while peer_udp.get_available_packet_count() == 0:
		var buffer = PackedByteArray()
		buffer.append_array(message.to_utf8_buffer())
		peer_udp.put_packet(buffer)
		await get_tree().create_timer(1).timeout
	'''
	return res


## SERVER COMMUNICATION FUNCTIONS
# register player in the server
func register_player():
	var message = REGISTER_PLAYER + PlayerMenu.usr_name + ":" + PlayerMenu.private_ip + ":" + str(PlayerMenu.private_port) + ":" + str(PlayerMenu.peer_port)
	var res = await send_message(message, server_udp)
	
	# can't contact server
	if res == 2:
		print("Can't contact with server")	
	
	# response get by server
	if res == 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		
		# player registered correctly
		if packet_string.begins_with(OK):
			PlayerMenu.id = packet_string.split(":")[1] #get id
			return "ok"
		
		elif packet_string.begins_with(ER):
			var err = packet_string.split(":")[1] #get error message
			return err
			
			
# create session in the server
func create_session(teams, players, private):
	# create message
	var message = CREATE_SESSION + PlayerMenu.id + ":" + teams + ":" + players + ":" + private 
	# send message and receive response
	var res = await send_message(message, server_udp)
		
	# can't contact server		
	if res == 2:
		print("Can't contact with server")	
		
	if res == 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		
		# all ok
		if packet_string.begins_with(OK):
			CurrentSessionInfo.s_id = packet_string.split(":")[1] #get id
			CurrentSessionInfo.is_server = true # who created the session is the server
			CurrentSessionInfo.waiting = true # wait to new players
			return "ok" #ended correctly
		
		# there is an error
		elif packet_string.begins_with(ER):
			var err = packet_string.split(":")[1]
			return err

# find a session that fit the parameters
func find_session(teams, players):	
	#create message to send
	var message = FIND_SESSION + PlayerMenu.id + ":" + teams + ":" + players 
	var res = await send_message(message, server_udp)
	
	# can't contact server		
	if res == 2:
		print("Can't contact with server")	
	
	# server response received
	if res == 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		
		# server send ok message
		if packet_string.begins_with(OK):
			# load session information in singleton
			CurrentSessionInfo.s_id = packet_string.split(":")[1]
			CurrentSessionInfo.teams = packet_string.split(":")[2]
			CurrentSessionInfo.private = packet_string.split(":")[3]
			CurrentSessionInfo.players = int(packet_string.split(":")[4])
			CurrentSessionInfo.players_in_room = int(packet_string.split(":")[5]) #get actual players in room

			var username = ""
			# wait until all session user usernmaes reach
			'''for i in range(0, CurrentSessionInfo.players_in_room-1):
				res = await wait_message()
				
				#timeout, ask that user again
				message = SESSION_USER + ":" + CurrentSessionInfo.s_id + ":" + str(i) 	
				while res == 1:
					res = await send_message(message)
				
				# if res==0
				array_bytes = server_udp.get_packet()
				packet_string = array_bytes.get_string_from_ascii()
				username = packet_string.split(":")[1]
				CurrentSessionInfo.players_list.append(username)'''
				
			# check if session is complete or if player have to wait
			if CurrentSessionInfo.players_in_room < CurrentSessionInfo.players: # session not full
				CurrentSessionInfo.waiting = true
			return "ok"
		
		elif packet_string.begins_with(ER):
			var err = packet_string.split(":")[1]
			return err
	
			
# find a public session using session code
func find_session_by_code(code):
	#create message to send
	var message = FIND_SESSION_BY_CODE + PlayerMenu.id + ":" + code
	var res = await send_message(message, server_udp)
	
	# can't contact server		
	if res == 2:
		print("Can't contact with server")		
	
	# received server response
	if res == 0:	
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		if packet_string.begins_with(OK):
			#load session information
			CurrentSessionInfo.teams = packet_string.split(":")[1]
			CurrentSessionInfo.private = packet_string.split(":")[2]
			CurrentSessionInfo.players = int(packet_string.split(":")[3])
			CurrentSessionInfo.players_in_room = int(packet_string.split(":")[4])
			CurrentSessionInfo.s_id = packet_string.split(":")[5]
			
			# check if session is complete or if player have to wait
			if CurrentSessionInfo.players_in_room < CurrentSessionInfo.players:
				CurrentSessionInfo.waiting = true
			
			return "ok" # all ok
		
		# there is an error
		elif packet_string.begins_with(ER):
			var err = packet_string.split(":")[1]
			return err

# ask session users information and add it to the list
func get_session_users():
	#create message to send
	var message = SESSION_USER + PlayerMenu.id
	var res = await send_message(message, server_udp)
	
	# can't contact server		
	if res == 2:
		print("Can't contact with server")	
	
	# server response received
	if res == 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		
		# server send ok message
		if packet_string.begins_with(OK):
			CurrentSessionInfo.players_in_room = int(packet_string.split(":")[1])
			var usernames = packet_string.split(":") # usernames starts from 2. eleement
			
			CurrentSessionInfo.players_list.clear()
			for i in range(0, CurrentSessionInfo.players_in_room):
				print(CurrentSessionInfo.players_in_room)
				CurrentSessionInfo.players_list.append(usernames[i+2])
			
			return "ok"
			
		elif packet_string.begins_with(ER):
			var err = packet_string.split(":")[1]
			return err
	
func leave_session():
	#create message to send
	var message = LEAVE_SESSION + PlayerMenu.id
	var res = await send_message(message, server_udp)
	
	# can't contact server		
	if res == 2:
		print("Can't contact with server")	
	
	# server response received
	if res == 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		
		# server send ok message
		if packet_string.begins_with(OK):
			CurrentSessionInfo.clear_session()
			return "ok"
			
		elif packet_string.begins_with(ER):
			var err = packet_string.split(":")[1]
			return err
	
	
func remove_session():
	#create message to send
	var message = REMOVE_SESSION + PlayerMenu.id
	var res = await send_message(message, server_udp)
	
	# can't contact server		
	if res == 2:
		print("Can't contact with server")	
	
	# server response received
	if res == 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		
		# server send ok message
		if packet_string.begins_with(OK):
			CurrentSessionInfo.clear_session()
			return "ok"
			
		elif packet_string.begins_with(ER):
			var err = packet_string.split(":")[1]
			return err
	


func invite_player():
	pass


func drop_player():
	pass
	

func _notification(what):
	# exit game notification reached (quit button pressed)
	if what == NOTIFICATION_WM_CLOSE_REQUEST or what == NOTIFICATION_WM_GO_BACK_REQUEST: # second for android
		if CurrentSessionInfo.s_id == null:
			CurrentSessionInfo.s_id = "null"
		var message = EXIT_GAME + CurrentSessionInfo.s_id + ":" + str(PlayerMenu.id) 
		var res = await send_message(message, server_udp)
		get_tree().quit()

		
func exit_game():
	#create message to send
	if CurrentSessionInfo.s_id == null:
			CurrentSessionInfo.s_id = "null"

	var message = EXIT_GAME + str(PlayerMenu.id) 
	print(message)
	var res = await send_message(message, server_udp)
	
	if server_udp.get_available_packet_count() > 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		
		# for testing
		return packet_string
	
	#get_tree().quit()

# start a game
func start_game():
	CurrentSessionInfo.waiting = false
	print("is_server: " + str(CurrentSessionInfo.is_server))
	
	hole_punching()

# hole punch ip and ports to communicate clients P2P
func hole_punching():
	var buffer = PackedByteArray()
	
	# if there is a packet that it isn't needed
	if server_udp.get_available_packet_count() > 0:
		server_udp.get_packet()
	
	# if session is server
	if CurrentSessionInfo.is_server:
		var peers_contacted = 0
		
		# create message to send
		var message = GET_PORT + CurrentSessionInfo.s_id
		# send message
		var res = await send_message(message, server_udp)
	
		# can't contact server		
		if res == 2:
			print("Can't contact with server")	
	
		if res==0:
			var array_bytes = server_udp.get_packet()
			var packet_string = array_bytes.get_string_from_ascii()
			print("packet string")
			print(packet_string)
			
			# server send ok message
			if packet_string.begins_with(OK):
				
				await get_tree().create_timer(3).timeout
				# close server socket
				var ip_and_ports = packet_string.split(":")
				server_udp.close()
		
				# start listening in own_port port
				var own_ip = ip_and_ports[1].split("-")[0]
				var own_port = int(ip_and_ports[1].split("-")[1])
				var own_private_ip = ip_and_ports[1].split("-")[2]
				var own_private_port = ip_and_ports[1].split("-")[3]

				# contact with all clients
				for i in range(2, len(ip_and_ports)):
					var ip = ip_and_ports[i].split("-")[0]
					var port = int(ip_and_ports[i].split("-")[1])
					var private_ip = ip_and_ports[i].split("-")[2]
					var private_port = int(ip_and_ports[i].split("-")[3])
					
					message = SAY_HELLO
					
					# connect each socket to peer public or private socket
					peer_udp.connect_to_host(ip, int(port))
					local_peer_udp.connect_to_host(private_ip, int(private_port))
				
					var peers_udp = []
					peers_udp.append(peer_udp)
					peers_udp.append(local_peer_udp)
					
					#peer_udp.connect_to_host(private_ip, private_port)
					#print("sending message to client " + str(i) + " whoes ip and port are: (" + private_ip + ":" + str(private_port) + ")")
					res = await communicate_with_another_client(message, peers_udp, 1000)
					#print("res = " + str(res))
					if res >= 0:
						peers_contacted+=1
						#print("Response reached from " + str(i) + " client")
						#get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/InitialMenu.tscn")
					else:
						print("TIMEOUT, CANT CONTACT WITH " + str(i) + " ONE CLIENT")
				
				print("Peers contacted = " + str(peers_contacted))
				if peers_contacted == CurrentSessionInfo.players-1:
					print("GAME IS READY TO START")
				
				else:
					print("CAN'T START GAME, REMOVING SESSION")
				
			# server send an error message
			#TODO
			
	# if session is client
	else:  
		#create message to send
		var message = START_GAME + CurrentSessionInfo.s_id + ":" + PlayerMenu.id
		#send message
		var res = await send_message(message, server_udp)
	
		# can't contact server		
		if res == 2:
			print("Can't contact with server")	
	
		if res == 0:
			var array_bytes = server_udp.get_packet()
			var packet_string = array_bytes.get_string_from_ascii()
			print("packet string")
			print(packet_string)
			
			# server send ok message
			if packet_string.begins_with(OK):
				
				await get_tree().create_timer(3).timeout
				# store information and close udp server connection
				var game_server_ip = packet_string.split(":")[1]
				var game_server_port = packet_string.split(":")[2]
				var game_server_private_ip = packet_string.split(":")[3]
				var game_server_private_port = packet_string.split(":")[4]

				server_udp.close()
				
				# connect each socket to peer public or private socket
				peer_udp.connect_to_host(game_server_ip, int(game_server_port))
				local_peer_udp.connect_to_host(game_server_private_ip, int(game_server_private_port))
				
				await get_tree().create_timer(2).timeout # time to server server listening
				
				# start listening in this port --> PORT IS ALREADY BINDING
				#peer_udp.bind(int(own_port), "*")
				# try to communicate with server
				#peer_udp.set_dest_address(server_ip, server_port)
				message = SAY_HELLO
				#send_message(message, peer_udp)
				
				# try first with private info (not necesary to hole punch
				#peer_udp.connect_to_host(game_server_private_ip, int(game_server_private_port))
				var peers_udp = []
				peers_udp.append(peer_udp)
				peers_udp.append(local_peer_udp)
				
				res = await communicate_with_another_client(message, peers_udp, 1000)
				print("res = " + str(res))
				# some socket get a result
				if res >= 0:
					print("message reached from server")
					get_tree().change_scene_to_file("res://Scenes/MenuScenes/PC/InitialMenu.tscn")
				
				else:
					print("TIMEOUT")
				
				
# FUNCTIONS FOR TESTING
func get_active_users():
	# create message
	var message = GET_ALL_USERS 
	# send message and receive response
	var res = await send_message(message, server_udp)
		
	# can't contact server		
	if res == 2:
		print("Can't contact with server")	
		
	if res == 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		
		# all ok
		if packet_string.begins_with(OK):
			return int(packet_string.split(":")[1])
	
func get_active_session():
	# create message
	var message = GET_ALL_SESSION 
	# send message and receive response
	var res = await send_message(message, server_udp)
		
	# can't contact server		
	if res == 2:
		print("Can't contact with server")	
		
	if res == 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		
		# all ok
		if packet_string.begins_with(OK):
			return int(packet_string.split(":")[1])

func get_users_in_session(s_id):
	# create message
	var message = GET_USERS_IN_SESSION + s_id
	
	# send message and receive response
	var res = await send_message(message, server_udp)
		
	# can't contact server		
	if res == 2:
		print("Can't contact with server")	
		
	if res == 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		
		# all ok
		if packet_string.begins_with(OK):
			return int(packet_string.split(":")[1])
	

func clear_server():
# create message
	var message = CLEAR_SERVER
	
	# send message and receive response
	var res = await send_message(message, server_udp)
		
	# can't contact server		
	if res == 2:
		print("Can't contact with server")	
		
	if res == 0:
		var array_bytes = server_udp.get_packet()
		var packet_string = array_bytes.get_string_from_ascii()
		
		# all ok
		if packet_string.begins_with(OK):
			return "ok"
	
