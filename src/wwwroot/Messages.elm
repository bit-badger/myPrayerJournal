module Messages exposing (..)

import Navigation exposing (Location)
import Routing exposing (Route)


type Msg
  = OnLocationChange Location
  | NavTo String
  | UpdateTitle String