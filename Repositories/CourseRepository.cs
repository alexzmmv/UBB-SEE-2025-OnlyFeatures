using LearningPlat.Entities;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CourseRepository
{
    private readonly DatabaseConnection _dbConnection;

    public CourseRepository(DatabaseConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<List<Course>> GetAllCoursesAsync()
    {
        var courses = new List<Course>();

        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"SELECT c.course_id, c.title, c.description, c.type_id, c.created_at, 
                         ct.type_name, ct.price
                         FROM Courses c
                         JOIN CourseTypes ct ON c.type_id = ct.type_id";

            using (var command = new SqlCommand(query, connection))
            {
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
                            CreatedAt = reader.GetDateTime(4),
                            DifficultyLevel = reader.GetInt32(5),
                            TimerDurationMinutes = reader.GetInt32(6),
                            TimerCompletionReward = reader.GetDecimal(7),
                            CompletionReward = reader.GetDecimal(8)
                        });
                    }
                }
            }
        }

        return courses;
    }

    public async Task<Course> GetCourseByIdAsync(int courseId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"SELECT c.course_id, c.title, c.description, c.type_id, c.created_at, 
                         ct.type_name, ct.price
                         FROM Courses c
                         JOIN CourseTypes ct ON c.type_id = ct.type_id
                         WHERE c.course_id = @CourseId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CourseId", courseId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Course
                        {
                            CourseId = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Description = reader.GetString(2),
                            TypeId = reader.GetInt32(3),
                            CreatedAt = reader.GetDateTime(4),
                            DifficultyLevel = reader.GetInt32(5),
                            TimerDurationMinutes = reader.GetInt32(6),
                            TimerCompletionReward = reader.GetDecimal(7),
                            CompletionReward = reader.GetDecimal(8)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<List<Course>> GetCoursesByTypeAsync(string typeName)
    {
        var courses = new List<Course>();

        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"SELECT c.course_id, c.title, c.description, c.type_id, c.created_at, 
                         ct.type_name, ct.price
                         FROM Courses c
                         JOIN CourseTypes ct ON c.type_id = ct.type_id
                         WHERE ct.type_name = @TypeName";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TypeName", typeName);

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
                            CreatedAt = reader.GetDateTime(4),
                            DifficultyLevel = reader.GetInt32(5),
                            TimerDurationMinutes = reader.GetInt32(6),
                            TimerCompletionReward = reader.GetDecimal(7),
                            CompletionReward = reader.GetDecimal(8)
                        });
                    }
                }
            }
        }

        return courses;
    }

    public async Task<List<Course>> SearchCoursesAsync(string searchTerm)
    {
        var courses = new List<Course>();

        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"SELECT c.course_id, c.title, c.description, c.type_id, c.created_at, 
                         ct.type_name, ct.price
                         FROM Courses c
                         JOIN CourseTypes ct ON c.type_id = ct.type_id
                         WHERE c.title LIKE @SearchTerm";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

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
                            CreatedAt = reader.GetDateTime(4),
                            DifficultyLevel = reader.GetInt32(5),
                            TimerDurationMinutes = reader.GetInt32(6),
                            TimerCompletionReward = reader.GetDecimal(7),
                            CompletionReward = reader.GetDecimal(8)
                        });
                    }
                }
            }
        }

        return courses;
    }

    public async Task<List<string>> GetCourseTagsAsync(int courseId)
    {
        var tags = new List<string>();

        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"SELECT t.tag_name
                         FROM CourseTags ct
                         JOIN Tags t ON ct.tag_id = t.tag_id
                         WHERE ct.course_id = @CourseId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CourseId", courseId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tags.Add(reader.GetString(0));
                    }
                }
            }
        }

        return tags;
    }
}