# Caveats

_myPrayerJournal is currently alpha software. There likely will be errors, the way things work may change, and parts of the application are unfinished or need polish. I **will** do my best to not lose any data, though; it is backed up the way other DJS Consulting sites have their data backed up. Throughout this document, current gotchas will be called out with italic text, like this notice._

# Finding the Site

The application is at <https://prayerjournal.me>.

# Signing Up

myPrayerJournal uses login services using Google or Microsoft accounts. The only information the application stores in its database is your user Id token it receives from these services, so there are no permissions you should have to accept from these provider other than establishing that you can log on with that account. Because of this, you'll want to pick the same one each time; the tokens between the two accounts are different, even if you use the same e-mail address to log on to both.

# Adding a Request

To add a request, click the "Add a New Request" button in your "Dashboard" _(which will probably get renamed to "Journal" before this goes from alpha to beta)_. Then, enter the text of the request as you see fit; there is no right or wrong way, and you are the only person who will see the text you enter. When you save the request, it will go to the bottom of the list of requests.

# Praying for Requests

The first button for each request has a checkmark icon; clicking this button will mark the request as "Prayed" and move it to the bottom of the list. This allows you, if you're praying through your requests, to start at the top (with the request that it's been the longest since you've prayed) and click the button as you pray; when the request goes to the bottom of the list, the next will move up to the top.

# Editing Requests

The second button for each request has a pencil icon. This allows you to edit the text of the request, pretty much the same way you entered it; it starts with the current text, and you can add to it, modify it, or completely replace it. By default, updates will go in with an "Updated" status; you have the option to also mark this update as "Prayed" or "Answered." Answered requests will drop off the journal list. _(There is currently no way to see answered requests once they have been answered; this functionality is planned soon.)_

# Viewing a Request and Its History

myPrayerJournal tracks all of the actions related to a request; the third button, with the document icon, will show you the entire history, including the text as it changed, and all the times "Prayed" was recorded.

# Known Issues

- _The "As Of" column does not update as time goes on; if you pray through your list, you may end up with all requests saying "a few seconds ago" (or, if your computer's time is off, it may say "a few seconds from now"). On the menu, if you click the title, then back to "Dashboard," it should reload the journal and display the times._
- _There is no way to view "Answered" requests; the absence of this functionality is a big reason this is still considered alpha. Going back through requests to see how God has answered them is an encouraging benefit of taking the time to journal._
- _If you try to do something an get an error notification instead of a green checkmark, try logging off and logging back on again. The site currently doesn't check to see if your session has expired, but the server with which it's communicating does._
