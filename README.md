# Dominions Replay Helper

A basic command line program to help create copies of multiplayer dominion 6 game turns for later review. Basically finds the multiplayer games in your dominions 6 saved game folder, checks the current turn from the html page associated with that game, and then allows you to save that turn into a new saved game folder if said folder doesn't already exist.

## How to use

Run the `DominionsReplayHelper.GUI.exe` from a terminal. Set the path to the Dominions 6 save game folder. Select the multiplayer game you want to save and press `Shift-S`.

### Quick Save Use

Once the application has been configured you can quick save all the multiplayer games by adding `--saveAll` to the arguments of the program.

example:
`.\DominionsReplayHelper.GUI.exe --saveAll`

This will save the current turn for all the multiplayer games, exluding ones that already have a turn saved.