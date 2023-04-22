using System.Collections.Generic;

namespace AccountService.API.Dto.Response;

public class CustomerProfileByEmailResponse
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public List<CustomerAddressResponse> Addresses { get; set; } = new();
    public CustomerProfileFavouriteDealerResponse FavouriteDealer { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}
