<template lang="pug">
article.mpj-main-content(role='main')
  pageTitle(title='Logging On')
  p Logging you on...
</template>

<script lang="ts">
import { createComponent, onBeforeMount } from '@vue/composition-api'
import VueRouter from 'vue-router' // eslint-disable-line no-unused-vars
import { Store } from 'vuex' // eslint-disable-line no-unused-vars

import { AppState } from '../../store/types' // eslint-disable-line no-unused-vars
import { AuthService } from '../../auth' // eslint-disable-line no-unused-vars

import { useAuth } from '../../plugins/auth'
import { useRouter } from '../../plugins/router'
import { useStore } from '../../plugins/store'

export default createComponent({
  setup () {
    /** Auth service instance */
    const auth = useAuth() as AuthService

    /** Store instance */
    const store = useStore() as Store<AppState>

    /** Router instance */
    const router = useRouter() as VueRouter

    /** Navigate on auth completion */
    auth.on('loginEvent', (data: any) => {
      router.push(data.state.target || '/journal')
    })

    // this.progress.$emit('show', 'indeterminate')
    onBeforeMount(async () => { await auth.handleAuthentication(store) })

    return { }
  }
})
</script>
