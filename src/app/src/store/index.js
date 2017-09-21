import Vue from 'vue'
import Vuex from 'vuex'

import api from '@/api'
import AuthService from '@/auth/AuthService'

import mutations from './mutation-types'
import actions from './action-types'

Vue.use(Vuex)

this.auth0 = new AuthService()

const logError = function (error) {
  if (error.response) {
    // The request was made and the server responded with a status code
    // that falls out of the range of 2xx
    console.log(error.response.data)
    console.log(error.response.status)
    console.log(error.response.headers)
  } else if (error.request) {
    // The request was made but no response was received
    // `error.request` is an instance of XMLHttpRequest in the browser and an instance of
    // http.ClientRequest in node.js
    console.log(error.request)
  } else {
    // Something happened in setting up the request that triggered an Error
    console.log('Error', error.message)
  }
  console.log(error.config)
}

export default new Vuex.Store({
  state: {
    user: JSON.parse(localStorage.getItem('user_profile') || '{}'),
    isAuthenticated: this.auth0.isAuthenticated(),
    journal: {},
    isLoadingJournal: false
  },
  mutations: {
    [mutations.USER_LOGGED_ON] (state, user) {
      localStorage.setItem('user_profile', JSON.stringify(user))
      state.user = user
      api.setBearer(localStorage.getItem('id_token'))
      state.isAuthenticated = true
    },
    [mutations.USER_LOGGED_OFF] (state) {
      state.user = {}
      api.removeBearer()
      state.isAuthenticated = false
    },
    [mutations.LOADING_JOURNAL] (state, flag) {
      state.isLoadingJournal = flag
    },
    [mutations.LOADED_JOURNAL] (state, journal) {
      state.journal = journal
    },
    [mutations.REQUEST_ADDED] (state, newRequest) {
      state.journal.unshift(newRequest)
    }
  },
  actions: {
    async [actions.LOAD_JOURNAL] ({ commit }) {
      commit(mutations.LOADED_JOURNAL, {})
      commit(mutations.LOADING_JOURNAL, true)
      api.setBearer(localStorage.getItem('id_token'))
      try {
        const jrnl = await api.journal()
        commit(mutations.LOADED_JOURNAL, jrnl.data)
      } catch (err) {
        logError(err)
      } finally {
        commit(mutations.LOADING_JOURNAL, false)
      }
    },
    async [actions.ADD_REQUEST] ({ commit }, requestText) {
      try {
        const newRequest = await api.addRequest(requestText)
        commit(mutations.REQUEST_ADDED, newRequest)
      } catch (err) {
        logError(err)
      }
    }
  },
  getters: {},
  modules: {}
})
