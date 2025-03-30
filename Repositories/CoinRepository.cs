using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using LearningPlat.Entities;

public class CoinRepository
{
    private readonly DatabaseConnection _dbConnection;

    public CoinRepository(DatabaseConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<decimal> GetUserCoinBalanceAsync(int userId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = "SELECT coin_balance FROM UserCoins WHERE users_id = @UserId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToDecimal(result) : 0;
            }
        }
    }

    public async Task UpdateUserCoinBalanceAsync(int userId, decimal newBalance)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"IF EXISTS (SELECT 1 FROM UserCoins WHERE users_id = @UserId)
                          BEGIN
                            UPDATE UserCoins 
                            SET coin_balance = @Balance
                            WHERE users_id = @UserId
                          END
                          ELSE
                          BEGIN
                            INSERT INTO UserCoins (users_id, coin_balance)
                            VALUES (@UserId, @Balance)
                          END";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Balance", newBalance);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task AwardDailyLoginBonusAsync(int userId)
    {
        decimal dailyBonus = 10; // Should come from configuration

        var currentBalance = await GetUserCoinBalanceAsync(userId);
        await UpdateUserCoinBalanceAsync(userId, currentBalance + dailyBonus);
    }

    public async Task AwardCourseCompletionBonusAsync(int userId, int courseId, decimal bonusAmount)
    {
        var currentBalance = await GetUserCoinBalanceAsync(userId);
        await UpdateUserCoinBalanceAsync(userId, currentBalance + bonusAmount);
    }

    public async Task<bool> PurchaseCourseAsync(int userId, decimal price)
    {
        var currentBalance = await GetUserCoinBalanceAsync(userId);

        if (currentBalance >= price)
        {
            await UpdateUserCoinBalanceAsync(userId, currentBalance - price);
            return true;
        }

        return false;
    }

    public async Task AwardImageInteractionBonusAsync(int userId, int pictureId, decimal bonusAmount)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            // Check if user already received coins for this image
            var checkQuery = "SELECT 1 FROM UserPictureCoins WHERE users_id = @UserId AND picture_id = @PictureId";

            using (var checkCommand = new SqlCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@UserId", userId);
                checkCommand.Parameters.AddWithValue("@PictureId", pictureId);

                var hasReceived = await checkCommand.ExecuteScalarAsync();

                if (hasReceived == null)
                {
                    // Award coins
                    var currentBalance = await GetUserCoinBalanceAsync(userId);
                    await UpdateUserCoinBalanceAsync(userId, currentBalance + bonusAmount);

                    // Record the transaction
                    var recordQuery = "INSERT INTO UserPictureCoins (users_id, picture_id, coins_received) VALUES (@UserId, @PictureId, @Amount)";

                    using (var recordCommand = new SqlCommand(recordQuery, connection))
                    {
                        recordCommand.Parameters.AddWithValue("@UserId", userId);
                        recordCommand.Parameters.AddWithValue("@PictureId", pictureId);
                        recordCommand.Parameters.AddWithValue("@Amount", bonusAmount);

                        await recordCommand.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }

    // Add these methods to the existing CoinRepository class

    public async Task AwardTimerCompletionBonusAsync(int userId, int courseId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            // Get timer completion reward amount from the course
            var rewardQuery = "SELECT timer_completion_reward FROM Courses WHERE course_id = @CourseId";

            using (var command = new SqlCommand(rewardQuery, connection))
            {
                command.Parameters.AddWithValue("@CourseId", courseId);
                var rewardAmount = await command.ExecuteScalarAsync();

                if (rewardAmount != null)
                {
                    decimal bonusAmount = Convert.ToDecimal(rewardAmount);

                    // Get user's current balance and update it
                    var currentBalance = await GetUserCoinBalanceAsync(userId);
                    await UpdateUserCoinBalanceAsync(userId, currentBalance + bonusAmount);
                }
            }
        }
    }

    public async Task<bool> PurchaseBonusModuleAsync(int userId, int moduleId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            // Get the unlock cost for the bonus module
            var costQuery = "SELECT unlock_cost FROM Modules WHERE module_id = @ModuleId AND is_bonus_module = 1";

            using (var command = new SqlCommand(costQuery, connection))
            {
                command.Parameters.AddWithValue("@ModuleId", moduleId);
                var result = await command.ExecuteScalarAsync();

                if (result != null)
                {
                    decimal unlockCost = Convert.ToDecimal(result);

                    // Check if user has enough coins and perform the purchase
                    var currentBalance = await GetUserCoinBalanceAsync(userId);

                    if (currentBalance >= unlockCost)
                    {
                        await UpdateUserCoinBalanceAsync(userId, currentBalance - unlockCost);
                        return true;
                    }
                }
            }
        }

        return false;
    }
}