# Documentation

## Caveats

_myPrayerJournal is currently alpha software. There likely will be errors, the way things work may change, and parts of the application are unfinished or need polish. I **will** do my best to not lose any data, though; it is backed up the way other Bit Badger Solutions sites have their data backed up. Throughout this document, current gotchas will be called out with italic text, like this notice._

## Finding the Site

The application is at <https://prayerjournal.me>.

## Signing Up

myPrayerJournal uses login services using Google or Microsoft accounts. The only information the application stores in its database is your user Id token it receives from these services, so there are no permissions you should have to accept from these provider other than establishing that you can log on with that account. Because of this, you'll want to pick the same one each time; the tokens between the two accounts are different, even if you use the same e-mail address to log on to both.

## Your Prayer Journal

Your current requests will be presented in three columns (or one, if you're using a mobile phone). Each request is in its own card, and the buttons at the bottom of each card apply to that request. The last line of each request also tells you how long it has been since anything has been done on that request. Any time you see something like "a few minutes ago," you can hover over that to see the actual date/time the action was taken.

## Adding a Request

To add a request, click the "Add a New Request" button at the top of your journal. Then, enter the text of the request as you see fit; there is no right or wrong way, and you are the only person who will see the text you enter. When you save the request, it will go to the bottom of the list of requests.

## Praying for Requests

The first button for each request has a checkmark icon; clicking this button will mark the request as "Prayed" and move it to the bottom of the list. This allows you, if you're praying through your requests, to start at the top left (with the request that it's been the longest since you've prayed) and click the button as you pray; when the request goes to the bottom of the list, the next-least-recently-prayed request will take the top spot.

## Editing Requests

The second button for each request has a pencil icon. This allows you to edit the text of the request, pretty much the same way you entered it; it starts with the current text, and you can add to it, modify it, or completely replace it. By default, updates will go in with an "Updated" status; you have the option to also mark this update as "Prayed" or "Answered." Answered requests will drop off the journal list.

## Viewing a Request and Its History

myPrayerJournal tracks all of the actions related to a request; the fourth button, with the magnifying glass icon, will show you the entire history, including the text as it changed, and all the times "Prayed" was recorded.

## Answered Requests

Next to "Journal" on the top navigation is the word "Answered." This page lists all answered requests, from most recent to least recent, along with the text of the request at the time it was marked as answered. It will also show you when it was marked answered. The button with the magnifying class at the words "Show Full Request" behave the same way as the paragraph immediately preceding this describes. _(This will likely change before a 0.9.x release, but this gives at least some way to find and review answered requests.)_

## Known Issues

See [the GitHub issues list](https://github.com/danieljsummers/myPrayerJournal/issues) for the most up-to-date list.
