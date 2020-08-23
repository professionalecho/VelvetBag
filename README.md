# VelvetBag
A Discord bot that handles the setup of games of Blood on the Clocktower. VelvetBag allows Storytellers to specify who's in the game, which script you'd like to play, and (optionally) any roles you would definitely like to play with. It then randomly generates and assigns roles to every player, and (once the Storyteller has seen and approved the list) sends a private message to every player informing them of their role.

## Installation
You'll need to host the bot either locally or on a server, then bring it into your server. See the [Discord.Net introductory guides](https://discord.foxbot.me/stable/guides/getting_started/first-bot.html) for more information.

Just before you run VelvetBag, make sure each player has set their privacy settings to allow direct messages from server members, otherwise VelvetBag won't be able to inform them of their role.

## How It Works
Once installed and running, you can use three commands in succession to set up a game of Blood on the Clocktower with your discord. The three steps are: **!start**, **!set**, and **!send**.

### !Start (script)
In the server that contains your players, you can use !start to begin a game. It will check if you meet the requirements for being a storyteller (by default, your nickname in that server must begin with "!ST") before counting each player with a specified role (by default "in play"). All players need to have this role before the command is used!

When you use !start you must specify a script code to indicate which script of the game you're playing. The scripts available are:

| Script 	| Script code 	|
|-	|-	|
| Trouble Brewing 	| tb 	|
| Bad Moon Rising 	| bmr 	|
| Sects & Violets 	| sav 	|

### !Set [roles]
Storyteller, once you have started a game, you will receive a DM from VelvetBag inviting you to set the roles for the game. These are selected and assigned at random, but you can include any number of mandatory roles following the command, such as **!set baron** or **!set clockmaker "no dashii"**. Capitalisation doesn't matter, but roles with spaces in the name need to be surrounded by quotation marks.

You use **!send** in the DM channel with VelvetBag, so the players don't get a hint of what will be in the game!

You (and only you) will see the list of players and their roles. If you like what you see, you can go straight to **!send**. Otherwise, if you want to try a different setup (for example, the generator has given the Demon to a brand new player) you can just use **!set** again to generate a new set of roles. Remember to include any mandatory roles again!

### !Send
Finally, once you have a set of roles you're happy with, simply use **!send**. This final command will send a DM to each player in the game telling them their role. Everything else (first night actions, revealing the Demon to the Minions, and so on) is to be handled by the Storyteller.

## Special Cases

### Baron, Godfather, Fang Gu, and Vigormortis

Each of these roles affects setup by adding or removing a certain number of Outsiders, replacing or being replaced by Townsfolk. VelvetBag handles all these cases automatically, so your **!set** command should look legal if any of these roles are included.

### Drunk, Lunatic

Each of these roles believes they are a different role. Again, VelvetBag handles this for you. When these roles come up in the **!set** result, it will tell the Storyteller that that player is the Drunk or Lunatic, but believes themself to be (a random leftover Townsfolk).

When it comes time to **!send**, the player will of course only receive the role they believe themselves to be.

## Questions?

This was literally my first project in C# and my first bot. I'm very pleased with it, but if I did something wrong here on Github or in the code, please do let me know!
