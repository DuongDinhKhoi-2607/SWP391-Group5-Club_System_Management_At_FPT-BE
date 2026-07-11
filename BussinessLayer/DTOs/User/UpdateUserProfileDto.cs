using Microsoft.AspNetCore.Http;
using System;

namespace BussinessLayer.DTOs.User
{
    public class UpdateUserProfileDto
    {
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }
}
