import { describe, expect, it } from 'vitest'
import { formatCurrencyCompact, splitCompactSuffix } from '@/services/formatters'

describe('formatCurrencyCompact', () => {
  it.each([
    [50_000_000, '50J'],
    [7_451_000_000, '7,45M'],
    [1_250_000, '1,25J'],
    [512_000_000, '512J'],
    [1_234_567_890, '1,23M'],
    [17_150_091_632, '17,15M'],
    [50_000_000_000, '50M'],
    [2_500_000_000, '2,5M'],
    [1_020_000_000, '1,02M'],
    [2_000_000_000_000, '2T'],
    [1_234_567_890_123, '1,23T'],
    [999_999_999_999, '1.000M'],
    [850_000, '850.000'],
    [999, '999'],
    [100_000, '100.000'],
    [0, '0'],
  ])('formats %i as %s', (value, expected) => {
    expect(formatCurrencyCompact(value)).toBe(expected)
  })

  it('trims trailing zeros in the juta scale', () => {
    expect(formatCurrencyCompact(50_000_000)).toBe('50J')
    expect(formatCurrencyCompact(1_500_000)).toBe('1,5J')
  })

  it('trims trailing zeros in the miliar scale', () => {
    expect(formatCurrencyCompact(7_500_000_000)).toBe('7,5M')
    expect(formatCurrencyCompact(50_000_000_000)).toBe('50M')
    expect(formatCurrencyCompact(7_451_234_000)).toBe('7,45M')
  })

  it('keeps up to two fraction digits in the juta scale', () => {
    expect(formatCurrencyCompact(1_234_567)).toBe('1,23J')
  })

  it('formats negative values with the same scale', () => {
    expect(formatCurrencyCompact(-50_000_000)).toBe('-50J')
    expect(formatCurrencyCompact(-1_234_567_890)).toBe('-1,23M')
    expect(formatCurrencyCompact(-2_000_000_000_000)).toBe('-2T')
  })

  it('falls back to the full number below one juta', () => {
    expect(formatCurrencyCompact(999_999)).toBe('999.999')
    expect(formatCurrencyCompact(-850_000)).toBe('-850.000')
  })

  it('handles the borrow boundary around one triliun', () => {
    expect(formatCurrencyCompact(1_000_000_000_000)).toBe('1T')
    expect(formatCurrencyCompact(1_000_000_001_000)).toBe('1T')
  })
})

describe('splitCompactSuffix', () => {
  it.each([
    ['7,45M', { mantissa: '7,45', suffix: 'M' }],
    ['50J', { mantissa: '50', suffix: 'J' }],
    ['-1,23M', { mantissa: '-1,23', suffix: 'M' }],
    ['2T', { mantissa: '2', suffix: 'T' }],
    ['1.000M', { mantissa: '1.000', suffix: 'M' }],
  ])('splits compact value %s', (value, expected) => {
    expect(splitCompactSuffix(value)).toEqual(expected)
  })

  it('returns input untouched when there is no scaled suffix', () => {
    const plain = ['850.000', '78,0%', '12', '—', '(78,0%)']
    for (const value of plain) {
      expect(splitCompactSuffix(value)).toEqual({ mantissa: value, suffix: null })
    }
  })
})