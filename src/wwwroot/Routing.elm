module Routing exposing (..)

import Navigation exposing (Location)
import UrlParser  exposing (..)


type Route
  = Home
  | ChangePassword
  | LogOff
  | LogOn
  | NotFound


findRoute : Parser (Route -> a) a
findRoute =
  oneOf
    [ map Home top
    , map LogOn (s "user" </> s "log-on")
    , map LogOff (s "user" </> s "log-off")
    , map ChangePassword (s "user" </> s "password" </> s "change")
    ]


parseLocation : Location -> Route
parseLocation location =
  case (parsePath findRoute location) of
    Just route ->
      route
    Nothing ->
      NotFound
