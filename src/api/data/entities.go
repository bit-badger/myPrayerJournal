package data

// History is a record of action taken on a prayer request, including updates to its text.
type History struct {
	RequestID string `json:"requestId"`
	AsOf      int64  `json:"asOf"`
	Status    string `json:"status"`
	Text      string `json:"text"`
}

// Note is a note regarding a prayer request that does not result in an update to its text.
type Note struct {
	RequestID string `json:"requestId"`
	AsOf      int64  `json:"asOf"`
	Notes     string `json:"notes"`
}

// Request is the identifying record for a prayer request.
type Request struct {
	ID        string `json:"requestId"`
	EnteredOn int64  `json:"enteredOn"`
	UserID    string `json:"userId"`
}

// JournalRequest is the form of a prayer request returned for the request journal display. It also contains
// properties that may be filled for history and notes.
type JournalRequest struct {
	RequestID  string    `json:"requestId"`
	UserID     string    `json:"userId"`
	Text       string    `json:"text"`
	AsOf       int64     `json:"asOf"`
	LastStatus string    `json:"lastStatus"`
	History    []History `json:"history"`
	Notes      []Note    `json:"notes"`
}
