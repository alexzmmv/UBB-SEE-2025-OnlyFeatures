using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class UserProgressRepository
{
    private readonly DatabaseConnection _dbConnection;

    public UserProgressRepository(DatabaseConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task UpdateUserProgressAsync(int userId, int courseId, int moduleId, string status)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"IF EXISTS (SELECT 1 FROM UserProgress 
                           WHERE UserId = @UserId AND CourseId = @CourseId AND ModuleId = @ModuleId)
                          BEGIN
                            UPDATE UserProgress 
                            SET Status = @Status, LastUpdated = @Now
                            WHERE UserId = @UserId AND CourseId = @CourseId AND ModuleId = @ModuleId
                          END
                          ELSE
                          BEGIN
                            INSERT INTO UserProgress (UserId, CourseId, ModuleId, Status, LastUpdated)
                            VALUES (@UserId, @CourseId, @ModuleId, @Status, @Now)
                          END";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);
                command.Parameters.AddWithValue("@ModuleId", moduleId);
                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.AddWithValue("@Now", DateTime.UtcNow);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<UserProgress> GetUserProgressAsync(int userId, int courseId, int moduleId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = "SELECT ProgressId, UserId, CourseId, ModuleId, Status, LastUpdated " +
                        "FROM UserProgress WHERE UserId = @UserId AND CourseId = @CourseId AND ModuleId = @ModuleId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);
                command.Parameters.AddWithValue("@ModuleId", moduleId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new UserProgress
                        {
                            ProgressId = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            CourseId = reader.GetInt32(2),
                            ModuleId = reader.GetInt32(3),
                            Status = reader.GetString(4),
                            LastUpdated = reader.GetDateTime(5)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<List<UserProgress>> GetAllUserProgressForCourseAsync(int userId, int courseId)
    {
        var progressList = new List<UserProgress>();

        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = "SELECT ProgressId, UserId, CourseId, ModuleId, Status, LastUpdated " +
                        "FROM UserProgress WHERE UserId = @UserId AND CourseId = @CourseId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        progressList.Add(new UserProgress
                        {
                            ProgressId = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            CourseId = reader.GetInt32(2),
                            ModuleId = reader.GetInt32(3),
                            Status = reader.GetString(4),
                            LastUpdated = reader.GetDateTime(5)
                        });
                    }
                }
            }
        }

        return progressList;
    }

    public async Task<bool> IsCourseCompletedAsync(int userId, int courseId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"DECLARE @TotalModules INT
                          DECLARE @CompletedModules INT
                          
                          SELECT @TotalModules = COUNT(*) 
                          FROM Modules 
                          WHERE CourseId = @CourseId
                          
                          SELECT @CompletedModules = COUNT(*) 
                          FROM UserProgress 
                          WHERE UserId = @UserId AND CourseId = @CourseId AND Status = 'Completed'
                          
                          SELECT CASE WHEN @TotalModules = @CompletedModules THEN 1 ELSE 0 END AS IsCompleted";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) == 1;
            }
        }
    }
}