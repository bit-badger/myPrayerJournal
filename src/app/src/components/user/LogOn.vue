<template>
  <p>hang tight...</p>
</template>

<script>
  export default {
    name: 'log-on',
    props: ['auth'],
    data () {
      this.auth.handleAuthentication()
      return {}
    }
  }
</script>
