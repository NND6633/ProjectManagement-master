namespace ProjectManagement.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }

        public int StatusCode { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? ErrorCode { get; set; }

        public T? Data { get; set; }

        public static ApiResponse<T> SuccessResponse(
            T data,
            string message = "Success",
            int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Fail(
            string errorCode,
            string message,
            int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                ErrorCode = errorCode,
                Message = message,
                Data = default
            };
        }
    }
}