using System;
using System.ComponentModel.DataAnnotations;

namespace apbd_cw7_2026.DTOs;

public class AppointmentDetailsDto
{
    public string PatientEmail { get; set; } = string.Empty;
    public string PatientPhoneNumber  { get; set; } = string.Empty;
    public string DoctorLicenseNumber { get; set; } = string.Empty;
    public string? InternalNotes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
}