using System.Collections.Generic;
using System.Threading.Tasks;
using apbd_cw7_2026.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace apbd_cw7_2026.Services;

public class AppointmentsService : IAppointmentsService
{
    private readonly string _connectionString;

    public AppointmentsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public async Task<IEnumerable<AppointmentListDto>> GetAllAppointmentsAsync()
    {
        var query = "SELECT IdAppointment, Status FROM Appointments";
        
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = query;

        var reader = await command.ExecuteReaderAsync();
        
        var appointments = new List<AppointmentListDto>();
        while (await reader.ReadAsync())
        {
            var appointment = new AppointmentListDto()
            {
                IdAppointment = reader.GetInt32(0),
                Status = reader.GetString(1),
            };
            appointments.Add(appointment);
        }
        
        return appointments;
    }
}