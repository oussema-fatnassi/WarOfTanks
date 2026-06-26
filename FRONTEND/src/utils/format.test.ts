import { describe, expect, it } from 'vitest'
import { formatDateTime, formatDuration } from './format'

describe('format utilities', () => {
  it('formats a duration as mm:ss', () => {
    expect(formatDuration(0)).toBe('00:00')
    expect(formatDuration(61)).toBe('01:01')
    expect(formatDuration(602)).toBe('10:02')
  })

  it('rounds the seconds part without changing the minute floor', () => {
    expect(formatDuration(89.7)).toBe('01:30')
  })

  it('formats an ISO date using the browser local timezone', () => {
    const iso = '2026-05-12T21:14:00.000Z'
    const date = new Date(iso)
    const pad = (value: number) => String(value).padStart(2, '0')

    expect(formatDateTime(iso)).toBe(
      `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(date.getMinutes())}`,
    )
  })
})
