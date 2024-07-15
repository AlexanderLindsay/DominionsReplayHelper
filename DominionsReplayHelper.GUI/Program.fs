namespace DominionsReplayHelper.GUI
    
open System.Net.Http
open DominionsReplayHelper
open MainView

module entry =
    [<EntryPoint>]
    let main args =
        let client = new HttpClient()
        match args with
        | [|"--saveAll"|] ->
            let configResult = 
                ConfigFile.getConfigFile configurationFilename
                |> Result.bind (fun file -> 
                    let result = ConfigFile.parseConfig file
                    file.Close()
                    result
                )
            
            match configResult with
            | Ok config ->
                async {
                    let! state = HelperState.init config client
                    match state.State with
                    | NotConfigured ->
                        printfn "Set Saved Game Path before using short cut."
                    | Configured data ->
                        data.SavedGames
                        |> List.filter (fun sg -> sg.IsMultiplayer)
                        |> List.iter (fun sg ->
                            printfn "Saving turn %d for %s" sg.Turn sg.Name
                            saveGameTurn data.SavedGameFolderPath state.TurnNameFormat (Some sg)
                        )
                }
                |> Async.RunSynchronously
            | Error err ->
                printfn "Error Getting Config File: %s" err
        | _ ->
            MainView client
        0