using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.services;

namespace ProyectoPrograWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonationcController : ControllerBase
{
    private readonly donationservice _donationService;

    public DonationcController(donationservice donationService)
    {
        _donationService = donationService;
    }
    
    [HttpGet("{id}")]

    public async Task<IActionResult> GetById(string id)
    {
        var result = await _donationService.GetPostById(id);

        if (result == null)
        {
            return NotFound("Publicacion no encontrada");
        }
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] createdonationpostDTo? dto)
    {
        if (dto == null)
        {
            return BadRequest("Datos invalidos");
        }
        
        var result = await _donationService.CreatePost(dto);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromQuery] string donorId, [FromBody] updatedonationpostDTo? dto)
    {
        if (dto == null)
        {
            return BadRequest("Datos invalidos");
        }
        
        var result = await _donationService.UpdatePost(id, donorId, dto);

        if (!result)
        {
            return BadRequest("No se pudo actualizar la publicacion");
        }
        return Ok("Publicacion actualizada correctamente");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromQuery] string donorId)
    {
        var result = await _donationService.DesactivePost(id, donorId);

        if (!result)
        {
            return BadRequest("No se pudo eliminar la publicacion");
        }

        return Ok("Publicacion eliminada correctamente");
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _donationService.GetAllActivePost();
        return Ok(result);
    }

    [HttpGet("my-donations/{donorId}")]
    public async Task<IActionResult> GetMyDonations(string donorId)
    {
        var result = await _donationService.GetMyDonor(donorId);
        return Ok(result);
    }

    [HttpPost("{id}/accept-request/{requestId}")]
    public async Task<IActionResult> AcceptRequest(string id, string requestId)
    {
        var result = await _donationService.ChangeStatusToReserved(id, requestId);

        if (!result)
        {
            return BadRequest("No se pudo reservar la publicacion");
        }

        return Ok("Solicitud aceptada y publicacion reservada");
    }

    [HttpPost("{id}/deliver")]
    public async Task<IActionResult> Deliver(string id)
    {
        var result = await _donationService.ChangeStatusToDelivered(id);

        if (!result)
        {
            return BadRequest("No se pudo marcar como entregada");
        }
        return Ok("Publicacion entregada");
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var result = await _donationService.GetHistory();
        return Ok(result);
    }
}