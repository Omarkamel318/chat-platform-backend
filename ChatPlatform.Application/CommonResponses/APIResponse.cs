using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.CommonResponses
{
    public class APIResponse<T>
    {
        public bool Success { get; set; }
        public string Messege { get; set; } = string.Empty;
        public T? Data { get; set; }
        public IReadOnlyList<string> Errors { get; set; } = new List<string>();


        public static APIResponse<T> Ok(T data, string message = "Success")
                => new APIResponse<T>
                {
                    Success = true,
                    Messege = message,
                    Data = data,
                };
        public static APIResponse<T> Fail(IReadOnlyList<string> errors, string message = "Fail")
            => new APIResponse<T>
            {
                Success = false,
                Messege = message,
                Errors = errors

            };
    }
}
