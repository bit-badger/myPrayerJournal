'use strict'

/* eslint-disable no-multi-spaces */
import Vue  from 'vue'
import Vuex from 'vuex'

import api  from '@/api'
import auth from '@/auth/AuthService'

import mutations from './mutation-types'
import actions   from './action-types'
/* eslint-enable no-multi-spaces */

Vue.use(Vuex)

/* eslint-disable no-console */
const logError = function (error) {
  if (error.response) {
    // The request was made and the server responded with a status code
    // that falls out of the range of 2xx
    console.error(error.response.data)
    console.error(error.response.status)
    console.error(error.response.headers)
  } else if (error.request) {
    // The request was made but no response was received
    // `error.request` is an instance of XMLHttpRequest in the browser and an instance of
    // http.ClientRequest in node.js
    console.error(error.request)
  } else {
    // Something happened in setting up the request that triggered an Error
    console.error('Error', error.message)
  }
  console.error(`config: ${error.config}`)
}

/**
 * Set the "Bearer" authorization header with the current access token
 */
const setBearer = async function () {
  try {
    await auth.getAccessToken()
    api.setBearer(auth.session.id.token)
  } catch (err) {
    if (err === 'Not logged in') {
      console.warn('API request attempted when user was not logged in')
    } else {
      console.error(err)
    }
  }
}
/* eslint-enable no-console */

export default new Vuex.Store({
  state: {
    user: auth.session.profile,
    isAuthenticated: auth.isAuthenticated(),
    journal: {},
    isLoadingJournal: false
  },
  mutations: {
    [mutations.LOADING_JOURNAL] (state, flag) {
      state.isLoadingJournal = flag
    },
    [mutations.LOADED_JOURNAL] (state, journal) {
      state.journal = journal
    },
    [mutations.REQUEST_ADDED] (state, newRequest) {
      state.journal.push(newRequest)
    },
    [mutations.REQUEST_UPDATED] (state, request) {
      let jrnl = state.journal.filter(it => it.requestId !== request.requestId)
      if (request.lastStatus !== 'Answered') jrnl.push(request)
      state.journal = jrnl
    },
    [mutations.SET_AUTHENTICATION] (state, value) {
      state.isAuthenticated = value
    },
    [mutations.USER_LOGGED_OFF] (state) {
      state.user = {}
      api.removeBearer()
      state.isAuthenticated = false
    },
    [mutations.USER_LOGGED_ON] (state, user) {
      state.user = user
      state.isAuthenticated = true
    }
  },
  actions: {
    async [actions.ADD_REQUEST] ({ commit }, { progress, requestText, recurType, recurCount }) {
      progress.$emit('show', 'indeterminate')
      try {
        await setBearer()
        const newRequest = await api.addRequest(requestText, recurType, recurCount)
        commit(mutations.REQUEST_ADDED, newRequest.data)
        progress.$emit('done')
      } catch (err) {
        logError(err)
        progress.$emit('done')
      }
    },
    async [actions.CHECK_AUTHENTICATION] ({ commit }) {
      try {
        await auth.getAccessToken()
        commit(mutations.SET_AUTHENTICATION, auth.isAuthenticated())
      } catch (_) {
        commit(mutations.SET_AUTHENTICATION, false)
      }
    },
    async [actions.LOAD_JOURNAL] ({ commit }, progress) {
      commit(mutations.LOADED_JOURNAL, {})
      progress.$emit('show', 'query')
      commit(mutations.LOADING_JOURNAL, true)
      await setBearer()
      try {
        const jrnl = await api.journal()
        commit(mutations.LOADED_JOURNAL, jrnl.data)
        progress.$emit('done')
      } catch (err) {
        logError(err)
        progress.$emit('done')
      } finally {
        commit(mutations.LOADING_JOURNAL, false)
      }
    },
    async [actions.UPDATE_REQUEST] ({ commit, state }, { progress, requestId, status, updateText, recurType, recurCount }) {
      progress.$emit('show', 'indeterminate')
      try {
        await setBearer()
        let oldReq = (state.journal.filter(req => req.requestId === requestId) || [])[0] || {}
        if (!(status === 'Prayed' && updateText === '')) {
          if (status !== 'Answered' && (oldReq.recurType !== recurType || oldReq.recurCount !== recurCount)) {
            await api.updateRecurrence(requestId, recurType, recurCount)
          }
        }
        if (status !== 'Updated' || oldReq.text !== updateText) {
          await api.updateRequest(requestId, status, oldReq.text !== updateText ? updateText : '')
        }
        const request = await api.getRequest(requestId)
        commit(mutations.REQUEST_UPDATED, request.data)
        progress.$emit('done')
      } catch (err) {
        logError(err)
        progress.$emit('done')
      }
    },
    async [actions.SHOW_REQUEST_NOW] ({ commit }, { progress, requestId, showAfter }) {
      progress.$emit('show', 'indeterminate')
      try {
        await setBearer()
        await api.showRequest(requestId, showAfter)
        const request = await api.getRequest(requestId)
        commit(mutations.REQUEST_UPDATED, request.data)
        progress.$emit('done')
      } catch (err) {
        logError(err)
        progress.$emit('done')
      }
    },
    async [actions.SNOOZE_REQUEST] ({ commit }, { progress, requestId, until }) {
      progress.$emit('show', 'indeterminate')
      try {
        await setBearer()
        await api.snoozeRequest(requestId, until)
        const request = await api.getRequest(requestId)
        commit(mutations.REQUEST_UPDATED, request.data)
        progress.$emit('done')
      } catch (err) {
        logError(err)
        progress.$emit('done')
      }
    }
  },
  getters: {},
  modules: {}
})
