
namespace rest_file_api.models
{
    public class ApiError {
        public ApiError(string message) {
            this.Message = message;
        }
        
        public string Message {get; set;}
    }
}