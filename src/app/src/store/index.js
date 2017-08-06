import Vue from 'vue'
import Vuex from 'vuex'

import AuthService from '@/auth/AuthService'

import * as types from './mutation-types'

Vue.use(Vuex)

this.auth0 = new AuthService()

export default new Vuex.Store({
  state: {
    user: JSON.parse(localStorage.getItem('user_profile') || '{}'),
    isAuthenticated: this.auth0.isAuthenticated()
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
    }
  },
  actions: {},
  getters: {},
  modules: {}
})
