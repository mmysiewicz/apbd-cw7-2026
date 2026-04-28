using apbd_cw7_2026.DTOs;

namespace apbd_cw7_2026.Services;

public interface IAppointmentsService
{
    Task<IEnumerable<AppointmentListDto>> GetAllAppointmentsAsync();
}