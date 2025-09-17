using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Asset_Management.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SignalsController : Controller
    {
        private readonly ISignalsService _service;
        public SignalsController(ISignalsService service)
        {

            _service = service;
        }

        [HttpGet("Asset/{assetId}/AllSignals")]
        public async Task<IActionResult> GetSignals(int assetId)
        {
            try
            {
                IEnumerable<Signal> signals = await _service.GetSignals(assetId);
                return Ok(signals);

            }catch(Exception ex)
            {
                return BadRequest($"Error : {ex.Message}");
            }

        }

        [HttpGet("Asset/{assetId}/Signal/{signalId}")]
        public async Task<IActionResult> GetSpecificSignal(int assetId, int signalId)
        {
            try
            {
                Signal signal = await _service.GetSpecificSignal(assetId, signalId);
                return Ok(signal);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error : {ex.Message}");
            }
        }

        [HttpPost("Asset/{assetId}/AddSignal")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> AddSignal(int assetId, [FromBody] GlobalSignalDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Name or Description");
            }
            try
            {
                await _service.AddSignal(assetId, request);
                return Ok("Signal added successfully");

            }
            catch(DbUpdateException ex)
            {
                    return BadRequest("Signal already present under the same asset");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }


        }
        [HttpPut("Asset/{assetId}/UpdateSignal/{signalId}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> UpdateSignal(int assetId, int signalId ,[FromBody] GlobalSignalDTO request)
        {
            Console.WriteLine($"Signal Info {request.Name}, {request.ValueType}, {request.Description}");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Errors = errors });
            }

            try
            {
                await _service.UpdateSignal(assetId, signalId, request);

                return Ok("Signal updated successfully");

            }catch(DbUpdateException ex)
            {
                return BadRequest("Signal with same name already exists");
            }

            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("Asset/{assetId}/Delete/Signal/{signalId}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DeleteSignal(int signalId, int assetId)
        {
            try
            {
                await _service.DeleteSignal(signalId, assetId);
                return Ok("Signal deleted successfully");

            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

    }

    
    
}
