module View exposing (view)

import Html exposing (Html, button, div, footer, h2, li, p, span, text, ul)
import Html.Attributes exposing (attribute, class)
import Messages exposing (Msg(..))
import Models exposing (..)
import Routing exposing (Route(..))
import String exposing (split, trim)
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


pageTitle : Model -> Html Msg
pageTitle model =
  let
    title = 
      case List.head <| split "|" model.title of
        Just ttl ->
          trim ttl
        Nothing ->
          ""
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


layout : Model -> List (Html Msg) -> Html Msg
layout model contents =
  let
    pageContent = 
      [ [ pageTitle model ]
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
      layout model [ text "password change page goes here" ]
    Home ->
      layout model Home.Public.view
    LogOff ->
      layout model [ text "Log off page goes here" ]
    LogOn ->
      layout model [ text "Log On page goes here" ]
    NotFound ->
      layout model [ text "404, dude" ]
