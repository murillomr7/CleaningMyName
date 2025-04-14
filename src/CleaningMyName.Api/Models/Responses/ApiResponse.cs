namespace CleaningMyName.Api.Models.Responses;

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    
    public ApiResponse(bool success = true, string? message = null)
    {
        Success = success;
        Message = message;
    }
    
    public static ApiResponse SuccessResponse(string? message = null)
    {
        return new ApiResponse(true, message);
    }
    
    public static ApiResponse ErrorResponse(string message)
    {
        return new ApiResponse(false, message);
    }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
    
    public ApiResponse(bool success = true, string? message = null, T? data = default)
        : base(success, message)
    {
        Data = data;
    }
    
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>(true, message, data);
    }
    
    public static ApiResponse<T> ErrorResponse(string message)
    {
        return new ApiResponse<T>(false, message);
    }
}
