extends GutTest

var sockets = []

func before_each():
	pass
	
func after_each():
	var res = await ServerConnection.clear_server()
	
	PlayerMenu.clear()
	CurrentSessionInfo.clear()
	
	
func before_all():
	pass
	
func after_all():
	pass

########################
# CREATE SESSION TESTS #
########################
func test_create_sesion_when_user_not_exist():
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	PlayerMenu.id = "5000" # this user doesn't exist in server
	# create one session
	var res  = await ServerConnection.create_session(str(false), str(2), str(false))
	assert_eq("User doesn't exists", res, "Should pass")
	
	
func test_user_creates_session_twice():
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	
	# check user has been registered correctly
	assert_eq('ok', res, "Should pass")

	# create one session
	res  = await ServerConnection.create_session(str(false), str(2), str(false))
	assert_eq('ok', res, "Should pass")
	
	# check if session has been registered
	res = await ServerConnection.get_active_session()
	assert_eq(1, res, "Should pass")
	
	# try to create another session
	res = await ServerConnection.create_session(str(false), str(2), str(false))
	assert_eq('ok', res, "Should pass")
	
	# check that there is only one session
	res = await ServerConnection.get_active_session()
	assert_eq(1, res, "Should pass")
	
	
	
######################
# FIND SESSION TESTS #
######################
func test_find_session_when_user_not_exist():
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	PlayerMenu.id = "5000"
	
	# create one session
	var res  = await ServerConnection.find_session(str(false), str(2))
	assert_eq("User doesn't exists", res, "Should pass")
	

func test_find_session_twice():
	# create neccesary sockets
	var socket = PacketPeerUDP.new()
	
	# initialize socket
	ServerConnection.start_peer_udp(socket, 5000)
	var port = socket.get_local_port()
	var ip = ServerConnection.get_private_ip()
	
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	
	# check user has been registered correctly
	assert_eq('ok', res, "Should pass")

	# create one session (to find)
	res  = await ServerConnection.create_session(str(false), str(4), str(false))
	assert_eq('ok', res, "Should pass")
	
	
	# create another user (who will find session)
	PlayerMenu.usr_name = "user2"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5555
	PlayerMenu.peer_port = 5556
	res = await ServerConnection.register_player()
	
	# find session
	res = await ServerConnection.find_session(str(false), str(4))
	assert_eq('ok', res, "Should pass")
	var s_id = CurrentSessionInfo.s_id
	
	# check if user is in session
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(2, res, "Should pass")
	
	# try to find another session
	res = await ServerConnection.find_session(str(false), str(4))
	assert_eq('ok', res, "Should pass")
	
	# check that session keeps having two users
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(2, res, "Should pass")

func test_find_session_when_session_with_that_properties_not_exist():
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	
	# check user has been registered correctly
	assert_eq('ok', res, "Should pass")

	# create one session (to find)
	res  = await ServerConnection.find_session(str(false), str(4))
	assert_eq("Doesn't exist any session with that properties!", res, "Should pass")


##############################
# FIND SESSION BY CODE TESTS #
##############################
func test_find_session_by_code_when_user_not_exist():
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	PlayerMenu.id = "5000"
	
	# create one session
	var res  = await ServerConnection.find_session_by_code("AAAAAAAA")
	assert_eq("User doesn't exists", res, "Should pass")
	

func test_find_session_by_code_twice():
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	
	# check user has been registered correctly
	assert_eq('ok', res, "Should pass")

	# create one session (to find)
	res  = await ServerConnection.create_session(str(false), str(4), str(false))
	assert_eq('ok', res, "Should pass")
	
	var s_id = CurrentSessionInfo.s_id
	
	# create another user (who will find session)
	PlayerMenu.usr_name = "user2"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5555
	PlayerMenu.peer_port = 5556
	res = await ServerConnection.register_player()
	
	# find session
	res = await ServerConnection.find_session_by_code(s_id)
	assert_eq('ok', res, "Should pass")
	
	# check if user is in session
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(2, res, "Should pass")
	
	# try to find another session
	res = await ServerConnection.find_session_by_code(s_id)
	assert_eq('ok', res, "Should pass")
	
	# check that session keeps having two users
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(2, res, "Should pass")
	
	
func test_find_session_by_code_when_session_is_full():
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	
	# check user has been registered correctly
	assert_eq('ok', res, "Should pass")

	# create one session (to find)
	res  = await ServerConnection.create_session(str(false), str(2), str(false))
	assert_eq('ok', res, "Should pass")
	
	
	# create another user (who will find session)
	PlayerMenu.usr_name = "user2"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5555
	PlayerMenu.peer_port = 5556
	res = await ServerConnection.register_player()
	
	# find session
	res = await ServerConnection.find_session(str(false), str(2))
	assert_eq('ok', res, "Should pass")
	var s_id = CurrentSessionInfo.s_id
	
	# check if user is in session
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(2, res, "Should pass")
	
	
	
	# create another user and try to find that session
	PlayerMenu.usr_name = "user3"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5555
	PlayerMenu.peer_port = 5556
	res = await ServerConnection.register_player()
	
	# find session
	res = await ServerConnection.find_session_by_code(s_id)
	assert_eq('Session is full!', res, "Should pass")
	
	# check that there are only 2 users in session
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(2, res, "Should pass")

