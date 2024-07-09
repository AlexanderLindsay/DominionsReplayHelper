namespace DominionsReplayHelper.GUI

open System
open System.Diagnostics
open System.Globalization
open System.Linq
open Terminal.Gui
open DominionsReplayHelper
open DominionsReplayHelper.GUI.Utils

module MainView =
    let createGameList games =
        games
        |> List.map (fun g -> sprintf "%s (Turn %d)" g.Name g.Turn)
        |> (fun l -> l.ToList())

    let getContent viewer state : View list =
        match state.State with
        | NotConfigured -> 
            let pathInput = new TextField(ustr "", X = 0, Y = 0,Width = Dim.op_Implicit 50)
            let setPathButton = new Button (ustr "Set Path", X = 0, Y = Pos.Bottom pathInput);
            setPathButton.add_Clicked (Action (fun () ->
                pathInput.Text
                |> fromUstr
                |> (fun s ->
                    if s.EndsWith("\\") then s else sprintf "%s\\" s
                )
                |> HelperState.setPath state
                |> viewer
            ))

            [
                pathInput;
                setPathButton;
            ]
        | Configured cs -> 
            let pathLabel = new Label (ustr <| sprintf "Dominions Saved Game Folder:%s" cs.SavedGameFolderPath)
            let gamesList = new ListView(createGameList cs.SavedGames, Width = Dim.Fill(), Height = Dim.Fill())
            gamesList.add_SelectedItemChanged (Action<ListViewItemEventArgs> (fun _ -> 
                gamesList.EnsureSelectedItemVisible()
            ))

            [
                pathLabel;
                gamesList;
            ]

    let rec view state =
        let top = Application.Top
        let margin = 3
        let win = 
            new Window (ustr "Dominions Replay Helper",
                X = Pos.At 1,
                Y = Pos.At 1,
                Width = Dim.Fill () - Dim.op_Implicit margin,
                Height = Dim.Fill () - Dim.op_Implicit margin)
        
        let content = getContent view state
        content
        |> List.iter (fun item ->
            win.Add item
        )

        top.RemoveAll ()
        top.Add win

    let MainView client =
        if Debugger.IsAttached then
            CultureInfo.DefaultThreadCurrentUICulture <- CultureInfo.GetCultureInfo ("en-US")

        let state = HelperState.init client
        
        Application.Init()
        view state

        Application.Run ()
        Application.Shutdown ()