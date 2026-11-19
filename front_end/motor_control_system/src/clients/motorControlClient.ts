import { GREETING_ENDPOINT } from '../config/constants'
import { getText } from './httpClient'


export const motorControlClient = {
  getGreeting: () => getText(GREETING_ENDPOINT),
}
