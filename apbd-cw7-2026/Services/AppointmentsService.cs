using apbd_cw7_2026.DTOs;
using Microsoft.Data.SqlClient;


namespace apbd_cw7_2026.Services;

public class AppointmentsService : IAppointmentsService
{
    private readonly string _connectionString;

    public AppointmentsService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? String.Empty;
    }
    
    public async Task<IEnumerable<AppointmentListDto>> GetAllAppointmentsAsync(string? status, string? patientLastName )
    {
        var appointments =  new List<AppointmentListDto>();
        
        var query = """
                    SELECT
                        a.IdAppointment,
                        a.AppointmentDate,
                        a.Status,
                        a.Reason,
                        p.FirstName + N' ' + p.LastName AS PatientFullName,
                        p.Email AS PatientEmail
                    FROM dbo.Appointments a
                    JOIN dbo.Patients p ON p.IdPatient = a.IdPatient
                    WHERE (@Status IS NULL OR a.Status = @Status)
                      AND (@PatientLastName IS NULL OR p.LastName = @PatientLastName)
                    ORDER BY a.AppointmentDate;
                    """;
        
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(status) ? DBNull.Value : status);
        command.Parameters.AddWithValue("@PatientLastName", string.IsNullOrEmpty(patientLastName) ? DBNull.Value : patientLastName);
        
        await using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var appointment = new AppointmentListDto
            {
                IdAppointment = reader.GetInt32(reader.GetOrdinal("IdAppointment")),
                AppointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Reason = reader.GetString(reader.GetOrdinal("Reason")),
                PatientFullName = reader.GetString(reader.GetOrdinal("PatientFullName")),
                PatientEmail = reader.GetString(reader.GetOrdinal("PatientEmail")),
            };
            appointments.Add(appointment);
        }
        
        return appointments;
    }

    public async Task<AppointmentDetailsDto> GetByIdAsync(int id)
    {
        var query = """
                    SELECT
                        p.IdAppointment,
                        p.PhoneNumber,
                        d.LicenseNumber,
                        a.InternalNotes,
                        a.CreatedAt
                    FROM dbo.Appointments a
                    JOIN dbo.Patients p ON p.IdPatient = a.IdPatient
                    JOIN dbo.Doctors d ON d.IdDoctor = a.IdDoctor
                    WHERE a.IdAppointment = @IdAppointment
                    """;
        
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdAppointment", id);
        
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var appointment = new AppointmentDetailsDto
        {
            PatientEmail = reader.GetString(reader.GetOrdinal("PatientEmail")),
            PatientPhoneNumber = reader.GetString(reader.GetOrdinal("PatientPhoneNumber")),
            DoctorLicenseNumber = reader.GetString(reader.GetOrdinal("DoctorLicenseNumber")),
            InternalNotes = reader.GetString(reader.GetOrdinal("InternalNotes")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };

        return appointment;
    }

    public async Task<int> CreateAppointmentAsync(CreateAppointmentRequestDto createAppointmentRequest)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        if (createAppointmentRequest.AppointmentDate < DateTime.Now)
        {
            throw new ArgumentException("Data wizyty nie może odbyć się w przeszłości");
        }

        var checkDoctor = """
                          SELECT 1 FROM dbo.Doctors d WHERE d.IdDoctor = @IdDoctor AND IsActive = 1
                          """;
        await using var check = new SqlCommand(checkDoctor, connection);
        check.Parameters.AddWithValue("@IdDoctor", createAppointmentRequest.IdDoctor);
        check.Parameters.AddWithValue("@AppointmentDate", createAppointmentRequest.AppointmentDate);
        
        if (await check.ExecuteScalarAsync() == null)
        {
            throw new ArgumentException("Doktor nie istnieje, lub ma zaplanowaną wizytę");
        }

        var insrt = """
                    INSERT INTO dbo.Appointments(IdPatient, IdDoctor, AppointmentDate, Reason, Status)
                    VALUES (@IdPatient, @IdDoctor, @AppointmentDate, @Reason, 'Scheduled');
                    """;
        
        await using var insertCommand = new SqlCommand(insrt, connection);
        insertCommand.Parameters.AddWithValue("@IdPatient", createAppointmentRequest.IdPatient);
        insertCommand.Parameters.AddWithValue("@IdDoctor", createAppointmentRequest.IdDoctor);
        insertCommand.Parameters.AddWithValue("@AppointmentDate", createAppointmentRequest.AppointmentDate);
        insertCommand.Parameters.AddWithValue("@Reason", createAppointmentRequest.Reason);
        
        var result = await insertCommand.ExecuteNonQueryAsync();
        return result;
    }

    public async Task UpdateAppointmentAsync(int id, UpdateAppointmentRequestDto updateAppointmentRequest)
    { 
        
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var statusQuery = """
                          SELECT Status From dbo.Appointments WHERE IdAppointment = @IdAppointment;
                          """;
        await using var command = new SqlCommand(statusQuery, connection);
        command.Parameters.AddWithValue("@IdAppointment", id);

        string status = string.Empty;
        DateTime appointmentDate = DateTime.Now;
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                status = reader.GetString(reader.GetOrdinal("Status"));
                appointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate"));
            }
        }
        
        if (status == null)
        {
            throw new ArgumentException("Taka wizyta nie istnieje");
        }

        if (status == "Completed" && updateAppointmentRequest.Status != "Completed")
        {
            throw new ArgumentException("Wizyta jest już zakończona");
        }

        if (appointmentDate != updateAppointmentRequest.AppointmentDate)
        {
            throw new ArgumentException("Niepopoprawna data");
        }
        
        var checkDoctor = """
                          SELECT 1 FROM dbo.Doctors d WHERE d.IdDoctor = @IdDoctor AND IsActive = 1;
                          """;
        await using var check = new SqlCommand(checkDoctor, connection);
        check.Parameters.AddWithValue("@IdDoctor", updateAppointmentRequest.IdDoctor);
        
        if (await check.ExecuteScalarAsync() == null)
        {
            throw new ArgumentException("Doktor nie istnieje lub jest nieaktywny");
        }
        
        var checkPatient = """
                          SELECT 1 FROM dbo.Patients p WHERE p.IdPatient = @IdPatient AND IsActive = 1;
                          """;
        await using var checkCommand = new SqlCommand(checkPatient, connection);
        checkCommand.Parameters.AddWithValue("@IdPatient", updateAppointmentRequest.IdPatient);
        
        
        if (await checkCommand.ExecuteScalarAsync() == null)
        {
            throw new ArgumentException("Pacjent nie istnieje lub jest nieaktywny");
        }

        var query = """
                    UPDATE dbo.Appointments 
                    SET IdPatient = @IdPatient, IdDoctor = @IdDoctor, AppointmentDate = @AppointmentDate, Status = @Status, Reason = @Reason, InternalNotes = @InternalNotes
                    WHERE IdAppointment = @IdAppointment;
                    """;
        await using var updateCommand = new SqlCommand(query, connection);
        updateCommand.Parameters.AddWithValue("@IdPatient", updateAppointmentRequest.IdPatient);
        updateCommand.Parameters.AddWithValue("@IdDoctor", updateAppointmentRequest.IdDoctor);
        updateCommand.Parameters.AddWithValue("@AppointmentDate", updateAppointmentRequest.AppointmentDate);
        updateCommand.Parameters.AddWithValue("@Status", updateAppointmentRequest.Status);
        updateCommand.Parameters.AddWithValue("@Reason", updateAppointmentRequest.Reason);
        updateCommand.Parameters.AddWithValue("@InternalNotes", updateAppointmentRequest.InternalNotes);
        updateCommand.Parameters.AddWithValue("@IdAppointment", id);
        
        await updateCommand.ExecuteNonQueryAsync();
    }

    public async Task DeleteAppointmentAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var statusQuery = """
                          SELECT Status From dbo.Appointments WHERE IdAppointment = @IdAppointment;
                          """;
        await using var command = new SqlCommand(statusQuery, connection);
        command.Parameters.AddWithValue("@IdAppointment", id);
        
        string status = string.Empty;
        DateTime appointmentDate = DateTime.Now;
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                status = reader.GetString(reader.GetOrdinal("Status"));
                appointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate"));
            }
        }
        
        if (status == null)
        {
            throw new ArgumentException("Taka wizyta nie istnieje");
        }

        if (status == "Completed")
        {
            throw new ArgumentException("Nie można usunąć wizyty o statusie Completed");
        }

        var deleteQuery = """
                          DELETE FROM dbo.Appointments WHERE IdAppointment = @IdAppointment;
                          """;
        await using var deleteCommand = new SqlCommand(deleteQuery, connection);
        deleteCommand.Parameters.AddWithValue("@IdAppointment", id);
        
        await deleteCommand.ExecuteNonQueryAsync();
    }

}