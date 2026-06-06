namespace btr.portal.api.Models
{
    public class ApiResponse<T>
    {
        public string Status { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static ApiResponse<T> Success(T data)
        {
            return new ApiResponse<T>
            {
                Status = "success",
                Code = 200,
                Message = null,
                Data = data
            };
        }

        public static ApiResponse<T> Error(int code, string message)
        {
            return new ApiResponse<T>
            {
                Status = "error",
                Code = code,
                Message = message,
                Data = default
            };
        }
    }
}
