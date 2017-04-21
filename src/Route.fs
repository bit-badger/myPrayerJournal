/// URL routes for myPrayerJournal
module MyPrayerJournal.Route

/// The home page
let home = "/"

/// Routes dealing with users
module User =
  /// The route for user log on response from Auth0
  let logOn = "/user/log-on"
