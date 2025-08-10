import config from './config.js'
import fs from 'fs-extra'
import { JSDOM } from 'jsdom'
import path from 'path'
import configureLogger from './logger.js'

// Setup logger with specific context
const log = configureLogger('Resources')

// Main function to manage the resources process
const resources = async () => {
  try {
    log.info('Starting resources process...')
    if (config.path.img) {
      // Create dist directory if it doesn't exist
      await fs.ensureDir(`${config.path.img}`)
      await fs.copy(
          `${config.path.src_img}`,
          `${config.path.img}`
      )
    }
    if (config.path.fonts) {
      await fs.ensureDir(`${config.path.fonts}`)
      await fs.copy(
          `${config.path.src_fonts}`,
          `${config.path.fonts}`
      )
    }
    log.success('Resources process completed successfully!')
  } catch (error) {
    log.error(`Failed to complete resources process: ${error}`)
  }
}

// Start the distribution process
resources()
