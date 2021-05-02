# Boid-Up
Just boids ... going up 

Instructions:

1. Open up the server project (it should open to the sample scene that only has one object)

2. Open up Client project 

3. Start server first then start regular project and run a build for player 2




Connecting Options:

The clients by default are looking on your local machine for a server 
if you want to change this for testing you must go into the playerscript in the client project
and see commented lines 44-54 These show how to connect to a specific IP address and port 
(side note make sure the host the server is running has the 9000 port been port forwarded or else you wont be able to connect)

Game Logic:

Once the second player connects the game will start and teleport you a random point on the ground 
and assign each player a role, either  Hidder or Seeker, Seekers will know they are the Seeker 
as they will be marked as red and Hidder will be marked as white 
(Other player's materials dont update only yours so you will know only if you are the Seeker or not, not who actually is)
The game will keep track of how long each player is the Seeker for with the leaderboard in the top right

Chat Messages and Commands:

You can send chat messages using the textbox and the submit button (the text will update right above the textbox)
The game also has admin commands and these are only avaible to the first player who connects to the server
Command list: _ = space
"/setSpeed"_(number to set speed to)
"/setJump"_(number to set jump power to)
"/setBoidUpdate"_(number for how often boids update)
"/setTagDistance"_(number for how far tag distance is)
"/setTagTimer"_(number for how often tag can be swapped)
"/spawnAI"_(any number)(This command will only spawn 1 AI no matter what number you put but you must put a number for this commmand to work)

AI & Boids:

The AIs will remain black but they can be tagged just like a player can and their movement will be running away if they a hidder and seek if they are a seeker 
The green land masses are boids that are set to flock together only with their own flock (the standard setting is for there to be 4 flocks)
they are calculated server side and sent to the player for hiding or getting fancy with jumping around
Some boids will not move and this is because they dont have any neighboring boids of the SAME FLOCK if its flock comes within its neighbor range it will move and join them