#######################
# LEAVE SESSION TESTS #
#######################
# check that list is updated correctly
func test_leave_session():	
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")
	var first_player_id = PlayerMenu.id

	# check if server user list has one user
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")

	# user creates session
	res  = await ServerConnection.create_session(str(false), str(4), str(false))
	assert_eq('ok', res, "Should pass")
	
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user2"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5551
	PlayerMenu.peer_port = 5552
	
	# register second player
	res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")

	# user2 finds session
	res = await ServerConnection.find_session(str(false), str(4))
	assert_eq('ok', res, "Should pass")
	var s_id = CurrentSessionInfo.s_id
	
	# check if there are two users in session
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(2, res, "should pass")

	# second player leaves session
	res = await ServerConnection.leave_session()
	assert_eq('ok', res, "should pass")

	# check that there is only one user in session
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(1, res, "Should pass")
	
	
# leave session when session is full
func test_leave_session_when_session_is_full():
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")
	var first_player_id = PlayerMenu.id

	# check if server user list has one user
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")

	# user creates session
	res  = await ServerConnection.create_session(str(false), str(2), str(false))
	assert_eq('ok', res, "Should pass")
	
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user2"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5551
	PlayerMenu.peer_port = 5552
	
	# register second player
	res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")

	# user2 finds session
	res = await ServerConnection.find_session(str(false), str(2))
	assert_eq('ok', res, "Should pass")
	var s_id = CurrentSessionInfo.s_id
	
	# check if there are two users in session
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(2, res, "should pass")

	# second player leaves session
	res = await ServerConnection.leave_session()
	assert_eq("Session is full, you can't leave it now!", res, "should pass")

	# check that there is only one user in session
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(2, res, "Should pass")
	

# try to leave session when user isn't in a session
func test_leave_session_when_user_not_in_session():
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	CurrentSessionInfo.s_id = "null"	

	# register player
	var res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")
	var first_player_id = PlayerMenu.id

	# check if server user list has one user
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")

	# second player leaves session
	res = await ServerConnection.leave_session()
	assert_eq("User not in a session", res, "should pass")


# try to leave a session when user is not registered
func test_leave_session_when_user_not_exists():
	# Initialize info in PlayerMenu
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")
	var first_player_id = PlayerMenu.id

	# check if server user list has one user
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")

	# user creates session
	res  = await ServerConnection.create_session(str(false), str(2), str(false))
	assert_eq('ok', res, "Should pass")
	
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user2"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5551
	PlayerMenu.peer_port = 5552
	PlayerMenu.id = "-1"
	
	# second player leaves session
	res = await ServerConnection.leave_session()
	assert_eq("User doesn't exists", res, "should pass")

	
########################
# REMOVE SESSION TESTS #
########################
# check that list is updated correctly
func test_remove_session():	
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")
	var first_player_id = PlayerMenu.id

	# check if server user list has one user
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")

	# user creates session
	res  = await ServerConnection.create_session(str(false), str(4), str(false))
	assert_eq('ok', res, "Should pass")
	
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user2"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5551
	PlayerMenu.peer_port = 5552
	
	# register second player
	res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")

	# user2 finds session
	res = await ServerConnection.find_session(str(false), str(4))
	assert_eq('ok', res, "Should pass")
	var s_id = CurrentSessionInfo.s_id
	
	# check if there are two users in session
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(2, res, "should pass")

	# switch to session admin
	PlayerMenu.id = first_player_id

	# second player leaves session
	res = await ServerConnection.remove_session()
	assert_eq('ok', res, "should pass")

	# check that there is only one user in session
	res = await ServerConnection.get_active_session()
	assert_eq(0, res, "Should pass")
	
	
# leave session when session is full
func test_remove_session_when_session_is_full():
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")
	var first_player_id = PlayerMenu.id

	# check if server user list has one user
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")

	# user creates session
	res  = await ServerConnection.create_session(str(false), str(2), str(false))
	assert_eq('ok', res, "Should pass")
	
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user2"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5551
	PlayerMenu.peer_port = 5552
	
	# register second player
	res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")

	# user2 finds session
	res = await ServerConnection.find_session(str(false), str(2))
	assert_eq('ok', res, "Should pass")
	var s_id = CurrentSessionInfo.s_id
	
	# check if there are two users in session
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(2, res, "should pass")

	
	# switch to player 1
	PlayerMenu.id = first_player_id

	# first player removes session
	res = await ServerConnection.remove_session()
	assert_eq("Session is full, you can't leave it now!", res, "should pass")

	# check that there is only one user in session
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(2, res, "Should pass")
	

# try to remove session when user isn't in a session
func test_remove_session_when_user_not_in_session():
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	CurrentSessionInfo.s_id = "null"	

	# register player
	var res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")
	var first_player_id = PlayerMenu.id

	# check if server user list has one user
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")

	# second player leaves session
	res = await ServerConnection.remove_session()
	assert_eq("User not in a session", res, "should pass")


