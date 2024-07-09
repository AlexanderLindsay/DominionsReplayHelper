namespace DominionsReplayHelper.GUI

open System.Diagnostics
open System.Globalization
open NStack
open Terminal.Gui

module MainView =
    let ustr (x: string) = ustring.Make(x)

    let MainView () =
        if Debugger.IsAttached then
            CultureInfo.DefaultThreadCurrentUICulture <- CultureInfo.GetCultureInfo ("en-US")
        
        Application.Init()

        let top = Application.Top
        let margin = 3
        let win = 
            new Window (ustr "Hello",
                X = Pos.At 1,
                Y = Pos.At 1,
                Width = Dim.Fill () - Dim.op_Implicit margin,
                Height = Dim.Fill () - Dim.op_Implicit margin)
        
        top.Add win
        Application.Run ()
        Application.Shutdown ()