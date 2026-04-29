using System;
using System.ComponentModel.DataAnnotations;

namespace apbd_cw7_2026.DTOs;

public class CreateAppointmentRequestDto
{
    public string IdPatient { get; set; }
    public string IdDoctor { get; set; }
    public DateTime AppointmentDate { get; set; }
    [StringLength(250)]
    public string Reason { get; set; } = string.Empty;
}