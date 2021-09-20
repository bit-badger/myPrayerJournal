<template lang="pug">
main.container
  p Logging you on...
</template>

<script setup lang="ts">
import { onBeforeMount } from "vue"
import { useRouter } from "vue-router"
import { useAuth } from "@/plugins/auth"
import { useStore } from "@/store"

const store = useStore()
const router = useRouter()

/** Auth service instance */
const auth = useAuth()

/** Navigate on auth completion */
auth.on("loginEvent", (data: any) => {
  router.push(data.state.target ?? "/journal")
})

// this.progress.$emit('show', 'indeterminate')
onBeforeMount(async () => { await auth.handleAuthentication(store) })
</script>
