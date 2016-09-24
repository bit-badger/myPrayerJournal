module MyPrayerJournal.Strings

open Newtonsoft.Json
open System.Collections.Generic
open System.IO

/// The locales we'll try to load
let private supportedLocales = [ "en-US" ]

/// The fallback locale, if a key is not found in a non-default locale
let private fallbackLocale = "en-US"

/// Get an embedded JSON file as a string
let private getEmbedded locale =
  use stream = new FileStream((sprintf "resources/%s.json" locale), FileMode.Open)
  use rdr = new StreamReader(stream)
  rdr.ReadToEnd()

/// The dictionary of localized strings
let private strings =
  supportedLocales
  |> List.map (fun loc -> loc, JsonConvert.DeserializeObject<Dictionary<string, string>>(getEmbedded loc))
  |> dict

/// Get a key from the resources file for the given locale
let getForLocale locale key =
  let getString thisLocale = 
    match strings.ContainsKey thisLocale with
    | true -> match strings.[thisLocale].ContainsKey key with
              | true -> Some strings.[thisLocale].[key]
              | _ -> None
    | _ -> None
  match getString locale with
  | Some xlat -> Some xlat
  | _ when locale <> fallbackLocale -> getString fallbackLocale
  | _ -> None
  |> function Some xlat -> xlat | _ -> sprintf "%s.%s" locale key

/// Translate the key for the current locale
let get key = getForLocale System.Globalization.CultureInfo.CurrentCulture.Name key
