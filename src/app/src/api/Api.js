import axios from 'axios'

const http = axios.create({
  baseURL: 'http://localhost:8084'
})

export default {
  something: http.get('/blah')
}
