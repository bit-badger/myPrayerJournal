module Update exposing (..)

import Models exposing (Model)
import Messages exposing (Msg(..))
import Navigation exposing (newUrl)
import Routing exposing (parseLocation)
import Utils.View exposing (documentTitle)


update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
  case msg of
    OnLocationChange location ->
      let
        newRoute = parseLocation location
      in
        ({model | route = newRoute}, Cmd.none)
    NavTo url ->
      (model, newUrl url)
    UpdateTitle newTitle ->
      (model, documentTitle newTitle)