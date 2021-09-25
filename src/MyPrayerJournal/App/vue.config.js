module.exports = {
  lintOnSave: false,
  outputDir: "../Server/wwwroot",
  configureWebpack: {
    module: {
      rules: [{
        test: /\.mjs$/,
        include: /node_modules/,
        type: "javascript/auto"
      }]
    }
  }
}
