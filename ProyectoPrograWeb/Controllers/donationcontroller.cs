using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.services;

namespace ProyectoPrograWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class donationcontroller : ControllerBase
{
    private readonly donationservice _donationservice;

    public donationcontroller(donationservice donationservice)
    {
        _donationservice = donationservice;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost(createdonationpostDTo dTo)
    {
        var result = await _donationservice.CreatePost(dTo);
        return Ok(result);
    }
    
    [HttpGet]
    public IActionResult Test()
    {
        return Ok("Donation API funcionando");
    }
}