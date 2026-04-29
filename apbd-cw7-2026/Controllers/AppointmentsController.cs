using System.Threading.Tasks;
using apbd_cw7_2026.DTOs;
using apbd_cw7_2026.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw7_2026.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentsService _appointmentsService;

        public AppointmentsController(IAppointmentsService appointmentsService)
        {
            _appointmentsService = appointmentsService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? status, [FromQuery] string? patientLastName)
        {
            var appointments = await _appointmentsService.GetAllAppointmentsAsync(status, patientLastName);
            return Ok(appointments);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var appointment = await _appointmentsService.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            return Ok(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequestDto createAppointmentRequest)
        {
            try
            {
                var id = await _appointmentsService.CreateAppointmentAsync(createAppointmentRequest);
                return Created("", id);

            }
            catch (ArgumentException argumentException)
            {
                return BadRequest(argumentException.Message);
            }
            catch(InvalidOperationException invalidOperationException)
            {
                return Conflict(invalidOperationException.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentRequestDto updateAppointmentRequest)
        {
            try
            {
                await _appointmentsService.UpdateAppointmentAsync(id, updateAppointmentRequest);
                return Ok();
            } catch (ArgumentException argumentException)
            {
                return NotFound(argumentException.Message);
            } catch (InvalidOperationException invalidOperationException)
            {
                return Conflict(invalidOperationException.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _appointmentsService.DeleteAppointmentAsync(id);
                return NoContent();
            } catch (ArgumentException argumentException)
            {
                return NotFound(argumentException.Message);
            } catch (InvalidOperationException invalidOperationException)
            {
                return Conflict(invalidOperationException.Message);
            }
        }
    }
}