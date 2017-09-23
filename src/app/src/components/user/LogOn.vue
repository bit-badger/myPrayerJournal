<template lang="pug">
p Logging you on...
</template>

<script>
'use strict'

import AuthService from '@/auth/AuthService'

export default {
  name: 'log-on',
  created () {
    this.$Progress.start()
    new AuthService().handleAuthentication(this.$store, this.$router)
    // Auth service redirects to dashboard, which restarts the progress bar
  }
}
</script>
