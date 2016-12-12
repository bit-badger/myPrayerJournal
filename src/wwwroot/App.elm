module App exposing (..)

import Messages exposing (..)
import Models exposing (Model, initialModel)
import Navigation exposing (Location)
import Routing exposing (Route(..), parseLocation)
import Update exposing (update)
import View exposing (view)


init : Location -> (Model, Cmd Msg)
init location =
  let
    currentRoute = Home --parseLocation location
  in
    (initialModel currentRoute, Cmd.none)


main : Program Never Model Msg
main =
  Navigation.program OnLocationChange
    { init = init
    , view = view
    , update = update
    , subscriptions = \_ -> Sub.none
    }
