const webpack = require('webpack')
module.exports = {
  outputDir: '../MyPrayerJournal.Api/wwwroot',
  configureWebpack: {
    plugins: [
      new webpack.IgnorePlugin(/^\.\/locale$/, /moment$/)
    ]
  }
}
