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
        public ActionResult<IEnumerable<ShipDto>> Get()
        {
            var ships = _shipService.GetAll();
            return Ok(ships);
        }

        [HttpPost]
        [Authorize]
        public ActionResult<ShipDto> Create(ShipDto dto)
        {
            var created = _shipService.Create(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
    }
}
