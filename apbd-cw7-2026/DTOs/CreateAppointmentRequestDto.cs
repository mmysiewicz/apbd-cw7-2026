using System;

namespace apbd_cw7_2026.DTOs;

public class CreateAppointmentRequestDto
{
    public string IdPatient { get; set; }
    public string IdDoctor { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}