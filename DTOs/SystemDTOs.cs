namespace ChatSystem.DTOs;
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; } // Useful for showing stack traces during local debugging
    }