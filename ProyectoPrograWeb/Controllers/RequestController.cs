using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.services;

namespace ProyectoPrograWeb.Controllers;

[ApiController]
[Route("api/requests")]
public class RequestController : ControllerBase
{
    private readonly RequestService _requestService;

    public RequestController(RequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CrearSolicitud([FromBody] CreateDonationRequestDTo dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != dto.ReceiverId)
                return Unauthorized(new { mensaje = "No puedes crear solicitudes para otro usuario" });

            var resultado = await _requestService.CrearSolicitudAsync(
                dto.PostId,
                dto.ReceiverId,
                dto.ReceiverName
            );

            return CreatedAtAction(nameof(ObtenerSolicitudPorId),
                new { id = resultado.Id }, resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("{postId}")]
    public async Task<IActionResult> ObtenerSolicitudesPorPost(string postId)
    {
        try
        {
            var solicitudes = await _requestService.ObtenerSolicitudesPorPostAsync(postId);
            return Ok(solicitudes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> ObtenerMisSolicitudes()
    {
        try
        {
            var receiverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(receiverId))
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var solicitudes = await _requestService.ObtenerSolicitudesPorReceptorAsync(receiverId);
            return Ok(solicitudes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("detail/{id}")]
    public async Task<IActionResult> ObtenerSolicitudPorId(string id)
    {
        try
        {
            var solicitud = await _requestService.ObtenerSolicitudPorIdAsync(id);
            return Ok(solicitud);
        }
        catch (Exception ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{postId}/select-receiver")]
    [Authorize]
    public async Task<IActionResult> SeleccionarReceptor(string postId, [FromBody] SelectReceiverDTo dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var donorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(donorId))
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var resultado = await _requestService.SeleccionarReceptorAsync(postId, dto.RequestId, donorId);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id}/confirm-reception")]
    [Authorize]
    public async Task<IActionResult> ConfirmarRecepcion(string id)
    {
        try
        {
            var receiverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(receiverId))
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var resultado = await _requestService.ConfirmarRecepcionAsync(id, receiverId);
            return Ok(new { mensaje = "Recepción confirmada correctamente", exito = resultado });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelarSolicitud(string id)
    {
        try
        {
            var receiverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(receiverId))
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            var resultado = await _requestService.CancelarSolicitudAsync(id, receiverId);
            return Ok(new { mensaje = "Solicitud cancelada correctamente", exito = resultado });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}