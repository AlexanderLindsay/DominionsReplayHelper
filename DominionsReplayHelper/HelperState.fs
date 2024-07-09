namespace DominionsReplayHelper

open System
open System.IO
open System.Net.Http
open System.Threading.Tasks
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

    let getCurrentTurn (client: HttpClient) game =
        let url = sprintf "http://ulm.illwinter.com/dom6/server/%s.html" game
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

    let listSavedGames state path =
        Directory.EnumerateDirectories (path)
        |> Seq.map (fun d -> d.Replace(path, ""))
        |> Seq.choose (fun name ->
            getCurrentTurn state.Client name
            |> Option.map (fun turn ->
                { 
                    Name = name
                    Turn = turn
                }
            )
        )
        |> List.ofSeq

    let init client =
        { 
            Client = client
            State = NotConfigured
        }
    
    let setPath state path =
        { state with 
            State =
                Configured { 
                    SavedGameFolderPath = path
                    SavedGames = listSavedGames state path
                }
        }
