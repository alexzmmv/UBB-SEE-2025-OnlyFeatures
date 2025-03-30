using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using LearningPlat.Entities;

public class EnrollmentRepository
{
    private readonly DatabaseConnection _dbConnection;

    public EnrollmentRepository(DatabaseConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task EnrollUserInCourseAsync(int userId, int courseId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"IF NOT EXISTS (SELECT 1 FROM Enrollment WHERE users_id = @UserId AND course_id = @CourseId)
                          BEGIN
                            INSERT INTO Enrollment (users_id, course_id, enrolled_at)
                            VALUES (@UserId, @CourseId, @Now)
                          END";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);
                command.Parameters.AddWithValue("@Now", DateTime.UtcNow);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<bool> IsUserEnrolledAsync(int userId, int courseId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = "SELECT 1 FROM Enrollment WHERE users_id = @UserId AND course_id = @CourseId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);

                var result = await command.ExecuteScalarAsync();
                return result != null;
            }
        }
    }

    public async Task<List<Course>> GetEnrolledCoursesAsync(int userId)
    {
        var courses = new List<Course>();

        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"SELECT c.course_id, c.title, c.description, c.type_id, c.created_at, 
                         ct.type_name, ct.price
                         FROM Courses c
                         JOIN CourseTypes ct ON c.type_id = ct.type_id
                         JOIN Enrollment e ON c.course_id = e.course_id
                         WHERE e.users_id = @UserId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        courses.Add(new Course
                        {
                            CourseId = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Description = reader.GetString(2),
                            TypeId = reader.GetInt32(3),
                            CreatedAt = reader.GetDateTime(4)
                        });
                    }
                }
            }
        }

        return courses;
    }

    public async Task UnenrollUserFromCourseAsync(int userId, int courseId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = "DELETE FROM Enrollment WHERE users_id = @UserId AND course_id = @CourseId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}