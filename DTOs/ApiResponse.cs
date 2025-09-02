using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
        public int? StatusCode { get; set; }
        public string? TraceId { get; set; }

        // Constructor para respuesta exitosa
        public static ApiResponse<T> SuccessResponse(T data, string message = "Operación exitosa")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = 200
            };
        }

        // Constructor para error simple
        public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = new List<string> { message }
            };
        }

        // Constructor para errores múltiples
        public static ApiResponse<T> ErrorResponse(List<string> errors, string message = "Se encontraron errores", int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }

        // Constructor para errores de validación
        public static ApiResponse<T> ValidationErrorResponse(Dictionary<string, string[]> validationErrors, string message = "Error de validación")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                ValidationErrors = validationErrors,
                StatusCode = 422
            };
        }
    }

    // Versión sin data para operaciones que no retornan datos
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string>? Errors { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
        public int? StatusCode { get; set; }
        public string? TraceId { get; set; }

        public static ApiResponse SuccessResponse(string message = "Operación exitosa")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                StatusCode = 200
            };
        }

        public static ApiResponse ErrorResponse(string message, int statusCode = 400)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = new List<string> { message }
            };
        }

        public static ApiResponse ErrorResponse(List<string> errors, string message = "Se encontraron errores", int statusCode = 400)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }

        public static ApiResponse ValidationErrorResponse(Dictionary<string, string[]> validationErrors, string message = "Error de validación")
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                ValidationErrors = validationErrors,
                StatusCode = 422
            };
        }
    }
}