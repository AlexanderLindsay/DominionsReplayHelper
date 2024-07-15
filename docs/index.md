A command line program that helps archive off multiplayer [Dominions 6](https://www.illwinter.com/dom6/docs.html) turns so the turns can be viewed later.

<details>
  <summary>How do I get this?</summary>
  Download the zip file from the latest release, <a href="https://github.com//AlexanderLindsay/DominionsReplayHelper/releases/latest/download/DominionsReplayHelperApp.zip">here</a>.
</details>
<details>
  <summary>How do I use this?</summary>
  After downloading the zip file, run the <code>DominionsReplayHelper.GUI.exe</code> file. A console will open up with the app. If this is the first time running the app, it will ask for the path to the dominions 6 save folder. This is most likely in <code>AppData/Roaming/Dominions6/savedgames</code>, but the app expects the full path.

  Once the path has been set, the app will display a list of the multiplayer games, the current turn, and if that turn has been saved or not. Highlighting the row and pressing Shift-S or clicking Save the button in the bottom bar will save the current turn.
</details>
<details>
  <summary>Why should I use this</summary>
  This is useful if you are in multiplayer games of Dominions 6 and want to save a record of your turns for later review. It is easily enough to do this by hand or with a powershell script, its just copying and renaming a folder. This app removes the need for any manual work so long as the assumptions it is making match yours. Those assumptions are as follows: its a multiplayer game, you want to save a turn only once, you don't mind copying the entire folder, your are going to remember to run the script before the turn finishes.
</details>
