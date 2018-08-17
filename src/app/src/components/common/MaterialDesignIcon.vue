<template lang="pug">
i.material-icons(v-html='icon')
</template>

<script>
export default {
  name: 'md-icon',
  props: {
    icon: {
      type: String,
      required: true
    }
  }
}
</script>
