namespace BtrReturGudang.AppTier.DTOs
{
    /// <summary>
    /// Rich result model for Cloud API upload operations.
    /// Carries the HTTP status code, success flag, error message, and the raw response payload
    /// so callers can surface detailed diagnostics for failed uploads.
    /// </summary>
    public class ApiUploadResultDto
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string RawResponse { get; set; }

        public static ApiUploadResultDto Ok(int statusCode = 200, string rawResponse = null)
        {
            return new ApiUploadResultDto
            {
                Success = true,
                StatusCode = statusCode,
                ErrorMessage = null,
                RawResponse = rawResponse
            };
        }

        public static ApiUploadResultDto Fail(int statusCode, string errorMessage, string rawResponse = null)
        {
            return new ApiUploadResultDto
            {
                Success = false,
                StatusCode = statusCode,
                ErrorMessage = errorMessage,
                RawResponse = rawResponse
            };
        }
    }
}
