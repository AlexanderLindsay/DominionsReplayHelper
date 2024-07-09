﻿namespace DominionsReplayHelper

type ConfiguredHelperState = 
    {
        SavedGameFolderPath: string
    }

type HelperState = 
    | NotConfigured
    | Configured of ConfiguredHelperState

module HelperState =
    let init () =
        NotConfigured
