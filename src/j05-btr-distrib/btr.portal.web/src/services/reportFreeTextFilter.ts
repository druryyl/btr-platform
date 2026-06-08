function containsMultiWord(text: string, keyword: string): boolean {
  const words = keyword.trim().toLowerCase().split(/\s+/).filter(Boolean)
  if (words.length === 0) {
    return true
  }

  const haystack = text.toLowerCase()
  return words.every((word) => haystack.includes(word))
}

export function filterRowsByFreeText<T extends Record<string, unknown>>(
  rows: T[],
  keyword: string,
  fields: string[],
): T[] {
  const trimmed = keyword.trim()
  if (!trimmed) {
    return rows
  }

  return rows.filter((row) =>
    fields.some((field) => {
      const value = row[field]
      if (value == null) {
        return false
      }

      return containsMultiWord(String(value), trimmed)
    }),
  )
}
