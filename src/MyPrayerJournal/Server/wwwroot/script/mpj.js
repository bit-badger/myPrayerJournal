"use strict"

/** myPrayerJournal script */
const mpj = {
  /** Auth0 configuration */
  auth: {
    /** The Auth0 client */
    auth0: null,
    /** Configure the Auth0 client */
    async configureClient () {
      const response = await fetch("/auth-config.json")
      const config = await response.json()
      mpj.auth.auth0 = await createAuth0Client({
        domain: config.domain,
        client_id: config.clientId,
        audience: config.audience
      })
    }
  },
  /** Whether the user is currently authenticated */
  isAuthenticated: false,
  /** Whether we should redirect to the journal the next time the menu items are refreshed */
  redirToJournal: false,
  /**
   * Process a log on request
   * @param {Event} e The HTML event from the `onclick` event
   */
  async logOn (e) {
    e.preventDefault()
    await mpj.auth.auth0.loginWithRedirect({ redirect_uri: `${window.location.origin}/user/log-on` })
  },
  /**
   * Log the user off
   * @param {Event} e The HTML event from the `onclick` event
   */
  logOff (e) {
    e.preventDefault()
    mpj.auth.auth0.logout({ returnTo: window.location.origin })
  },
  /**
   * Show a message via toast
   * @param {string} message The message to show
   */
  showToast (message) {
    const [level, msg] = message.split("|||")
    
    let header
    if (level !== "success") {
      const heading = typ => `<span class="me-auto"><strong>${typ.toUpperCase()}</strong></span>`
      
      header = document.createElement("div")
      header.className = "toast-header"
      header.innerHTML = heading(level === "warning" ? level : "error")
      
      const close = document.createElement("button")
      close.type = "button"
      close.className = "btn-close"
      close.setAttribute("data-bs-dismiss", "toast")
      close.setAttribute("aria-label", "Close")
      header.appendChild(close)
    }

    const body = document.createElement("div")
    body.className = "toast-body"
    body.innerText = msg
    
    const toastEl = document.createElement("div")
    toastEl.className = `toast bg-${level} text-white`
    toastEl.setAttribute("role", "alert")
    toastEl.setAttribute("aria-live", "assertlive")
    toastEl.setAttribute("aria-atomic", "true")
    toastEl.addEventListener("hidden.bs.toast", e => e.target.remove())
    if (header) toastEl.appendChild(header)
    
    toastEl.appendChild(body)
    document.getElementById("toasts").appendChild(toastEl)
    new bootstrap.Toast(toastEl, { autohide: level === "success" }).show()
  },
  /** Script for the request edit component */
  edit: {
    /**
     * Toggle the recurrence input fields
     * @param {Event} e The click event
     */
    toggleRecurrence ({ target }) {
      const isDisabled = target.value === "Immediate"
      ;["recurCount","recurInterval"].forEach(it => document.getElementById(it).disabled = isDisabled)
    }
  },
}

window.onload = async () => {
  /** If the user is authenticated, set the JWT on the `body` tag */
  const establishAuth = async () => {
    mpj.isAuthenticated = await mpj.auth.auth0.isAuthenticated()
    if (mpj.isAuthenticated) {
      const token = await mpj.auth.auth0.getTokenSilently()
      const user = await mpj.auth.auth0.getUser()
      document.querySelector("body")
        .setAttribute("hx-headers", `{ "Authorization": "Bearer ${token}", "X-Given-Name": "${user.given_name}" }`)
      htmx.trigger(htmx.find(".navbar-nav"), "menu-refresh")
    }  
  }

  // Set up Auth0
  await mpj.auth.configureClient()
  await establishAuth()
  if (mpj.isAuthenticated) return

  // Handle log on code, if present
  const query = window.location.search
  if (query.includes("code=") && query.includes("state=")) {
    await mpj.auth.auth0.handleRedirectCallback()
    await establishAuth()
    if (window.location.pathname === "/user/log-on") {
      mpj.redirToJournal = true
    } else {
      window.history.replaceState({}, document.title, "/")
    }
  }
}

htmx.on("htmx:afterOnLoad", function (evt) {
  const hdrs = evt.detail.xhr.getAllResponseHeaders()
  // Set the page title if a header was in the response
  if (hdrs.indexOf("x-page-title") >= 0) {
    const title = document.querySelector("title")
    title.innerText = evt.detail.xhr.getResponseHeader("x-page-title")
    title.innerHTML += " &#xab; myPrayerJournal"
  }
  // Show a message if there was one in the response
  if (hdrs.indexOf("x-toast") >= 0) {
    mpj.showToast(evt.detail.xhr.getResponseHeader("x-toast"))
  }
})
htmx.on("htmx:afterSettle", function (evt) {
  // Redirect to the journal (once menu items load after log on)
  if (mpj.redirToJournal 
      && ([...evt.target.attributes].find(it => it.name === "hx-target")?.value ?? "") === ".navbar-nav") {
    mpj.redirToJournal = false
    document.querySelector(`a[href="/journal"]`).click()
  }
})
