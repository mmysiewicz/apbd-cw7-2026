using System.Collections.Generic;
using System.Threading.Tasks;
using apbd_cw7_2026.DTOs;

namespace apbd_cw7_2026.Services;

public interface IAppointmentsService
{
     Task<IEnumerable<AppointmentListDto>> GetAllAppointmentsAsync(string? status, string? patientLastName);
     Task<AppointmentDetailsDto> GetByIdAsync(int id);
     Task<int> CreateAppointmentAsync(CreateAppointmentRequestDto createAppointmentRequest);
     Task UpdateAppointmentAsync(int id, UpdateAppointmentRequestDto updateAppointmentRequest);
     Task DeleteAppointmentAsync(int id);
}