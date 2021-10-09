"use strict"

/** myPrayerJournal script */
const mpj = {
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

htmx.on("htmx:afterOnLoad", function (evt) {
  const hdrs = evt.detail.xhr.getAllResponseHeaders()
  // Show a message if there was one in the response
  if (hdrs.indexOf("x-toast") >= 0) {
    mpj.showToast(evt.detail.xhr.getResponseHeader("x-toast"))
  }
})
