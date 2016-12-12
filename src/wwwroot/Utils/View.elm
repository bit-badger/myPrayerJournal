port module Utils.View exposing (..)

import Html exposing (..)
import Html.Attributes exposing (class, href, style, title)
import Html.Events exposing (defaultOptions, onWithOptions)
import Json.Decode as Json
import Messages exposing (Msg(..))


-- Set the document title
port documentTitle : String -> Cmd a


-- Wrap the given content in a row
row : List (Html Msg) -> Html Msg
row columns =
  div [ class "row "] columns


-- Display the given content in a full row
fullRow : List (Html Msg) -> Html Msg
fullRow content =
  row
    [ div
        [ class "col-xs-12" ]
        content
    ]


-- Create a navigation link
navLink : String -> String -> List (Attribute Msg) -> Html Msg
navLink url linkText attrs =
  let
    attributes =
      List.concat 
        [
          [ title linkText
          , onWithOptions
              "click" { defaultOptions | preventDefault = True }
              <| Json.succeed
              <| NavTo url
          , href url
          ]
          , attrs
        ]
  in
    a attributes [ text linkText ]
