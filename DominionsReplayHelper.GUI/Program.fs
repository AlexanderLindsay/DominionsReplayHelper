namespace DominionsReplayHelper.GUI
    
open System.Net.Http
open MainView

module entry =
    [<EntryPoint>]
    let main args =
        let client = new HttpClient()
        MainView client
        0