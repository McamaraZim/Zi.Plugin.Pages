const config = {
  path: {
    src_cshtml: 'Themes',
    web_cshtml: '../../../Presentation/Nop.Web/Plugins/Zimaltec.CustomPages/Themes',
    src_css: '',
    scss: '',
    css: '',
    web_css: '',
    src_js: 'src/js',
    js: 'assets/js',
    js_override: '',
    web_js: '../../../Presentation/Nop.Web/Plugins/Zimaltec.CustomPages/assets/js',
    src_img: '',
    img: '',
    web_img: '',
    src_fonts: '',
    fonts: '',
    web_fonts: '',
    vendor: 'assets/vendor',
    web_themes: '../../../Presentation/Nop.Web/Themes',
  },
  sass: {
    lintScss: false,
    sourcemap: false,
  },
  bundleJs: {
    lintJs: false,
    sourcemap: false,
  },
  fileNames: {
    scss: '',
    css: '',
    js: 'theme',
  },
  jsBanner: `
  /*!
   * Zimaltec Custom Pages Plugin
   * Copyright 2025 Zimaltec Team
   * Plugin scripts
   *
   * @copyright Zimaltec Team
   * @version 1.0.0
   */
  `,
  browserSync: {
    proxy: 'http://localhost:5000',
  }
}

export default config
