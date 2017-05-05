/// URL routes for myPrayerJournal
module MyPrayerJournal.Route

/// The home page
let home = "/"

/// The main journal page
let journal = "/journal"

/// Routes dealing with users
module User =
  /// The route for user log on response from Auth0
  let logOn = "/user/log-on"
  let logOff = "/user/log-off"
  