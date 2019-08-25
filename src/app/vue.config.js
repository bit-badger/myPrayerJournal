const webpack = require('webpack')
// const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
module.exports = {
  outputDir: '../MyPrayerJournal.Api/wwwroot',
  configureWebpack: {
    plugins: [
      // new BundleAnalyzerPlugin(),
      new webpack.IgnorePlugin(/^\.\/locale$/, /moment$/)
    ],
    optimization: {
      splitChunks: {
        chunks: 'all'
      }
    }
  }
}
