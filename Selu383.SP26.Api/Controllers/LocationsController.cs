using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Locations;
using Selu383.SP26.Api.Features.Users;

namespace Selu383.SP26.Api.Controllers;

[Route("api/locations")]
[ApiController]
public class LocationsController(
    DataContext dataContext
    ) : ControllerBase
{
    [HttpGet]
    public IQueryable<LocationDto> GetAll()
    {
        return dataContext.Set<Location>()
            .Select(x => new LocationDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                TableCount = x.TableCount,
                ManagerId = x.ManagerId,
                });
    }

    [HttpGet("{id}")]
    public ActionResult<LocationDto> GetById(int id)
    {
        var result = dataContext.Set<Location>()
            .FirstOrDefault(x => x.Id == id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(new LocationDto
        {
            Id = result.Id,
            Name = result.Name,
            Address = result.Address,
            TableCount = result.TableCount,
            ManagerId = result.ManagerId,
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public ActionResult<LocationDto> Create(LocationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest();
        }
        if (dto.Name!.Length > 120)
        {
            return BadRequest();
        }
        if (string.IsNullOrWhiteSpace(dto.Address))
        {
            return BadRequest();
        }
        if (dto.TableCount < 1)
        {
            return BadRequest();
        }
        // ManagerId must be valid user if provided
        if (dto.ManagerId.HasValue && !dataContext.Users.Any(u => u.Id == dto.ManagerId.Value))
        {
            return BadRequest();
        }
        var location = new Location
        {
            Name = dto.Name!,
            Address = dto.Address!,
            TableCount = dto.TableCount,
            ManagerId = dto.ManagerId,
        };

        dataContext.Set<Location>().Add(location);
        dataContext.SaveChanges();

        dto.Id = location.Id;

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [Authorize]
    [HttpPut("{id}")]
    public ActionResult<LocationDto> Update(int id, LocationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest();
        }
        if (dto.Name!.Length > 120)
        {
            return BadRequest();
        }
        if (string.IsNullOrWhiteSpace(dto.Address))
        {
            return BadRequest();
        }
        if (dto.TableCount < 1)
        {
            return BadRequest();
        }

        var location = dataContext.Set<Location>()
            .FirstOrDefault(x => x.Id == id);

        if (location == null)
        {
            return NotFound();
        }

        // authorize: admin or manager of this location
        var isAdmin = User.IsInRole("Admin");
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var currentUserId);
        var isManager = location.ManagerId.HasValue && currentUserId == location.ManagerId.Value;
        if (!isAdmin && !isManager)
        {
            return Forbid();
        }

        location.Name = dto.Name!;
        location.Address = dto.Address!;
        location.TableCount = dto.TableCount;
        // Only admins may change ManagerId
        if (isAdmin)
        {
            if (dto.ManagerId.HasValue && !dataContext.Users.Any(u => u.Id == dto.ManagerId.Value))
            {
                return BadRequest();
            }
            location.ManagerId = dto.ManagerId;
        }

        dataContext.SaveChanges();

        dto.Id = location.Id;

        return Ok(dto);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var location = dataContext.Set<Location>()
            .FirstOrDefault(x => x.Id == id);

        if (location == null)
        {
            return NotFound();
        }

        dataContext.Set<Location>().Remove(location);
        dataContext.SaveChanges();

        return Ok();
    }
}
