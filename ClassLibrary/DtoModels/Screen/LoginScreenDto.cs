using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.DtoModels.Screen
{
    public class LoginScreenDto
    {
        [Required]
        public string MACAddress { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
} 