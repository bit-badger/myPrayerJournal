<template lang="pug">
article
  page-title(title="Welcome!" hideOnPage="true")
  p &nbsp;
  p.
    myPrayerJournal is a place where individuals can record their prayer requests, record that they prayed for them,
    update them as God moves in the situation, and record a final answer received on that request.&nbsp; It will also
    allow individuals to review their answered prayers.
  p.
    This site is currently in very limited alpha, as it is being developed with a core group of test users.&nbsp; If
    this is something in which you are interested, check back around mid-November 2017 for an update on the
    development progress.
</template>

<script>
'use strict'

import PageTitle from './PageTitle.vue'

export default {
  name: 'home',
  components: {
    PageTitle
  }
}
</script>
