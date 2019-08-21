<template lang="pug">
article.mpj-main-content(role='main')
  pageTitle(title='Logging On')
  p Logging you on...
</template>

<script>
'use strict'

import AuthService from '@/auth/AuthService'

export default {
  name: 'log-on',
  inject: ['progress'],
  created () {
    this.progress.$emit('show', 'indeterminate')
    new AuthService().handleAuthentication(this.$store, this.$router)
    // Auth service redirects to dashboard, which restarts the progress bar
  }
}
</script>
