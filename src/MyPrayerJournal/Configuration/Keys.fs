/// Magic strings? Look behind the curtain...
[<RequireQualifiedAccess>]
module MyPrayerJournal.Keys

/// Instance name for cookie authentication
let Authentication = "mpj-authentication"

/// The current user
let CurrentUser = "mpj-user"

/// The page generator
let Generator = "mpj-generator"

/// The request start ticks
let RequestTimer = "mpj-request-timer"