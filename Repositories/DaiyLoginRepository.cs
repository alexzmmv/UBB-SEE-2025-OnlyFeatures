using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using LearningPlat.Entities;

public class DailyLoginRepository
{
    private readonly DatabaseConnection _dbConnection;
    private readonly CoinRepository _coinRepository;

    public DailyLoginRepository(DatabaseConnection dbConnection, CoinRepository coinRepository)
    {
        _dbConnection = dbConnection;
        _coinRepository = coinRepository;
    }

    public async Task<bool> ProcessDailyLoginAsync(int userId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            // Check if user already received daily login bonus today
            var today = DateTime.UtcNow.Date;
            var checkQuery = "SELECT 1 FROM DailyLoginRewards WHERE users_id = @UserId AND reward_date = @Today";

            using (var command = new SqlCommand(checkQuery, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Today", today);

                var hasReceivedToday = await command.ExecuteScalarAsync();

                if (hasReceivedToday == null)
                {
                    // User hasn't received today's bonus, so award it
                    decimal dailyBonus = 10; // Should come from configuration
                    await _coinRepository.AwardDailyLoginBonusAsync(userId);

                    // Record the transaction
                    var insertQuery = @"INSERT INTO DailyLoginRewards (users_id, reward_date, coins_received)
                                      VALUES (@UserId, @Today, @Amount)";

                    using (var insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@UserId", userId);
                        insertCommand.Parameters.AddWithValue("@Today", today);
                        insertCommand.Parameters.AddWithValue("@Amount", dailyBonus);

                        await insertCommand.ExecuteNonQueryAsync();
                    }

                    return true;
                }
            }
        }

        return false; // User already received today's bonus
    }

    public async Task<DateTime?> GetLastLoginRewardDateAsync(int userId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = "SELECT MAX(reward_date) FROM DailyLoginRewards WHERE users_id = @UserId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                var result = await command.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToDateTime(result);
                }
            }
        }

        return null;
    }
}