const webpack = require('webpack')
module.exports = {
  outputDir: '../api/MyPrayerJournal.Api/wwwroot',
  configureWebpack: {
    plugins: [
      new webpack.IgnorePlugin(/^\.\/locale$/, /moment$/)
    ]
  }
}
