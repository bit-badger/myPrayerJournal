# Documentation

## About myPrayerJournal

Journaling has a long history; it helps people remember what happened, and the act of writing helps people think about what happened and process it. A prayer journal is not a new concept; it helps you keep track of the requests for which you've prayed, you can use it to pray over things repeatedly, and you can write the result when the answer comes _(or it was "no")_.

myPrayerJournal was borne of out of a personal desire [Daniel](https://github.com/danieljsummers) had to have something that would help him with his prayer life. When it's time to pray, it's not really time to use an app, so the design goal here is to keep it simple and unobtrusive. It will also help eliminate some of the downsides to a paper prayer journal, like not remembering whether you've prayed for a request, or running out of room to write another update on one.

## Finding the Site

The application is at <https://prayerjournal.me>.

## Signing Up

myPrayerJournal uses login services using Google or Microsoft accounts. The only information the application stores in its database is your user Id token it receives from these services, so there are no permissions you should have to accept from these provider other than establishing that you can log on with that account. Because of this, you'll want to pick the same one each time; the tokens between the two accounts are different, even if you use the same e-mail address to log on to both.

## Your Prayer Journal

Your current requests will be presented in columns (usually three, but it could be more or less, depending on the size of your screen or device). Each request is in its own card, and the buttons at the top of each card apply to that request. The last line of each request also tells you how long it has been since anything has been done on that request. Any time you see something like "a few minutes ago," you can hover over that to see the actual date/time the action was taken.

## Adding a Request

To add a request, click the "Add a New Request" button at the top of your journal. Then, enter the text of the request as you see fit; there is no right or wrong way, and you are the only person who will see the text you enter. When you save the request, it will go to the bottom of the list of requests.

## Setting Request Recurrence

When you add or update a request, you can choose whether requests go to the bottom of the journal once they have been marked "Prayed" or whether they will reappear after a delay. You can set recurrence in terms of hours, days, or weeks, but it cannot be longer than 365 days. If you decide you want a request to reappear sooner, you can skip the current delay; click the "Active" menu link, find the request in the list (likely near the bottom), and click the "Show Now" button.

## Praying for Requests

The first button for each request has a checkmark icon; clicking this button will mark the request as "Prayed" and move it to the bottom of the list (or off, if you've set a recurrence period for the request). This allows you, if you're praying through your requests, to start at the top left (with the request that it's been the longest since you've prayed) and click the button as you pray; when the request move below or away, the next-least-recently-prayed request will take the top spot.

## Editing Requests

The second button for each request has a pencil icon. This allows you to edit the text of the request, pretty much the same way you entered it; it starts with the current text, and you can add to it, modify it, or completely replace it. By default, updates will go in with an "Updated" status; you have the option to also mark this update as "Prayed" or "Answered." Answered requests will drop off the journal list.

## Adding Notes

The third button for each request has an icon that looks like a speech bubble with lines on it; this lets you record notes about the request. If there is something you want to record that doesn't change the text of the request, this is the place to do it. For example, you may be praying for a long-term health issue, and that person tells you that their status is the same; or, you may want to record something God said to you while you were praying for that request.

## Snoozing Requests

There may be a time where a request does not need to appear. The fourth button, with the clock icon, allows you to snooze requests until the day you specify. Additionally, if you have any snoozed requests, a "Snoozed" menu item will appear next to the "Journal" one; this page allows you to see what requests are snoozed, and return them to your journal by canceling the snooze.

## Viewing a Request and Its History

myPrayerJournal tracks all of the actions related to a request; from the "Active" and "Answered" menu links (and "Snoozed", if it's showing), there is a "View Full Request" button. That page will show the current text of the request; how many times it has been marked as prayed; how long it has been an active request; and a log of all updates, prayers, and notes you have recorded. That log is listed from most recent to least recent; if you want to read it chronologically, just press the "End" key on your keyboard and read it from the bottom up.

The "Active" link will show all requests that have not yet been marked answered, including snoozed and recurring requests. If requests are snoozed, or in a recurrence period off the journal, there will be a button where you can return the request to the list (either "Cancel Snooze" or "Show Now"). The "Answered" link shows all requests that have been marked answered. The "Snoozed" link just shows snoozed requests.

## Final Notes

- myPrayerJournal is nearing the end of its public beta, approaching its first official release. If you encounter errors, please [file an issue on GitHub](https://github.com/bit-badger/myPrayerJournal/issues) with as much detail as possible. You can also browse the list of issues to see what has been done and what is still left to do.
- Prayer requests and their history are securely backed up nightly along with other Bit Badger Solutions data.
- Prayer changes things - most of all, the one doing the praying. I pray that this tool enables you to deepen and strengthen your prayer life.
