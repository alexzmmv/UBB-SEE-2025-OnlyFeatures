using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using LearningPlat.Entities;

public class ModuleRepository
{
    private readonly DatabaseConnection _dbConnection;

    public ModuleRepository(DatabaseConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<List<Module>> GetModulesByCourseAsync(int courseId)
    {
        var modules = new List<Module>();

        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = "SELECT module_id, course_id, title, description, position FROM Modules WHERE course_id = @CourseId ORDER BY position";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CourseId", courseId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        modules.Add(new Module
                        {
                            ModuleId = reader.GetInt32(0),
                            CourseId = reader.GetInt32(1),
                            Title = reader.GetString(2),
                            Description = reader.GetString(3),
                            Position = reader.GetInt32(4),
                            IsBonusModule = reader.GetBoolean(5),
                            UnlockCost = reader.GetDecimal(6)

                        });
                    }
                }
            }
        }

        return modules;
    }

    public async Task<Module> GetModuleByIdAsync(int moduleId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = "SELECT module_id, course_id, title, description, position FROM Modules WHERE module_id = @ModuleId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ModuleId", moduleId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Module
                        {
                            ModuleId = reader.GetInt32(0),
                            CourseId = reader.GetInt32(1),
                            Title = reader.GetString(2),
                            Description = reader.GetString(3),
                            Position = reader.GetInt32(4),
                            IsBonusModule = reader.GetBoolean(5),
                            UnlockCost = reader.GetDecimal(6)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task MarkModuleAsCompletedAsync(int userId, int courseId, int moduleId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"IF EXISTS (SELECT 1 FROM UserProgress 
                           WHERE users_id = @UserId AND course_id = @CourseId AND module_id = @ModuleId)
                          BEGIN
                            UPDATE UserProgress 
                            SET progress_percentage = 100, last_updated = @Now
                            WHERE users_id = @UserId AND course_id = @CourseId AND module_id = @ModuleId
                          END
                          ELSE
                          BEGIN
                            INSERT INTO UserProgress (users_id, course_id, module_id, progress_percentage, last_updated)
                            VALUES (@UserId, @CourseId, @ModuleId, 100, @Now)
                          END";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);
                command.Parameters.AddWithValue("@ModuleId", moduleId);
                command.Parameters.AddWithValue("@Now", DateTime.UtcNow);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<bool> IsModuleCompletedAsync(int userId, int courseId, int moduleId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = "SELECT 1 FROM UserProgress WHERE users_id = @UserId AND course_id = @CourseId AND module_id = @ModuleId AND progress_percentage = 100";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);
                command.Parameters.AddWithValue("@ModuleId", moduleId);

                var result = await command.ExecuteScalarAsync();
                return result != null;
            }
        }
    }
}