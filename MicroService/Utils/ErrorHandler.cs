using System;

namespace MicroService.Utils
{
    public class ErrorHandler
    {
        public static string Handle(Exception ex)
        {
            var error = ex.InnerException == null ?
                ex.Message :
                $"Error message: {ex.Message}. Inner exception: {ex.InnerException}";

            return error;
        }
    }
}
