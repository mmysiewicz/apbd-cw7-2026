using System.Threading.Tasks;
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
        public async Task<IActionResult> Get()
        {
            var appointments = await _appointmentsService.GetAllAppointmentsAsync();
            return Ok(appointments);
        }
    }
}