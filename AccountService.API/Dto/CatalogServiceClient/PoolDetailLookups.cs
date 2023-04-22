using System.Collections.Generic;

namespace AccountService.API.Dto.CatalogServiceClient;

public class PoolDetailLookups
{
    public PoolDetailLookups()
    {
        PoolMaterials = new List<PoolMaterialResponse>();
        PoolSizes = new List<PoolSizeResponse>();
        PoolTypes = new List<PoolTypeResponse>();
        SanitationMethods = new List<SanitationMethodResponse>();
        PoolSeasons = new List<PoolSeasonResponse>();
        HotTubTypes = new List<HotTubTypeResponse>();
    }

    public List<PoolMaterialResponse> PoolMaterials { get; set; }
    public List<PoolTypeResponse> PoolTypes { get; set; }
    public List<PoolSizeResponse> PoolSizes { get; set; }
    public List<SanitationMethodResponse> SanitationMethods { get; set; }
    public List<PoolSeasonResponse> PoolSeasons { get; set; }
    public List<HotTubTypeResponse> HotTubTypes { get; set; }
}
