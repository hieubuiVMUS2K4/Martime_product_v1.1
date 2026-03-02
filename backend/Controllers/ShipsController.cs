using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.DTOs;
using ProductApi.Models;
using ProductApi.Services;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipsController : ControllerBase
    {
        private readonly IShipService _shipService;
        public ShipsController(IShipService shipService)
        {
            _shipService = shipService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShipDto>>> Get()
        {
            var ships = await _shipService.GetAllAsync();
            return Ok(ships);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ShipDto>> Create(ShipDto dto)
        {
            var created = await _shipService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
    }
}
