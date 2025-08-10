import config from './config.js'
import { rollup } from 'rollup'
import nodeResolve from '@rollup/plugin-node-resolve'
import { babel } from '@rollup/plugin-babel'
import { minify } from 'terser'
import { ESLint } from 'eslint'
import configureLogger from './logger.js'
import fs from 'fs'
import path from 'path'

// Setup logger with specific context
const log = configureLogger('Scripts')

// Determine the desired operation based on command line arguments
const operation = process.argv[2]

// Function to lint JavaScript files using ESLint
const lintJS = async () => {
  log.info('Linting JavaScript...')
  const eslint = new ESLint()
  const results = await eslint.lintFiles([`${config.path.src_js}/*.js`, `${config.path.src_js}/**/*.js`])
  const errorResults = ESLint.getErrorResults(results)

  if (errorResults.length > 0) {
    const formatter = await eslint.loadFormatter('stylish')
    const resultText = formatter.format(errorResults)
    log.error(resultText)
    throw new Error('Linting errors found')
  } else {
    log.success('Lint JavaScript')
  }
}

// Function to copy a file to the destination directory
async function copyFile(src, dest) {
  try {
    await fs.promises.copyFile(src, dest)
    console.log('Archivo copiado exitosamente')
  } catch (error) {
    console.error('Error al copiar el archivo:', error)
  }
}

// Function to bundle JavaScript files using Rollup
const bundleJS = async () => {
  log.info('Bundling JavaScript Theme...')
  try {
    const inputOptions = {
      input: `./${config.path.src_js}/${config.fileNames.js}.js`,
      plugins: [
        nodeResolve(),
        babel({
          babelHelpers: 'bundled',
          exclude: '../../../node_modules/**',
        }),
      ],
      onwarn: (warning, warn) => {
        if (warning.code === 'THIS_IS_UNDEFINED') {
          return
        }
        warn(warning)
      },
    }

    const outputOptions = {
      file: `${config.path.js}/${config.fileNames.js}.js`,
      format: 'iife',
      sourcemap: config.bundleJs.sourcemap,
      banner: config.jsBanner,
    }

    const bundle = await rollup(inputOptions)
    await bundle.write(outputOptions)
    log.success('Bundled JavaScript Theme')
  } catch (error) {
    log.error(error.message)
  }
}

// Function to copy static JavaScript files
const copyJSFiles = async () => {
  log.info('Copying JavaScript Files...')
  try {
    // Buscar y copiar los js
    const overridePath = path.join(config.path.src_js, config.path.js_override);
    const jsFiles = await fs.promises.readdir(overridePath)

    if (jsFiles.length === 0) {
      log.error('No js files found to copy.')
      return
    }

    for (const file of jsFiles) {
      const filePath = `${overridePath}/${file}`
      const destPath = path.join(config.path.js, path.basename(filePath))
      await copyFile(filePath, destPath) // Copy the modified file to the destination directory
    }

    log.success('Copied JavaScript Files')
  } catch (error) {
    log.error(error.message)
  }
}

// Function to minify the bundled JavaScript file
const minifyJS = async () => {
  log.info('Minifying JavaScript...')
  try {
    const fileName = `${config.path.js}/${config.fileNames.js}.js`
    const fileContent = fs.readFileSync(fileName, 'utf8')
    const minified = await minify(fileContent, {
      compress: {
        passes: 2, // Compress the file in 2 passes for better optimization
      },
      mangle: true, // Shorten variable and function names
      format: {
        comments: /^!/, // Preserve comments that start with '!'
      },
      sourceMap: config.bundleJs.sourcemap ? {
        content: fs.readFileSync(
          `${config.path.js}/${config.fileNames.js}.js.map`,
          'utf8'
        ), // Assuming an existing source map is available
        url: `${config.fileNames.js}.min.js.map`, // Setting the source map URL in the output file
      } : false,
    })

    if (minified.error) {
      throw new Error(minified.error)
    }

    fs.writeFileSync(
      `${config.path.js}/${config.fileNames.js}.min.js`,
      minified.code
    ) // Write the minified code to the file system
    if (minified.map) {
      fs.writeFileSync(
        `${config.path.js}/${config.fileNames.js}.min.js.map`,
        minified.map
      ) // Write the source map if available
    }

    log.success('Minified JavaScript successfully')
  } catch (error) {
    log.error(`Failed to minify JavaScript: ${error.message}`)
  }
}

// Main function to manage script building process based on operation type
const buildScripts = async () => {
  try {
    switch (operation) {
      case 'compile':
        log.info('Starting JavaScript compilation process...')
        if (config.bundleJs.lintJs) {
          await lintJS()
        }
        if (config.path.src_js) {
          await bundleJS() // Bundle JavaScript
          await copyJSFiles() // Copy JavaScript static files
        }
        log.success('JavaScript compilation process finished succesfully!')
        break
      case 'minify':
        log.info('Starting JavaScript minification process...')
        if (config.path.js) {
          await minifyJS() // Minify the already bundled JavaScript file
        }
        log.success('JavaScript minification process finished succesfully!')
        break
      default:
        log.error('Invalid operation. Use either "compile" or "minify"') // Handle invalid operations
        break
    }
  } catch (error) {
    log.error(error.message) // Handle any errors during the process
  }
}

// Start the script build process
buildScripts()
