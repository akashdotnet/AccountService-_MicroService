using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PodCommonsLibrary.Core.Annotations;

namespace AccountService.API.Dto.Request;

public class ProfilePhotoUploadRequest
{
    [Required]
    [MaxFileSize(3 * 1024 * 1024)] //3MB 
    [AllowedExtensionsAttribute(new[] {".png", "image/png", ".jpeg", "image/jpeg", ".jpg"})]
    public IFormFile ProfilePhoto { get; set; }
}
