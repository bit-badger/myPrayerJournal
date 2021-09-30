module Giraffe.Htmx

open System

/// HTMX request header values
type HtmxReqHeader =
/// Indicates that the request is via an element using `hx-boost`
| Boosted of string
/// The current URL of the browser
| CurrentUrl of Uri
/// `true` if the request is for history restoration after a miss in the local history cache
| HistoryRestoreRequest of bool
/// The user response to an hx-prompt
| Prompt of string
/// Always `true`
| Request of bool
/// The `id` of the target element if it exists
| Target of string
/// The `id` of the triggered element if it exists
| Trigger of string
/// The `name` of the triggered element if it exists
| TriggerName of string

/// Functions for manipulating htmx request headers
module HtmxReqHeader =
  /// True if this is an `HX-Boosted` header, false if not
  let isBoosted = function Boosted _ -> true | _ -> false
  /// True if this is an `HX-Current-URL` header, false if not
  let isCurrentUrl = function CurrentUrl _ -> true | _ -> false
  /// True if this is an `HX-History-Restore-Request` header, false if not
  let isHistoryRestoreRequest = function HistoryRestoreRequest _ -> true | _ -> false
  /// True if this is an `HX-Prompt` header, false if not
  let isPrompt = function Prompt _ -> true | _ -> false
  /// True if this is an `HX-Request` header, false if not
  let isRequest = function Request _ -> true | _ -> false
  /// True if this is an `HX-Target` header, false if not
  let isTarget = function Target _ -> true | _ -> false
  /// True if this is an `HX-Trigger` header, false if not
  let isTrigger = function Trigger _ -> true | _ -> false
  /// True if this is an `HX-Trigger-Name` header, false if not
  let isTriggerName = function TriggerName _ -> true | _ -> false


/// HTMX response header values
type HtmxResHeader =
/// Pushes a new url into the history stack
| Push of bool
/// Can be used to do a client-side redirect to a new location
| Redirect of string
/// If set to `true` the client side will do a a full refresh of the page
| Refresh of bool
/// Allows you to trigger client side events
| Trigger of obj
/// Allows you to trigger client side events after changes have settled
| TriggerAfterSettle of obj
/// Allows you to trigger client side events after DOM swapping occurs
| TriggerAfterSwap of obj


module Headers =
  
  open Microsoft.AspNetCore.Http
  open Microsoft.Extensions.Primitives

  /// Get the HTMX headers from the request context
  let fromRequest (ctx : HttpContext) =
    ctx.Request.Headers.Keys
    |> Seq.filter (fun key -> key.StartsWith "HX-")
    |> Seq.map (fun key ->
        let v = ctx.Request.Headers.[key].[0]
        match key with
        | "HX-Boosted" -> v |> (Boosted >> Some)
        | "HX-Current-URL" -> v |> (Uri >> CurrentUrl >> Some)
        | "HX-History-Restore-Request" -> v |> (bool.Parse >> HistoryRestoreRequest >> Some)
        | "HX-Prompt" -> v |> (Prompt >> Some)
        | "HX-Request" -> v |> (bool.Parse >> Request >> Some)
        | "HX-Target" -> v |> (Target >> Some)
        | "HX-Trigger" -> v |> (HtmxReqHeader.Trigger >> Some)
        | "HX-Trigger-Name" -> v |> (TriggerName >> Some)
        | _ -> None
        )
    |> Seq.filter Option.isSome
    |> Seq.map Option.get
    |> List.ofSeq
  
  /// Add an htmx header to the response
  let toResponse (hdr : HtmxResHeader) : HttpHandler =
    let toJson (it : obj) =
      match it with
      | :? string as x -> x
      | _ -> "" // TODO: serialize object
    fun next ctx -> task {
      match hdr with
      | Push push -> "HX-Push", string push
      | Redirect url -> "HX-Redirect", url
      | Refresh refresh -> "HX-Refresh", string refresh
      | Trigger trig -> "HX-Trigger", toJson trig
      | TriggerAfterSettle trig -> "HX-Trigger-After-Settle", toJson trig
      | TriggerAfterSwap trig -> "HX-Trigger-After-Swap", toJson trig
      |> function (k, v) -> ctx.Response.Headers.Add (k, StringValues v)
      return! next ctx
    }
