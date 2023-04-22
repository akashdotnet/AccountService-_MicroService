using System.ComponentModel.DataAnnotations;

namespace AccountService.API.Dto.Request;

public class FavouriteDealerRequest
{
    [Required] public int BusinessLocationId { get; set; }
}
