module Messages exposing (..)

import Navigation exposing (Location)


type Msg
  = OnLocationChange Location
  | NavTo String
  | UpdateTitle String