import Vue from 'vue'
import Vuex from 'vuex'

import api from '@/api'
import AuthService from '@/auth/AuthService'

import * as types from './mutation-types'
import * as actions from './action-types'

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
    [types.USER_LOGGED_ON] (state, user) {
      localStorage.setItem('user_profile', JSON.stringify(user))
      state.user = user
      state.isAuthenticated = true
    },
    [types.USER_LOGGED_OFF] (state) {
      state.user = {}
      state.isAuthenticated = false
    },
    [types.LOADING_JOURNAL] (state, flag) {
      state.isLoadingJournal = flag
    },
    [types.LOADED_JOURNAL] (state, journal) {
      state.journal = journal
    }
  },
  actions: {
    [actions.LOAD_JOURNAL] ({ commit }) {
      commit(types.LOADED_JOURNAL, {})
      commit(types.LOADING_JOURNAL, true)
      api.journal()
        .then(jrnl => {
          commit(types.LOADING_JOURNAL, false)
          commit(types.LOADED_JOURNAL, jrnl)
        })
        .catch(err => {
          commit(types.LOADING_JOURNAL, false)
          logError(err)
        })
    }
  },
  getters: {},
  modules: {}
})
