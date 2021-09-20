<template lang="pug">
md-content(role='main').mpj-main-content
  md-card
    md-card-header
      .md-title Privacy Policy
      .md-subhead as of May 21, 2018
    md-card-content.mpj-full-page-card
      p.
        The nature of the service is one where privacy is a must. The items below will help you understand the data we
        collect, access, and store on your behalf as you use this service.
      hr
      h3 Third Party Services
      p.
        myPrayerJournal utilizes a third-party authentication and identity provider. You should familiarize yourself
        with the privacy policy for #[a(href='https://auth0.com/privacy' target='_blank') Auth0], as well as your
        chosen provider (#[a(href='https://privacy.microsoft.com/en-us/privacystatement' target='_blank') Microsoft] or
        #[a(href='https://policies.google.com/privacy' target='_blank') Google]).
      hr
      h3 What We Collect
      h4 Identifying Data
      ul
        li.
          The only identifying data myPrayerJournal stores is the subscriber (&ldquo;sub&rdquo;) field from the token we
          receive from Auth0, once you have signed in through their hosted service. All information is associated with
          you via this field.
        li.
          While you are signed in, within your browser, the service has access to your first and last names, along with
          a URL to the profile picture (provided by your selected identity provider). This information is not
          transmitted to the server, and is removed when &ldquo;Log Off&rdquo; is clicked.
      h4 User Provided Data
      ul
        li.
          myPrayerJournal stores the information you provide, including the text of prayer requests, updates, and notes;
          and the date/time when certain actions are taken.
      hr
      h3 How Your Data Is Accessed / Secured
      ul
        li.
          Your provided data is returned to you, as required, to display your journal or your answered requests. On the
          server, it is stored in a controlled-access database.
        li.
          Your data is backed up, along with other Bit Badger Solutions hosted systems, in a rolling manner; backups are
          preserved for the prior 7 days, and backups from the 1st and 15th are preserved for 3 months. These backups
          are stored in a private cloud data repository.
        li.
          The data collected and stored is the absolute minimum necessary for the functionality of the service. There
          are no plans to &ldquo;monetize&rdquo; this service, and storing the minimum amount of information means that
          the data we have is not interesting to purchasers (or those who may have more nefarious purposes).
        li Access to servers and backups is strictly controlled and monitored for unauthorized access attempts.
      hr
      h3 Removing Your Data
      p.
        At any time, you may choose to discontinue using this service. Both Microsoft and Google provide ways to revoke
        access from this application. However, if you want your data removed from the database, please contact daniel at
        bitbadger.solutions (via e-mail, replacing at with @) prior to doing so, to ensure we can determine which
        subscriber ID belongs to you.
</template>
