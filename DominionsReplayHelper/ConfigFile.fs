namespace DominionsReplayHelper

open System.IO
open Tommy

type GameConfiguration = {
    Name: string
    IsMultiplayer: bool
}

type ConfigData = {
    DominionsUrl: string
    SavedGamesPath: string option
    TurnNameFormat: string
    Games: GameConfiguration list
}

module ConfigFile =

    let getNode key (table: TomlTable) =
        match table.TryGetNode key with
        | (true, i) -> Some i
        | _ -> None
    
    let nodeAsString (node: TomlNode) =
        if node.IsString then Some node.AsString.Value else None

    let nodeAsBoolean (node: TomlNode) =
        if node.IsBoolean then Some node.AsBoolean.Value else None

    let nodeAsTable (node: TomlNode) =
        if node.IsTable then Some node.AsTable else None
    
    let getString key (table: TomlTable) =
        getNode key table
        |> Option.bind nodeAsString

    let getBoolean key (table: TomlTable) =
        getNode key table
        |> Option.bind nodeAsBoolean
    
    let getTableAsSeq key (table: TomlTable) =
        getNode key table
        |> Option.bind nodeAsTable
        |> Option.map (fun t -> t.Children)
        |> Option.defaultValue Seq.empty

    let parseConfig fileStream =
        let table = TOML.Parse(fileStream)
        let url = getString "DominionsUrl" table
        let path = getString "SavedGamesPath" table
        let turnNameFormat = getString "TurnNameFormat" table

        let games = 
            getTableAsSeq "games" table
            |> Seq.choose (fun node -> if node.IsTable then Some node.AsTable else None)
            |> Seq.choose (fun node ->
                let name = getString "Name" node
                let isMultiplayer = getBoolean "IsMultiplayer" node
                Option.map2 (fun name isMultiplayer -> 
                    {
                        Name = name;
                        IsMultiplayer = isMultiplayer;
                    }
                ) name isMultiplayer
            )
            |> List.ofSeq

        match (url, turnNameFormat) with
        | None, _ -> Error "Could not parse dominions url from config file. \n Expecting `DominionsUrl = <url>`."
        | _, None -> Error "Could not parse turn name format from config file. \n Expecting `TurnNameFormat = <format>`."
        | Some u, Some f -> 
            {
                DominionsUrl = u
                SavedGamesPath = path
                TurnNameFormat = f
                Games = games
            }
            |> Ok

    let buildConfigFile config =    
        let table = new TomlTable ()
        
        table.["DominionsUrl"] <- config.DominionsUrl
        table.["TurnNameFormat"] <- config.TurnNameFormat

        match config.SavedGamesPath with
        | Some path ->
            table.["SavedGamesPath"] <- path
        | None -> ()

        match config.Games with
        | [] -> ()
        | gs ->
            let gamesArray = new TomlArray ()
            gamesArray.IsTableArray <- true
            gamesArray.AddRange (
                gs
                |> List.map (fun g ->
                    let gameTable = new TomlTable ()
                    gameTable.["Name"] <- g.Name
                    gameTable.["IsMultiplayer"] <- g.IsMultiplayer
                    gameTable
                )
            )
            table.["Games"] <- gamesArray

        table

    let getConfigFile filename = 
        try
            File.OpenText filename
            |> Ok
        with
            | _ -> Error "Could not find or open file"
    
    let writeConfigFile filename (config: TomlTable) =
        try 
            let file = File.CreateText(filename)
            config.WriteTo(file)
            file.Flush()
            Ok true
        with
            | _ -> Error "Could not write config file"