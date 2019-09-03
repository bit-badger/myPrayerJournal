<template lang="pug">
md-content(role='main').mpj-main-content
  page-title(title='Terms of Service'
             hide-on-page=true)
  md-card
    md-card-header
      .md-title Terms of Service
      .md-subhead as of May 21, 2018
    md-card-content.mpj-full-page-card
      h3 1. Acceptance of Terms
      p.
        By accessing this web site, you are agreeing to be bound by these Terms and Conditions, and that you are
        responsible to ensure that your use of this site complies with all applicable laws. Your continued use of this
        site implies your acceptance of these terms.
      h3 2. Description of Service and Registration
      p.
        myPrayerJournal is a service that allows individuals to enter and amend their prayer requests. It requires no
        registration by itself, but access is granted based on a successful login with an external identity provider.
        See #[router-link(:to="{ name: 'PrivacyPolicy' }") our privacy policy] for details on how that information is
        accessed and stored.
      h3 3. Third Party Services
      p.
        This service utilizes a third-party service provider for identity management. Review the terms of service for
        #[a(href='https://auth0.com/terms' target='_blank') Auth0], as well as those for the selected authorization
        provider (#[a(href='https://www.microsoft.com/en-us/servicesagreement' target='_blank') Microsoft] or
        #[a(href='https://policies.google.com/terms' target='_blank') Google]).
      h3 4. Liability
      p.
        This service is provided "as is", and no warranty (express or implied) exists. The service and its developers
        may not be held liable for any damages that may arise through the use of this service.
      h3 5. Updates to Terms
      p.
        These terms and conditions may be updated at any time, and this service does not have the capability to notify
        users when these change. The date at the top of the page will be updated when any of the text of these terms is
        updated.
      hr
      p.
        You may also wish to review our #[router-link(:to="{ name: 'PrivacyPolicy' }") privacy policy] to learn how we
        handle your data.
</template>
