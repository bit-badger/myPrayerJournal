<template lang="pug">
main.container
  p Logging you on...
</template>

<script setup lang="ts">
import { inject, onBeforeMount } from "vue"
import { useRouter } from "vue-router"

import { AuthService } from "@/auth"
import { useStore } from "@/store"
import { AuthSymbol } from "@/App.vue"

const store = useStore()
const router = useRouter()

/** Auth service instance */
const auth = inject(AuthSymbol) as AuthService

/** Navigate on auth completion */
auth.on("loginEvent", (data: any) => {
  router.push(data.state.target ?? "/journal")
})

// this.progress.$emit('show', 'indeterminate')
onBeforeMount(async () => { await auth.handleAuthentication(store) })
</script>
