namespace DominionsReplayHelper

open System
open System.IO
open System.Net.Http
open System.Text.RegularExpressions

type Game = 
    {
        Name: string
        Turn: int
    }

type ConfiguredHelperState = 
    {
        SavedGameFolderPath: string
        SavedGames: Game list
    }

type HelperState = 
    | NotConfigured
    | Configured of ConfiguredHelperState

type StateWithClient =
    {
        Client: HttpClient
        Url: string
        State: HelperState
    }

module HelperState =
    let turnRegex = Regex(@", turn (\d+)", RegexOptions.Compiled)

    let toInt (str: string) =
        match Int32.TryParse str with
        | (true, i) -> Some i
        | _ -> None

    let parseTurn html =
        let turnMatch = turnRegex.Match(html)
        if turnMatch.Groups.Count >= 2 then (Some <| turnMatch.Groups.Item 1) else None
        |> Option.bind (fun g -> 
            toInt g.Value
        )

    let getCurrentTurn url (client: HttpClient) game =
        let url = sprintf "%s%s.html" url game
        let response = 
            async {
                return! Async.AwaitTask (client.GetStringAsync(url))
            }
            |> Async.Catch 
            |> Async.RunSynchronously
        match response with
        | Choice1Of2 html ->
            let turn = parseTurn html
            turn
        | _ -> None

    let listSavedGames url client path =
        Directory.EnumerateDirectories (path)
        |> Seq.map (fun d -> d.Replace(path, ""))
        |> Seq.choose (fun name ->
            getCurrentTurn url client name
            |> Option.map (fun turn ->
                { 
                    Name = name
                    Turn = turn
                }
            )
        )
        |> List.ofSeq

    let init config client =
        let state =
            match config.SavedGamesPath with
            | None -> NotConfigured
            | path ->
                path
                |> Option.map (fun p ->
                    Configured {
                        SavedGameFolderPath = p
                        SavedGames = listSavedGames config.DominionsUrl client p
                    }
                )
                |> Option.defaultValue NotConfigured

        { 
            Client = client
            Url = config.DominionsUrl
            State = state
        }
    
    let setPath state path =
        { state with 
            State =
                Configured { 
                    SavedGameFolderPath = path
                    SavedGames = listSavedGames state.Url state.Client path
                }
        }
