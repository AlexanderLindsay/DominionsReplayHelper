namespace DominionsReplayHelper.GUI

open NStack

module Utils =
    let ustr (x: string) = ustring.Make(x)
    let fromUstr (x: ustring) =
        match x with
        | null -> ""
        | _ -> x.ToString()