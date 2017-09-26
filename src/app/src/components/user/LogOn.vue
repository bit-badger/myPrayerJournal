<template lang="pug">
article
  pageTitle(title='Logging On')
  p Logging you on...
</template>

<script>
'use strict'

import PageTitle from '../common/PageTitle'
import AuthService from '@/auth/AuthService'

export default {
  name: 'log-on',
  components: {
    PageTitle
  },
  created () {
    this.$Progress.start()
    new AuthService().handleAuthentication(this.$store, this.$router)
    // Auth service redirects to dashboard, which restarts the progress bar
  }
}
</script>
