module Update exposing (..)

import Dict
import Models exposing (Model)
import Messages exposing (Msg(..))
import Navigation exposing (newUrl)
import Routing exposing (Route(..), parseLocation)
import Utils.View exposing (documentTitle)


update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
  case msg of
    OnLocationChange location ->
      let
        newRoute = parseLocation location
        title =
          case newRoute of
            ChangePassword -> "Change Your Password"
            Home -> "Welcome"
            LogOn -> "Log On"
            LogOff -> "Log Off"
            NotFound -> "Page Not Found"
        pageTitle = title ++ " | myPrayerJournal"
      in
        ({ model | route = newRoute, title = pageTitle }, documentTitle model.title)
    
    NavTo url ->
      (model, newUrl url)

    UpdateTitle newTitle ->
      (model, documentTitle model.title)