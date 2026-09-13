import { describe, expect, it } from 'vitest'
import {
  NO_DATA_LABEL,
  formatWithNoData,
  isMissingValue,
  orNoData,
} from './principalNoData'

describe('principalNoData', () => {
  it('exposes the shared label as exactly "No Data"', () => {
    expect(NO_DATA_LABEL).toBe('No Data')
    expect(NO_DATA_LABEL).not.toBe('0')
    expect(NO_DATA_LABEL).not.toBe('')
    expect(NO_DATA_LABEL).not.toBe('-')
    expect(NO_DATA_LABEL).not.toBe('—')
  })

  it('renders null as "No Data"', () => {
    expect(formatWithNoData<string>(null, (value) => value)).toBe('No Data')
    expect(orNoData(null)).toBe('No Data')
    expect(isMissingValue(null)).toBe(true)
  })

  it('renders undefined as "No Data"', () => {
    expect(formatWithNoData<string>(undefined, (value) => value)).toBe('No Data')
    expect(orNoData(undefined)).toBe('No Data')
    expect(isMissingValue(undefined)).toBe(true)
  })

  it('passes zero through instead of treating it as missing', () => {
    expect(formatWithNoData<number>(0, (value) => `${value}`)).toBe('0')
    expect(formatWithNoData<number>(0, (value) => `${value}`)).not.toBe('No Data')
    expect(isMissingValue(0)).toBe(false)
  })

  it('passes non-null values through the formatter', () => {
    expect(formatWithNoData<number>(82.4, (value) => `${value.toFixed(1)}%`)).toBe('82.4%')
    expect(formatWithNoData<string>('ABC Food', (value) => value)).toBe('ABC Food')
    expect(orNoData('XYZ Pharma')).toBe('XYZ Pharma')
    expect(isMissingValue('')).toBe(false)
  })
})
