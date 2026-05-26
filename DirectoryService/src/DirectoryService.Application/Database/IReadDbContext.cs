using DirectoryService.Domain.Locations;

namespace DirectoryService.Infrastructure;

public interface IReadDbContext
{
    IQueryable<Location> LocationsRead { get; }
}