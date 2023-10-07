module MyPrayerJournal.Data

/// Table(s) used by myPrayerJournal
module Table =

    /// Requests
    [<Literal>]
    let Request = "mpj.request"


open BitBadger.Npgsql.FSharp.Documents

module DataConnection =

    let ensureDb () = backgroundTask {
        do! Custom.nonQuery "CREATE SCHEMA IF NOT EXISTS mpj" []
        do! Definition.ensureTable Table.Request
        do! Definition.ensureIndex Table.Request Optimized
    }


/// Data access functions for requests
[<RequireQualifiedAccess>]
module Request =
    
    open NodaTime

    /// Add a request
    let add req = backgroundTask {
        do! insert Table.Request (RequestId.toString req.Id) req
    }

    /// Retrieve a request by its ID and user ID (includes history and notes)
    let tryByIdFull reqId userId = backgroundTask {
        match! Find.byId<Request> Table.Request (RequestId.toString reqId) with
        | Some req when req.UserId = userId -> return Some req
        | _ -> return None
    }
    
    /// Retrieve a request by its ID and user ID (excludes history and notes)
    let tryById reqId userId = backgroundTask {
        match! tryByIdFull reqId userId with
        | Some req -> return Some { req with History = [||]; Notes = [||] }
        | None -> return None
    }

    /// Does a request exist for the given request ID and user ID?
    let private existsById (reqId : RequestId) (userId : UserId) =
        Exists.byContains Table.Request {| Id = reqId; UserId = userId |}
    
    /// Update recurrence for a request
    let updateRecurrence reqId userId (recurType : Recurrence) = backgroundTask {
        let dbId = RequestId.toString reqId
        match! existsById reqId userId with
        | true -> do! Update.partialById Table.Request dbId {| Recurrence = recurType |}
        | false -> invalidOp "Request ID {dbId} not found"
    }

    /// Update the show-after time for a request
    let updateShowAfter reqId userId (showAfter : Instant) = backgroundTask {
        let dbId = RequestId.toString reqId
        match! existsById reqId userId with
        | true -> do! Update.partialById Table.Request dbId {| ShowAfter = showAfter |}
        | false -> invalidOp "Request ID {dbId} not found"
    }

    /// Update the snoozed and show-after values for a request
    let updateSnoozed reqId userId (until : Instant) = backgroundTask {
        let dbId = RequestId.toString reqId
        match! existsById reqId userId with
        | true -> do! Update.partialById Table.Request dbId {| SnoozedUntil = until; ShowAfter = until |}
        | false -> invalidOp "Request ID {dbId} not found"
    }


/// Specific manipulation of history entries
[<RequireQualifiedAccess>]
module History =

    /// Add a history entry
    let add reqId userId hist = backgroundTask {
        let dbId = RequestId.toString reqId
        match! Request.tryByIdFull reqId userId with
        | Some req -> do! Update.partialById Table.Request dbId {| History = Array.append [| hist |] req.History |}
        | None -> invalidOp $"Request ID {dbId} not found"
    }


/// Data access functions for journal-style requests
[<RequireQualifiedAccess>]
module Journal =

    /// Retrieve a user's answered requests
    let answered userId = backgroundTask {
        // TODO: only retrieve answered requests
        let! reqs = Find.byContains<Request> Table.Request {| UserId = UserId.toString userId |}
        return
            reqs
            |> Seq.ofList
            |> Seq.map JournalRequest.ofRequestFull
            |> Seq.filter (fun it -> it.LastStatus = Answered)
            |> Seq.sortByDescending (fun it -> it.AsOf)
            |> List.ofSeq
    }

    /// Retrieve a user's current prayer journal (includes snoozed and non-immediate recurrence)
    let forUser userId = backgroundTask {
        // TODO: only retrieve unanswered requests
        let! reqs = Find.byContains<Request> Table.Request {| UserId = UserId.toString userId |}
        return
            reqs
            |> Seq.ofList
            |> Seq.map JournalRequest.ofRequestFull
            |> Seq.filter (fun it -> it.LastStatus = Answered)
            |> Seq.sortByDescending (fun it -> it.AsOf)
            |> List.ofSeq
    }

    /// Does the user's journal have any snoozed requests?
    let hasSnoozed userId now = backgroundTask {
        let! jrnl = forUser userId
        return jrnl |> List.exists (fun r -> defaultArg (r.SnoozedUntil |> Option.map (fun it -> it > now)) false)
    }

    let tryById reqId userId = backgroundTask {
        let! req = Request.tryById reqId userId
        return req |> Option.map JournalRequest.ofRequestLite
    }


/// Specific manipulation of note entries
[<RequireQualifiedAccess>]
module Note =

    /// Add a note
    let add reqId userId note = backgroundTask {
        let dbId = RequestId.toString reqId
        match! Request.tryByIdFull reqId userId with
        | Some req -> do! Update.partialById Table.Request dbId {| Notes = Array.append [| note |] req.Notes |}
        | None -> invalidOp $"Request ID {dbId} not found"
    }

    /// Retrieve notes for a request by the request ID
    let byRequestId reqId userId = backgroundTask {
        match! Request.tryByIdFull reqId userId with Some req -> return req.Notes | None -> return [||]
    }
