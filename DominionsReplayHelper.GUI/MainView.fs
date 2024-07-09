namespace DominionsReplayHelper.GUI

open System
open System.Diagnostics
open System.Globalization
open NStack
open Terminal.Gui
open DominionsReplayHelper

module MainView =
    let ustr (x: string) = ustring.Make(x)
    let fromUstr (x: ustring) =
        match x with
        | null -> ""
        | _ -> x.ToString()

    let getContent viewer state : View=
        match state with
        | NotConfigured -> 
            let pathInput = new TextField(ustr "", X = 0, Y = 0,Width = Dim.op_Implicit 50)
            let setPathButton = new Button (ustr "Set Path", X = 0, Y = Pos.Bottom pathInput);
            setPathButton.add_Clicked (Action (fun () ->
                pathInput.Text
                |> fromUstr
                |> HelperState.setPath
                |> viewer
            ))
            let frame = new FrameView (Rect (0, 0, 50, 4), ustr "Dominions Saved Game Folder", [|
                pathInput;
                setPathButton;
            |], Unchecked.defaultof<Border>)

            frame
        | Configured cs -> new Label (ustr <| sprintf "Dominions Saved Game Folder:%s" cs.SavedGameFolderPath)

    let rec view state =
        let top = Application.Top
        let margin = 3
        let win = 
            new Window (ustr "Hello",
                X = Pos.At 1,
                Y = Pos.At 1,
                Width = Dim.Fill () - Dim.op_Implicit margin,
                Height = Dim.Fill () - Dim.op_Implicit margin)
        
        let content = getContent view state
        win.Add content

        top.RemoveAll ()
        top.Add win

    let MainView () =
        if Debugger.IsAttached then
            CultureInfo.DefaultThreadCurrentUICulture <- CultureInfo.GetCultureInfo ("en-US")

        let state = HelperState.init ()
        
        Application.Init()
        view state

        Application.Run ()
        Application.Shutdown ()