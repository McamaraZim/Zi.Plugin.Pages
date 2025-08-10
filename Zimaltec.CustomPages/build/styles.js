import fs from 'fs'
import fse from 'fs-extra'
import config from './config.js'
import { execSync } from 'child_process'
import stylelint from 'stylelint'
import autoprefixer from 'autoprefixer'
import postcss from 'postcss'
import rtlcss from 'rtlcss'
import cssnano from 'cssnano'
import configureLogger from './logger.js'
import path from 'path'

// Setup logger with specific context
const log = configureLogger('Styles')

// Determine the desired operation based on command line argument
const operation = process.argv[2]

// Function to copy a file to the destination directory
async function copyFile(src, dest) {
  try {
    await fs.promises.copyFile(src, dest)
    console.log('Archivo copiado exitosamente')
  } catch (error) {
    console.error('Error al copiar el archivo:', error)
  }
}

// Function to lint SCSS files using stylelint
const lintScss = async () => {
  log.info('Linting SCSS...')
  try {
    const result = await stylelint.lint({
      files: `${config.path.scss}/**/*.scss`,
      configFile: '.stylelintrc.json',
      formatter: 'string',
    })

    if (result.errored) {
      console.log(result.report) // Log errors if linting failed
      throw new Error('Linting errors found')
    } else {
      log.success('Lint SCSS') // Log success if no errors found
    }
  } catch (error) {
    throw error // Rethrow error to be handled by the caller
  }
}

// Function to compile SCSS to CSS using Sass and enhance CSS with PostCSS plugins
const compileSass = async () => {
  const sassCommand = `sass --quiet --style expanded ${config.sass.sourcemap ? '--source-map --embed-sources' : '--no-source-map'} --no-error-css --load-path=../../../node_modules ${config.path.scss}/${config.fileNames.scss}.scss:${config.path.css}/${config.fileNames.css}.css`
  log.info('Compiling CSS...')
  try {
    execSync(sassCommand)
    log.success('Compile SCSS to CSS') // Log success if script runs without errors
  } catch (error) {
    log.error(
      `Compile SCSS to CSS hasn't been completed! ${error.stdout.toString()}`
    ) // Log failure and show error output
  }

  const cssPath = `${config.path.css}/${config.fileNames.css}.css`
  if (fs.existsSync(cssPath)) {
    const cssContent = fs.readFileSync(cssPath, 'utf-8')
    const result = await postcss([autoprefixer]).process(cssContent, {
      from: cssPath,
      to: cssPath,
    })
    fs.writeFileSync(cssPath, result.css)
    if (result.map) {
      fs.writeFileSync(`${cssPath}.map`, result.map.toString()) // Write source map if available
    }
    log.success(`Added vendor prefixes${config.sass.sourcemap ? ' and generated source map' : ''}`)
  }
}

// Function to copy static Css files
const copyCssFiles = async () => {
  log.info('Copying Css Files...')
  try {
    // Buscar y copiar los css
    const cssFiles = await fs.promises.readdir(config.path.src_css)

    if (cssFiles.length === 0) {
      log.error('No Static Css files found to copy.')
      return
    }
    // Create dist directory if it doesn't exist
    await fse.ensureDir(`${config.path.css}`)
    
    for (const file of cssFiles) {
      const filePath = `${config.path.src_css}/${file}`
      const destPath = path.join(config.path.css, path.basename(filePath))
      await copyFile(filePath, destPath) // Copy the modified file to the destination directory
    }

    log.success('Copied Css Static Files')
  } catch (error) {
    log.error(error.message)
  }
}

// Function to convert CSS file to its RTL version and handle related source map
const convertToRTL = async (inputPath) => {
  try {
    log.info('Converting CSS to RTL process...')
    const cssContent = fs.readFileSync(inputPath, 'utf-8')
    const inputMapPath = `${inputPath}.map`
    let options = {
      from: inputPath,
      to: `${inputPath.replace('.css', '.rtl.css')}`,
      map: config.sass.sourcemap ? { inline: false } : false,
    }

    if (fs.existsSync(inputMapPath)) {
      const previousMap = fs.readFileSync(inputMapPath, 'utf-8')
      options.map.prev = previousMap // Use previous map to generate a new one
    }

    const result = await postcss([rtlcss]).process(cssContent, options)
    fs.writeFileSync(options.to, result.css)
    if (result.map) {
      fs.writeFileSync(`${options.to}.map`, result.map.toString())
    }
    log.success(`CSS to RTL conversion process finished successfully!${config.sass.sourcemap ? ' and saved with source map' : ''}`)
  } catch (error) {
    log.error(`Failed to convert ${inputPath} to RTL: ${error.message}`)
  }
}

// Helper function to minify a single CSS file
const minifySingleCssFile = async (inputPath, outputPath) => {
  log.info('Starting CSS minification process...')
  const cssContent = await fs.promises.readFile(inputPath, 'utf8')
  const result = await postcss([
    cssnano({
      preset: [
        'default',
        {
          discardComments: { removeAll: true },
        },
      ],
    }),
  ]).process(cssContent, {
    from: inputPath,
    to: outputPath,
    map: config.sass.sourcemap ? { inline: false } : false, // Generate external source map files
  })

  await fs.promises.writeFile(outputPath, result.css)
  if (result.map) {
    await fs.promises.writeFile(`${outputPath}.map`, result.map.toString())
  }

  log.success(`CSS minification process finished succesfully!${config.sass.sourcemap ? ' and generated source map' : ''}`)
}

// Function to minify all non-RTL CSS files
const minifyCss = async () => {
  const files = await fs.promises.readdir(config.path.css)
  const cssFiles = files.filter(
    (file) =>
      file.endsWith('.css') &&
      !file.endsWith('.rtl.css') &&
      !file.endsWith('.min.css')
  )

  if (cssFiles.length === 0) {
    log.error('No CSS files found to minify.')
    return
  }

  for (const file of cssFiles) {
    const cssPath = `${config.path.css}/${file}`
    const minCssPath = `${config.path.css}/${file.replace('.css', '.min.css')}`
    await minifySingleCssFile(cssPath, minCssPath)
  }
}

// Function to minify all RTL CSS files
const minifyRtlCss = async () => {
  const files = await fs.promises.readdir(config.path.css)
  const rtlCssFiles = files.filter((file) => file.endsWith('.rtl.css'))

  if (rtlCssFiles.length === 0) {
    log.error('No RTL CSS files found to minify.')
    return
  }

  for (const file of rtlCssFiles) {
    const cssPath = `${config.path.css}/${file}`
    const minCssPath = `${config.path.css}/${file.replace('.rtl.css', '.rtl.min.css')}`
    await minifySingleCssFile(cssPath, minCssPath)
  }
}

// Main function to orchestrate the build process based on command line argument
const main = async () => {
  switch (operation) {
    case 'compile':
      if (config.sass.lintScss) {
        await lintScss()
      }
      if (config.path.scss) {
        await compileSass()
      }
      if (config.path.src_css) {
        await copyCssFiles()
      }
      break
    case 'rtl':
      if (config.path.css) {
        await convertToRTL(`${config.path.css}/${config.fileNames.css}.css`)
      }
      break
    case 'minify':
      if (config.path.css) {
        await minifyCss()
      }
      break
    case 'minify-rtl':
      if (config.path.css) {
        await minifyRtlCss()
      }
      break
    default:
      log.error(
        'Invalid command. Use either "compile", "rtl", "minify", or "minify-rtl"'
      )
  }
}

main().catch((error) => log.error(error.message))
