import {
  createRouter,
  createWebHistory,
  NavigationGuardNext,
  RouteLocationNormalized,
  RouteRecordRaw
} from "vue-router"

import auth from "@/auth"
import store, { Mutations } from "@/store"
import Home from "@/views/Home.vue"

const routes: Array<RouteRecordRaw> = [
  {
    path: "/",
    name: "Home",
    component: Home,
    meta: { title: "Welcome!" }
  },
  {
    path: "/journal",
    name: "Journal",
    component: () => import(/* webpackChunkName: "journal" */ "@/views/Journal.vue"),
    meta: { auth: true, title: "Loading Prayer Journal..." }
  },
  {
    path: "/legal/privacy-policy",
    name: "PrivacyPolicy",
    component: () => import(/* webpackChunkName: "legal" */ "@/views/legal/PrivacyPolicy.vue"),
    meta: { title: "Privacy Policy" }
  },
  {
    path: "/legal/terms-of-service",
    name: "TermsOfService",
    component: () => import(/* webpackChunkName: "legal" */ "@/views/legal/TermsOfService.vue"),
    meta: { title: "Terms of Service" }
  },
  {
    path: "/request/:id/edit",
    name: "EditRequest",
    component: () => import(/* webpackChunkName: "edit" */ "@/views/request/EditRequest.vue"),
    meta: { auth: true, title: "Edit Prayer Request" }
  },
  {
    path: "/request/:id/full",
    name: "FullRequest",
    component: () => import(/* webpackChunkName: "full" */ "@/views/request/FullRequest.vue"),
    meta: { auth: true, title: "View Full Prayer Request" }
  },
  {
    path: "/requests/active",
    name: "ActiveRequests",
    component: () => import(/* webpackChunkName: "list" */ "@/views/request/ActiveRequests.vue"),
    meta: { auth: true, title: "All Active Requests" }
  },
  {
    path: "/requests/answered",
    name: "AnsweredRequests",
    component: () => import(/* webpackChunkName: "list" */ "@/views/request/AnsweredRequests.vue"),
    meta: { auth: true, title: "Answered Requests" }
  },
  {
    path: "/requests/snoozed",
    name: "SnoozedRequests",
    component: () => import(/* webpackChunkName: "list" */ "@/views/request/SnoozedRequests.vue"),
    meta: { auth: true, title: "Snoozed Requests" }
  },
  {
    path: "/user/log-on",
    name: "LogOn",
    component: () => import(/* webpackChunkName: "logon" */ "@/views/user/LogOn.vue"),
    meta: { title: "Logging On..." }
  }
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  scrollBehavior: (to : RouteLocationNormalized, from : RouteLocationNormalized, savedPosition : any) => {
    return savedPosition ?? { x: 0, y: 0 }
  },
  routes
})

router.beforeEach((to : RouteLocationNormalized, from : RouteLocationNormalized, next : NavigationGuardNext) => {
  // Check for routes requiring authentication
  if (!store.state.isAuthenticated && (to.meta.auth || false)) {
    return auth.login({ target: to.path })
  }
  store.commit(Mutations.SetTitle, to.meta.title ?? "")
  return next()
})

export default router
