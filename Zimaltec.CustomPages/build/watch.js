import config from './config.js'
import browserSyncPackage from 'browser-sync'
const browserSync = browserSyncPackage.create()
import { spawn } from 'child_process'
import fs from 'fs'
import path from 'path'


// Initialize BrowserSync to create a development server
browserSync.init({
    //server: {
    //  baseDir: './', // Set the base directory for the server
    //},
    open: false, // Automatically open the browser when the server starts
    proxy: config.browserSync.proxy,
})

// Function to copy a file to the destination directory
function copyFile(src, dest) {
    //console.log(`src param: ${src}`)
    //console.log(`dest param: ${dest}`)
    fs.copyFile(src, dest, (err) => {
        if (err) throw err
        console.log(`${src} was copied to ${dest}`)
    })
}

if (config.path.src_cshtml) {
    // Watch for changes in CSHTML files and reload the browser when any changes are detected
    browserSync.watch([`${config.path.src_cshtml}/*.cshtml`, `${config.path.src_cshtml}/**/*.cshtml`]).on('change', (filePath) => {

        fs.access(path.resolve(config.path.web_cshtml), fs.constants.F_OK, (err) => {
            if (err) {
                console.error('Folder doesnt exist.')
            } else {
                //console.log('Folder exists.')
                const relativePath = path.relative(config.path.src_cshtml, filePath);
                const destPath = path.join(config.path.web_cshtml, relativePath)
                //console.log(relativePath)
                //console.log(destPath)
                copyFile(filePath, destPath) // Copy the modified file to the destination directory
                browserSync.reload() // Reload the page in the browser
            }
        })
    })
}

if (config.path.scss) {
    // Watch for changes in SCSS files
    browserSync.watch([`${config.path.scss}/*.scss`, `${config.path.scss}/**/*.scss`]).on('change', () => {
        // Spawn a child process to run the 'styles:minified' npm script
        const stylesProcess = spawn('npm', ['run', 'styles'], {
            shell: true,
            stdio: 'inherit', // Inherit stdio to log npm script output in the same console
        })

        // When the npm script completes, reload CSS files in the browser
        stylesProcess.on('close', async () => {

            // Buscar y copiar los css
            const files = await fs.promises.readdir(config.path.css)
            const cssFiles = files.filter(
                (file) =>
                    file.endsWith('.css')
            )

            if (cssFiles.length === 0) {
                log.error('No CSS files found to copy.')
                return
            }

            for (const file of cssFiles) {
                const filePath = `${config.path.css}/${file}`
                const destPath = path.join(config.path.web_css, path.basename(filePath))
                copyFile(filePath, destPath) // Copy the modified file to the destination directory
            }

            browserSync.reload('*.css') // Only reload CSS files to prevent full page refresh
        })
    })
}

if (config.path.src_js) {
    // Watch for changes in JavaScript files
    browserSync.watch([`${config.path.src_js}/*.js`, `${config.path.src_js}/**/*.js`]).on('change', () => {
        // Spawn a child process to run the 'scripts:minified' npm script
        const scriptsProcess = spawn('npm', ['run', 'scripts'], {
            shell: true,
            stdio: 'inherit', // Inherit stdio to log npm script output in the same console
        })

        // When the npm script completes, reload the browser
        scriptsProcess.on('close', async () => {

            // Buscar y copiar los js
            const files = await fs.promises.readdir(config.path.js)
            const jsFiles = files.filter(
                (file) =>
                    file.endsWith('.js')
            )

            if (jsFiles.length === 0) {
                log.error('No js files found to copy.')
                return
            }

            for (const file of jsFiles) {
                const filePath = `${config.path.js}/${file}`
                const destPath = path.join(config.path.web_js, path.basename(filePath))
                copyFile(filePath, destPath) // Copy the modified file to the destination directory
            }

            browserSync.reload() // Reload the page in the browser
        })
    })
}