namespace DominionsReplayHelper.GUI

open System
open System.Diagnostics
open System.Globalization
open System.Linq
open Terminal.Gui
open DominionsReplayHelper
open DominionsReplayHelper.GUI.Utils

module MainView =
    open System.Threading.Tasks
    let configurationFilename = "Configuration.toml"

    let displayError title message =
        MessageBox.ErrorQuery (50, 7, ustr title, ustr message, ustr "Ok")
        |> ignore
    
    let createProgressBar () =
        let progress = new ProgressBar (BidirectionalMarquee = false, X = Pos.Center (), Width = Dim.Percent (50f, false), Height = 2)
        Application.Top.Add progress

        let timer = Func<MainLoop, bool> (fun _ ->
            progress.Pulse ()
            true
        )

        let progressTimerToken = Application.MainLoop.AddTimeout (TimeSpan.FromMilliseconds (50.), timer)
        progressTimerToken

    let updateConfig state =
        let config = {
            DominionsUrl = state.Url
            SavedGamesPath = 
                match state.State with
                | NotConfigured -> None
                | Configured d -> Some d.SavedGameFolderPath
        }

        let result = 
            ConfigFile.buildConfigFile config
            |> ConfigFile.writeConfigFile configurationFilename
        
        match result with
        | Error msg -> displayError "Configuration Error" msg
        | Ok _ -> ()

        state

    let createGameList games =
        games
        |> List.map (fun g -> sprintf "%s (Turn %d)" g.Name g.Turn)
        |> (fun l -> l.ToList())

    let getContent viewer state : View list =
        match state.State with
        | NotConfigured -> 
            let pathLabel = new Label (ustr "Dominions Saved Game Folder", X = 1, Y = 1)
            let pathInput = new TextField(ustr "", X = 1, Y = Pos.Bottom pathLabel, Width = Dim.op_Implicit 50)
            let setPathButton = new Button (ustr "Set Path", X = 1, Y = Pos.Bottom pathInput);
            setPathButton.add_Clicked (Action (fun () ->
                let progressToken = createProgressBar ()
                
                Application.MainLoop.Invoke(fun () ->
                    async {
                        let! state' = 
                            pathInput.Text
                            |> fromUstr
                            |> (fun s ->
                                if s.EndsWith("\\") then s else sprintf "%s\\" s
                            )
                            |> HelperState.setPath state

                        let updatedState = updateConfig state'
                        viewer updatedState

                        Application.MainLoop.RemoveTimeout (progressToken) |> ignore
                    }
                    |> Async.StartAsTask
                    |> ignore
                )
            ))

            [
                pathLabel;
                pathInput;
                setPathButton;
            ]
        | Configured cs -> 
            let pathLabel = new Label (ustr <| sprintf "Dominions Saved Game Folder: %s" cs.SavedGameFolderPath, X = 1, Y = 1)
            let gamesList = new ListView(createGameList cs.SavedGames, Width = Dim.Fill(), Height = Dim.Fill())
            gamesList.add_SelectedItemChanged (Action<ListViewItemEventArgs> (fun _ -> 
                gamesList.EnsureSelectedItemVisible()
            ))

            [
                pathLabel;
                gamesList;
            ]

    let Quit () =
        MessageBox.Query (50, 7, ustr "Quit Dominions Replay Helper", ustr "Are you sure you want to quit?", ustr "Yes", ustr "No") = 0

    let createWindow () =
        let top = Application.Top
        let margin = 3
        let win = 
            new Window (ustr "Dominions Replay Helper",
                X = Pos.At 1,
                Y = Pos.At 1,
                Width = Dim.Fill () - Dim.op_Implicit margin,
                Height = Dim.Fill () - Dim.op_Implicit margin)
        
        top.RemoveAll ()
        top.Add win

        let bottomBar = new StatusBar ([|
            StatusItem(Key.Q, ustr "~Ctrl-Q~ Quit", fun () -> if (Quit()) then top.Running <- false)
        |])

        top.Add bottomBar
        win

    let rec view state =
        let win = createWindow ()

        let content = getContent view state
        content
        |> List.iter (fun item ->
            win.Add item
        )

    let MainView client =
        if Debugger.IsAttached then
            CultureInfo.DefaultThreadCurrentUICulture <- CultureInfo.GetCultureInfo ("en-US")

        let configResult = 
            ConfigFile.getConfigFile configurationFilename
            |> Result.bind (fun file -> 
                ConfigFile.parseConfig file
            )
        
        Application.Init()

        createWindow () |> ignore
        let progressTimerToken = createProgressBar ()

        Application.MainLoop.Invoke(fun () ->
            async {
                do! Task.Delay (10) |> Async.AwaitTask
                match configResult with
                | Ok config ->
                    let! state = HelperState.init config client
                    view state
                | Error message ->
                    displayError "Configuration Error" message
                Application.MainLoop.RemoveTimeout progressTimerToken |> ignore
            } |> Async.StartAsTask
            |> ignore
        )

        Application.Run ()
        Application.Shutdown ()