# try to leave a session when user is not registered
func test_remove_session_when_user_not_exists():
	# Initialize info in PlayerMenu
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")
	var first_player_id = PlayerMenu.id

	# check if server user list has one user
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")

	# user creates session
	res  = await ServerConnection.create_session(str(false), str(2), str(false))
	assert_eq('ok', res, "Should pass")
	
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user2"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5551
	PlayerMenu.peer_port = 5552
	PlayerMenu.id = "-1"
	
	# second player leaves session
	res = await ServerConnection.remove_session()
	assert_eq("User doesn't exists", res, "should pass")

# try to remove a session when player isn't admin
func test_remove_session_when_user_not_admin():
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")


	# check if server user list has one user
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")

	# user creates session
	res  = await ServerConnection.create_session(str(false), str(4), str(false))
	assert_eq('ok', res, "Should pass")
	
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user2"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5551
	PlayerMenu.peer_port = 5552
	
	# register second player
	res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")

	# user2 finds session
	res = await ServerConnection.find_session(str(false), str(4))
	assert_eq('ok', res, "Should pass")
	var s_id = CurrentSessionInfo.s_id
	
	# check if there are two users in session
	res = await ServerConnection.get_users_in_session(s_id)
	assert_eq(2, res, "should pass")

	# second player leaves session
	res = await ServerConnection.remove_session()
	assert_eq('Only session admin can remove this session', res, "should pass")

	# check that there is only one user in session
	res = await ServerConnection.get_active_session()
	assert_eq(1, res, "Should pass")
	

	
###################
# EXIT GAME TESTS #
###################
# exit when user isn't in a session
func test_exit_game_user_not_in_session():
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	
	# check user has been registered correctly
	assert_eq('ok', res, "Should pass")

	# check if server user list has one user
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")
	
	# exit game
	res = await ServerConnection.exit_game()
	assert_eq("ok", res.split(":")[0], "Should pass")
	print(res.split(":")[1])
	# check that there isn't any player in server list
	res = await ServerConnection.get_active_users()

	assert_eq(0, res, "Should pass")

# exit when user is in a session created by him, and no other users are in session
func test_exit_game_user_in_existing_session():
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")

	# check if server user list has one user
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")

	# user creates session
	res  = await ServerConnection.create_session(str(false), str(2), str(false))
	assert_eq('ok', res, "Should pass")
	
	
	# exit game
	res = await ServerConnection.exit_game()
	assert_eq('ok', res.split(":")[0], "should pass")
	assert_eq('User and session removed', res.split(":")[1], "should pass")

	# check that there isn't any player in server list
	res = await ServerConnection.get_active_users()
	assert_eq(0, res, "Should pass")
	
	# check if session has been deleted properly
	res = await ServerConnection.get_active_session()
	assert_eq(0, res, "Should pass")
	

# exit game when user is in a session created by another user
func test_exit_game_user_in_existing_session_created_by_another_user():
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")

	# check if server user list has one user
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")

	# user creates session
	res  = await ServerConnection.create_session(str(false), str(4), str(false))
	assert_eq('ok', res, "Should pass")
	
	
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user2"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5551
	PlayerMenu.peer_port = 5552
	
	# register second player
	res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")

	# user2 finds session
	res = await ServerConnection.find_session(str(false), str(4))
	assert_eq('ok', res, "Should pass")

	# exit game
	res = await ServerConnection.exit_game()
	assert_eq('ok', res.split(":")[0], "should pass")
	assert_eq('User removed', res.split(":")[1], "should pass")

	# check that there isn't any player in server list
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")
	
	# check that session hasn't been deleted
	res = await ServerConnection.get_active_session()
	assert_eq(1, res, "Should pass")


# exit game when user is in a session created by him and there are more players in the room
func test_exit_game_user_in_existing_session_when_more_users_are_in_session():
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user1"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5500
	PlayerMenu.peer_port = 5501
	
	# register player
	var res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")
	var first_player_id = PlayerMenu.id

	# check if server user list has one user
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")

	# user creates session
	res  = await ServerConnection.create_session(str(false), str(4), str(false))
	assert_eq('ok', res, "Should pass")
	
	
	# Initialize info in PlayerMenu
	PlayerMenu.usr_name = "user2"
	PlayerMenu.private_ip = "0.0.0.0"
	PlayerMenu.private_port = 5551
	PlayerMenu.peer_port = 5552
	
	# register second player
	res = await ServerConnection.register_player()
	assert_eq('ok', res, "Should pass")

	# user2 finds session
	res = await ServerConnection.find_session(str(false), str(4))
	assert_eq('ok', res, "Should pass")


	# switch to session server (first player)
	PlayerMenu.id = first_player_id

	# exit game
	res = await ServerConnection.exit_game()
	assert_eq('ok', res.split(":")[0], "should pass")
	assert_eq('User and session removed', res.split(":")[1], "should pass")

	# check that there isn't any player in server list
	res = await ServerConnection.get_active_users()
	assert_eq(1, res, "Should pass")
	
	# check that session hasn't been deleted
	res = await ServerConnection.get_active_session()
	assert_eq(0, res, "Should pass")
