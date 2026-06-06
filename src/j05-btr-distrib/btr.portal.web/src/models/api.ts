export interface ApiResponse<T> {
  Status: string
  Code: number
  Message: string | null
  Data: T
}

export function isApiSuccess<T>(response: ApiResponse<T>): boolean {
  return response.Status === 'success' && response.Code === 200
}
