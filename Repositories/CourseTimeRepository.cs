using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

public class CourseTimerRepository
{
    private readonly DatabaseConnection _dbConnection;

    public CourseTimerRepository(DatabaseConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task CreateOrUpdateTimerAsync(int userId, int courseId, int elapsedMinutes)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"IF EXISTS (SELECT 1 FROM CourseTimers WHERE users_id = @UserId AND course_id = @CourseId)
                          BEGIN
                            UPDATE CourseTimers 
                            SET elapsed_time_minutes = @ElapsedMinutes, last_updated = @Now
                            WHERE users_id = @UserId AND course_id = @CourseId
                          END
                          ELSE
                          BEGIN
                            INSERT INTO CourseTimers (users_id, course_id, elapsed_time_minutes, last_updated, is_completed)
                            VALUES (@UserId, @CourseId, @ElapsedMinutes, @Now, 0)
                          END";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);
                command.Parameters.AddWithValue("@ElapsedMinutes", elapsedMinutes);
                command.Parameters.AddWithValue("@Now", DateTime.UtcNow);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<CourseTimer> GetTimerAsync(int userId, int courseId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"SELECT timer_id, users_id, course_id, elapsed_time_minutes, last_updated, is_completed 
                         FROM CourseTimers 
                         WHERE users_id = @UserId AND course_id = @CourseId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new CourseTimer
                        {
                            TimerId = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            CourseId = reader.GetInt32(2),
                            ElapsedTimeMinutes = reader.GetInt32(3),
                            LastUpdated = reader.GetDateTime(4),
                            IsCompleted = reader.GetBoolean(5)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task CompleteTimerAsync(int userId, int courseId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"UPDATE CourseTimers 
                         SET is_completed = 1, last_updated = @Now
                         WHERE users_id = @UserId AND course_id = @CourseId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);
                command.Parameters.AddWithValue("@Now", DateTime.UtcNow);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<bool> IsTimerCompletedWithinLimitAsync(int userId, int courseId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"SELECT CASE WHEN ct.elapsed_time_minutes <= c.timer_duration_minutes THEN 1 ELSE 0 END
                         FROM CourseTimers ct
                         JOIN Courses c ON ct.course_id = c.course_id
                         WHERE ct.users_id = @UserId AND ct.course_id = @CourseId AND ct.is_completed = 1";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);

                var result = await command.ExecuteScalarAsync();
                return result != null && Convert.ToInt32(result) == 1;
            }
        }
    }
}