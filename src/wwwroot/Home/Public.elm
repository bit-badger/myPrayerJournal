module Home.Public exposing (view)

import Html exposing (Html, p, text)
import Messages exposing (Msg(..))
import Models exposing (Model)
import Utils.View exposing (fullRow)


view : Model -> List (Html Msg)
view model =
  let
    paragraphs =
      [ " "
      , "myPrayerJournal is a place where individuals can record their prayer requests, record that they prayed for them, update them as God moves in the situation, and record a final answer received on that request.  It will also allow individuals to review their answered prayers."
      , "This site is currently in very limited alpha, as it is being developed with a core group of test users.  If this is something you are interested in using, check back around mid-November 2016 to check on the development progress."
      ]
    |> List.map (\para -> p [] [ text para ])
  in
    [ fullRow paragraphs ]
