import { App, inject, InjectionKey } from "vue"
import authService, { AuthService } from "@/auth"

/** The symbol to use for dependency injection */
const AuthSymbol : InjectionKey<AuthService> = Symbol("Auth service")

export default {
  install (app : App) {
    Object.defineProperty(app, "authService", { get: () => authService })

    app.provide(AuthSymbol, authService)
    
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
  return inject(AuthSymbol)!
}
