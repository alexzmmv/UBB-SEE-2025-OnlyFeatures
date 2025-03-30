using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

public class UserRepository
{
    private readonly DatabaseConnection _dbConnection;

    public UserRepository(DatabaseConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = "SELECT users_id, username, email, created_at FROM Users WHERE users_id = @UserId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Email = reader.GetString(2),
                            CreatedAt = reader.GetDateTime(3)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = "SELECT users_id, username, email, created_at FROM Users WHERE email = @Email";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", email);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Email = reader.GetString(2),
                            CreatedAt = reader.GetDateTime(3)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<int> CreateUserAsync(string username, string email)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"INSERT INTO Users (username, email, created_at)
                          OUTPUT INSERTED.users_id
                          VALUES (@Username, @Email, @Now)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Now", DateTime.UtcNow);

                return (int)await command.ExecuteScalarAsync();
            }
        }
    }

    public async Task InitializeUserCoinsAsync(int userId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"IF NOT EXISTS (SELECT 1 FROM UserCoins WHERE users_id = @UserId)
                          BEGIN
                            INSERT INTO UserCoins (users_id, coin_balance)
                            VALUES (@UserId, 0)
                          END";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}