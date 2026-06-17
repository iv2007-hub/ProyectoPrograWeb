using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.services;

namespace ProyectoPrograWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetReport(
        [FromQuery] string? categoryId = null,
        [FromQuery] string? zone = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        try
        {
            var report = await _reportService.GetReportAsync(categoryId, zone, from, to);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al generar el reporte", detail = ex.Message });
        }
    }
}