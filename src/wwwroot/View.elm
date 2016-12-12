module View exposing (view)

import Html exposing (..)
import Html.Attributes exposing (attribute, class)
import Messages exposing (Msg(..))
import Models exposing (..)
import Routing exposing (Route(..))
import Utils.View exposing (documentTitle, navLink)

import Home.Public


-- Layout functions

navigation : List (Html Msg)
navigation =
  [ navLink "/user/password/change" "Change Your Password" [] 
  , navLink "/user/log-off" "Log Off" []
  , navLink "/user/log-on" "Log On" []
  ]
  |> List.map (\anchor -> li [] [ anchor ])


pageHeader : Html Msg
pageHeader =
  div
    [ class "navbar navbar-inverse navbar-fixed-top" ]
    [ div
        [ class "container" ]
        [ div
            [ class "navbar-header" ]
            [ button
                [ class "navbar-toggle"
                , attribute "data-toggle" "collapse"
                , attribute "data-target" ".navbar-collapse"
                ]
                [ span [ class "sr-only" ] [ text "Toggle navigation" ]
                , span [ class "icon-bar" ] []
                , span [ class "icon-bar" ] []
                , span [ class "icon-bar" ] []
                ]
            , navLink "/" "myPrayerJournal" [ class "navbar-brand" ]
            ]
        , div
            [ class "navbar-collapse collapse" ]
            [ ul
                [ class "nav navbar-nav navbar-right" ]
                navigation
            ]
        ]
    ]


pageTitle : String -> Html Msg
pageTitle title =
  let
    x = documentTitle <| title ++ " | myPrayerJournal"
  in
    h2 [ class "page-title" ] [ text title ]


pageFooter : Html Msg
pageFooter =
  footer
    [ class "mpj-footer" ]
    [ p
        [ class "text-right" ]
        [ text "myPrayerJournal v0.8.1" ]
    ]


layout : Model -> String -> List (Html Msg) -> Html Msg
layout model pgTitle contents =
  let
    pageContent = 
      [ [ pageTitle pgTitle ]
      , contents
      , [ pageFooter ]
      ]
      |> List.concat
  in
    div []
      [ pageHeader
      , div
          [ class "container body-content" ]
          pageContent
      ]


-- View functions

view : Model -> Html Msg
view model =
  case model.route of
    ChangePassword ->
      layout model "Change Your Password" [ text "password change page goes here" ]
    Home ->
      layout model "Welcome" (Home.Public.view model)
    LogOff ->
      layout model "Log Off" [ text "Log off page goes hwere" ]
    LogOn ->
      layout model "Log On" [ text "Log On page goes here" ]
    NotFound ->
      layout model "Page Not Found" [ text "404, dude" ]
