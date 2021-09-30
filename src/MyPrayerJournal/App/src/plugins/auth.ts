import { App, InjectionKey } from "vue"
import authService, { AuthService } from "@/auth"

/** The symbol to use for dependency injection */
export const key : InjectionKey<AuthService> = Symbol("Auth service")

/** The auth service instance */
const service = new AuthService()

export default {
  install (app : App) {
    Object.defineProperty(app, "authService", { get: () => service })

    app.provide(key, service)
    
    app.mixin({
      created () {
        if (this.handleLoginEvent) {
          authService.addListener("loginEvent", this.handleLoginEvent)
        }
      },
      destroyed () {
        if (this.handleLoginEvent) {
          authService.removeListener("loginEvent", this.handleLoginEvent)
        }
      }
    })
  }
}

/** Use the auth service */
export function useAuth () : AuthService {
  return service
}
