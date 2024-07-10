namespace DominionsReplayHelper

open System.IO
open Tommy

type ConfigData = {
    DominionsUrl: string
    SavedGamesPath: string option
}

module ConfigFile =

    let getNode key (table: TomlTable) =
        match table.TryGetNode key with
        | (true, i) -> Some i
        | _ -> None
    
    let nodeAsString (node: TomlNode) =
        if node.IsString then Some node.AsString.Value else None
    
    let getString key (table: TomlTable) =
        getNode key table
        |> Option.bind nodeAsString

    let parseConfig fileStream =
        let table = TOML.Parse(fileStream)
        let url = getString "DominionsUrl" table
        let path = getString "SavedGamesPath" table

        match url with
        | None -> Error "Could not parse dominions url from config file. \n Expecting `DominionsUrl = <url>`."
        | Some u -> 
            {
                DominionsUrl = u
                SavedGamesPath = path
            }
            |> Ok

    let buildConfigFile config =    
        let table = new TomlTable ()
        
        table.["DominionsUrl"] <- config.DominionsUrl

        match config.SavedGamesPath with
        | Some path ->
            table.["SavedGamesPath"] <- path
        | None -> ()

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