using ClassLibrary.Models;

namespace ClassLibrary.DtoModels.Screen
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public ClassLibrary.Models.Screen Screen { get; set; }
    }
}