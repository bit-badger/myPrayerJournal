import { InjectionKey } from "vue"
import { createStore, Store, useStore as baseUseStore } from "vuex"
import { useTitle } from "@vueuse/core"

import api, { JournalRequest } from "@/api"
import auth from "@/auth"

import * as Actions from "./actions"
import * as Mutations from "./mutations"

/** The state of myPrayerJournal */
export interface State {
  pageTitle : string,
  /** The user's profile */
  user : any,
  /** Whether there is a user signed in */
  isAuthenticated : boolean,
  /** The current set of prayer requests */
  journal : JournalRequest[],
  /** Whether the journal is currently being loaded */
  isLoadingJournal : boolean
}

/** An injection key to identify this state with Vue */
export const key : InjectionKey<Store<State>> = Symbol("VueX Store")

/** Use this store in component `setup` functions */
export function useStore () : Store<State> {
  return baseUseStore(key)
}

/**
 * Set the "Bearer" authorization header with the current access token
 */
const setBearer = async function () {
  try {
    await auth.getAccessToken()
    api.setBearer(auth.session.id.token)
  } catch (err : any) {
    if (err.message === "Not logged in") {
      console.warn("API request attempted when user was not logged in")
    } else {
      console.error(err)
    }
  }
}

/** The name of the application */
const appName = "myPrayerJournal"

/**
 * Get the sort value for a prayer request
 * @param it The prayer request
 */
const sortValue = (it : JournalRequest) => it.showAfter === 0 ? it.asOf : it.showAfter

/**
 * Sort journal requests either by asOf or showAfter
 */
const journalSort = (a : JournalRequest, b : JournalRequest) => sortValue(a) - sortValue(b)
 
export default createStore({
  state: () : State => ({
    pageTitle: appName,
    user: auth.session.profile,
    isAuthenticated: auth.isAuthenticated(),
    journal: [],
    isLoadingJournal: false
  }),
  mutations: {
    [Mutations.LoadingJournal] (state, flag : boolean) {
      state.isLoadingJournal = flag
    },
    [Mutations.LoadedJournal] (state, journal : JournalRequest[]) {
      state.journal = journal.sort(journalSort)
    },
    [Mutations.RequestAdded] (state, newRequest : JournalRequest) {
      state.journal.push(newRequest)
    },
    [Mutations.RequestUpdated] (state, request : JournalRequest) {
      const jrnl = state.journal.filter(it => it.requestId !== request.requestId)
      if (request.lastStatus !== "Answered") jrnl.push(request)
      state.journal = jrnl
    },
    [Mutations.SetAuthentication] (state, value : boolean) {
      state.isAuthenticated = value
    },
    [Mutations.SetTitle]: (state, title : string) => {
      state.pageTitle = title === "" ? appName : `${title} | ${appName}`
      useTitle(state.pageTitle)
    },
    [Mutations.UserLoggedOff] (state) {
      state.user = {}
      api.removeBearer()
      state.isAuthenticated = false
    },
    [Mutations.UserLoggedOn] (state, user) {
      state.user = user
      state.isAuthenticated = true
    }
  },
  actions: {
    // async [Actions.AddRequest] ({ commit }, p : AddRequestAction) {
    //   const { progress, requestText, recurType, recurCount } = p
    //   progress.events.$emit('show', 'indeterminate')
    //   try {
    //     await setBearer()
    //     const newRequest = await api.addRequest(requestText, recurType, recurCount)
    //     commit(Mutations.RequestAdded, newRequest.data)
    //   } catch (err) {
    //     logError(err)
    //   } finally {
    //     progress.events.$emit("done")
    //   }
    // },
    async [Actions.CheckAuthentication] ({ commit }) {
      try {
        await auth.getAccessToken()
        commit(Mutations.SetAuthentication, auth.isAuthenticated())
      } catch (_) {
        commit(Mutations.SetAuthentication, false)
      }
    }
    // ,
    // async [Actions.LoadJournal] ({ commit }, progress : ProgressProps) {
    //   commit(Mutations.LoadedJournal, [])
    //   progress.events.$emit("show", "query")
    //   commit(Mutations.LoadingJournal, true)
    //   await setBearer()
    //   try {
    //     const jrnl = await api.journal()
    //     commit(Mutations.LoadedJournal, jrnl.data)
    //   } catch (err) {
    //     logError(err)
    //   } finally {
    //     progress.events.$emit("done")
    //     commit(Mutations.LoadingJournal, false)
    //   }
    // },
    // async [Actions.UpdateRequest] ({ commit, state }, p : UpdateRequestAction) {
    //   const { progress, requestId, status, updateText, recurType, recurCount } = p
    //   progress.events.$emit("show", "indeterminate")
    //   try {
    //     await setBearer()
    //     const oldReq = (state.journal.filter(req => req.requestId === requestId) || [])[0] || {}
    //     if (!(status === "Prayed" && updateText === "")) {
    //       if (status !== "Answered" && (oldReq.recurType !== recurType || oldReq.recurCount !== recurCount)) {
    //         await api.updateRecurrence(requestId, recurType, recurCount)
    //       }
    //     }
    //     if (status !== "Updated" || oldReq.text !== updateText) {
    //       await api.updateRequest(requestId, status, oldReq.text !== updateText ? updateText : "")
    //     }
    //     const request = await api.getRequest(requestId)
    //     commit(Mutations.RequestUpdated, request.data)
    //   } catch (err) {
    //     logError(err)
    //   } finally {
    //     progress.events.$emit('done')
    //   }
    // },
    // async [Actions.ShowRequestNow] ({ commit }, p : ShowRequestAction) {
    //   const { progress, requestId, showAfter } = p
    //   progress.events.$emit("show", "indeterminate")
    //   try {
    //     await setBearer()
    //     await api.showRequest(requestId, showAfter)
    //     const request = await api.getRequest(requestId)
    //     commit(Mutations.RequestUpdated, request.data)
    //   } catch (err) {
    //     logError(err)
    //   } finally {
    //     progress.events.$emit('done')
    //   }
    // },
    // async [Actions.SnoozeRequest] ({ commit }, p : SnoozeRequestAction) {
    //   const { progress, requestId, until } = p
    //   progress.events.$emit("show", "indeterminate")
    //   try {
    //     await setBearer()
    //     await api.snoozeRequest(requestId, until)
    //     const request = await api.getRequest(requestId)
    //     commit(Mutations.RequestUpdated, request.data)
    //   } catch (err) {
    //     logError(err)
    //   } finally {
    //     progress.events.$emit("done")
    //   }
    // }
  }
})

export * as Actions from "./actions"
export * as Mutations from "./mutations"
