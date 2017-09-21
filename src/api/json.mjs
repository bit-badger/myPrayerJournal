'use strict'

import fs from 'fs'

/**
 * Read and parse a JSON file
 * @param {string} path The path to the file
 * @param {string} encoding The encoding of the file (defaults to UTF-8)
 * @return {*} The parsed contents of the file
 */
export default (path, encoding = 'utf-8') =>
  JSON.parse(fs.readFileSync(path, encoding))
