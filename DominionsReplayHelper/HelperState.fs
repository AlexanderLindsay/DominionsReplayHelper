namespace DominionsReplayHelper

open System.IO

type ConfiguredHelperState = 
    {
        SavedGameFolderPath: string
        SavedGames: string list
    }

type HelperState = 
    | NotConfigured
    | Configured of ConfiguredHelperState

module HelperState =
    let listSavedGames path =
        Directory.EnumerateDirectories (path)
        |> Seq.map (fun d -> d.Replace(path, ""))
        |> List.ofSeq

    let init () =
        NotConfigured
    
    let setPath path =
        Configured { 
            SavedGameFolderPath = path
            SavedGames = listSavedGames path
        }
