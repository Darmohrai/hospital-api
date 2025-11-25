namespace hospital_api.Services;

public class ServiceResponse<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string ErrorMessage { get; set; } = string.Empty;

    public static ServiceResponse<T> Success(T data)
    {
        return new ServiceResponse<T> { Data = data };
    }

    public static ServiceResponse<T> Fail(string errorMessage)
    {
        return new ServiceResponse<T> { IsSuccess = false, ErrorMessage = errorMessage };
    }
}