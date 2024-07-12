namespace DominionsReplayHelper

open System
open System.IO
open System.Net.Http
open System.Text.RegularExpressions

type Game = 
    {
        Name: string
        Turn: int
        IsMultiplayer: bool
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
    open System.Threading.Tasks
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
        async {
            let url = sprintf "%s%s.html" url game
            let! response = 
                client.GetStringAsync(url)
                |> Async.AwaitTask
                |> Async.Catch 
            match response with
            | Choice1Of2 html ->
                let turn = parseTurn html
                return turn
            | _ -> return None
        }
    
    let createGame name turn isMultiplayer =
        {
            Name = name;
            Turn = turn;
            IsMultiplayer = isMultiplayer;
        }

    let listSavedGames url (priorGames: GameConfiguration list) client path =
        let priorGamesMap = 
            priorGames
            |> List.map (fun pg -> pg.Name, pg.IsMultiplayer)
            |> Map.ofList

        async {
            let! games = 
                Directory.EnumerateDirectories (path)
                |> Seq.map (fun d -> d.Replace(path, ""))
                |> Seq.filter (fun g -> g <> "newlords") //filter out the folder used to store saved gods
                |> Seq.map (fun name ->
                    async {
                        let isMultiplayer =
                            priorGamesMap
                            |> Map.tryFind name
                        
                        match isMultiplayer with
                        | Some im -> 
                            match im with
                            | false ->
                                return createGame name 1 false
                            | true ->
                                let! currentTurnResult = getCurrentTurn url client name
                                let currentTurn =
                                    currentTurnResult
                                    |> Option.map (fun turn ->
                                        createGame name turn true
                                    )
                                    |> Option.defaultValue (createGame name 1 false)
                                return currentTurn
                        | None ->
                            let! currentTurnResult = getCurrentTurn url client name
                            let currentTurn =
                                currentTurnResult
                                |> Option.map (fun turn ->
                                    createGame name turn true
                                )
                                |> Option.defaultValue (createGame name 1 false)
                            return currentTurn
                    }
                )
                |> Async.Parallel

            let gamesList = 
                games
                |> List.ofSeq
            
            return gamesList
        }

    let init config client =
        async {
            let! state = 
                match config.SavedGamesPath with
                | None -> 
                    NotConfigured
                    |> Task.FromResult
                    |> Async.AwaitTask
                | Some path ->
                    async {
                        let! savedGames = listSavedGames config.DominionsUrl config.Games client path
                        return Configured {
                            SavedGameFolderPath = path
                            SavedGames = savedGames
                        }
                    }

            return { 
                Client = client
                Url = config.DominionsUrl
                State = state
            }
        }
    
    let setPath state path =
        async {
            let! savedGames = listSavedGames state.Url [] state.Client path
            let state' = 
                { state with 
                    State =
                        Configured { 
                            SavedGameFolderPath = path
                            SavedGames = savedGames
                        }
                }
            return state'
        }
