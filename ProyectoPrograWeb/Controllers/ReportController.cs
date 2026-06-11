using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.services;

namespace ProyectoPrograWeb.Controllers;

// controlador que expone los endpoints de reportes para el administrador
[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportController(ReportService reportService)
    {
        _reportService = reportService;
    }

    // GET api/report
    // devuelve el reporte completo de estadisticas (totales, categorias, estados, tendencia semanal)
    // solo accesible para administradores
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetReport()
    {
        try
        {
            var report = await _reportService.GetReportAsync();
            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al generar el reporte", detail = ex.Message });
        }
    }
}
