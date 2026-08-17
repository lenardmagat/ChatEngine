using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.ErrorHandling.Extension
{
    public static class ResultExtension
    {
        public static IActionResult ToActionResult(this Result result)
        {
            return result.IsSuccess ? new OkResult(): 
                new ObjectResult(
                    new
                    {
                        error = result.Error,
                        timeStampt = DateTime.UtcNow,

                    }
                )
                {
                    StatusCode = result.StatusCode
                };
        }
        public static IActionResult ToActionResult<T>(this Result<T> result)
        {
            return result.IsSuccess ? 
                new OkObjectResult(result.Value) :
                new ObjectResult(
                    new
                    {
                        error = result.Error,
                        timeStampt = DateTime.UtcNow
                    }
                )
                {
                    StatusCode = result.StatusCode
                };
                
        }

        public static Result ToResult<T> (this Result<T> result, T Object)
        {
            return result.IsSuccess ? Result<Object>.Success(result.Value!) : Result<Object>.Failure(result.Error!, result.StatusCode);
        }

    } 
}