using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.services;

namespace ProyectoPrograWeb.Controllers;

/// <summary>
/// Controlador para manejar las solicitudes de donación en DonaCerca.
/// </summary>
[ApiController]
[Route("api/requests")]
public class RequestController : ControllerBase
{
    private readonly RequestService _requestService;

    public RequestController(RequestService requestService)
    {
        _requestService = requestService;
    }

    /// <summary>
    /// Crea una nueva solicitud de donación.
    /// POST /api/requests
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CrearSolicitud([FromBody] CreateDonationRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar que el usuario autenticado sea el mismo que hace la solicitud
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

    /// <summary>
    /// Devuelve todas las solicitudes de una publicación.
    /// GET /api/requests/{postId}
    /// </summary>
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

    /// <summary>
    /// Devuelve todas las solicitudes del receptor autenticado.
    /// GET /api/requests/mine
    /// </summary>
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

    /// <summary>
    /// Busca una solicitud por su ID.
    /// GET /api/requests/detail/{id}
    /// </summary>
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

    /// <summary>
    /// El donante selecciona al receptor ganador.
    /// PUT /api/requests/{postId}/select-receiver
    /// </summary>
    [HttpPut("{postId}/select-receiver")]
    [Authorize]
    public async Task<IActionResult> SeleccionarReceptor(string postId, [FromBody] SelectReceiverDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar que el usuario autenticado sea el donante
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

    /// <summary>
    /// Cancela una solicitud pendiente.
    /// DELETE /api/requests/{id}/cancel
    /// </summary>
    [HttpDelete("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelarSolicitud(string id)
    {
        try
        {
            // Obtener el id del receptor del token
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