module Models exposing (..)

import Routing exposing (Route(..))


type alias Model =
  { route : Route
  , title : String
  }


initialModel : Route -> Model
initialModel route =
  { route = route
  , title = "Index"
  }
