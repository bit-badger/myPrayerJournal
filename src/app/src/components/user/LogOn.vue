<template>
  <p>hang tight...</p>
</template>

<script>
import AuthService from '@/auth/AuthService'

export default {
  name: 'log-on',
  data () {
    new AuthService().handleAuthentication(this.$store, this.$router)
    return {}
  }
}
</script>
