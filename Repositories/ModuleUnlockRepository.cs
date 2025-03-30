using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LearningPlat.Entities;

public class ModuleUnlockRepository
{
    private readonly DatabaseConnection _dbConnection;
    private readonly CoinRepository _coinRepository;

    public ModuleUnlockRepository(DatabaseConnection dbConnection, CoinRepository coinRepository)
    {
        _dbConnection = dbConnection;
        _coinRepository = coinRepository;
    }

    public async Task<bool> UnlockBonusModuleAsync(int userId, int moduleId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            // Check if already unlocked
            if (await IsModuleUnlockedAsync(userId, moduleId))
            {
                return true;
            }

            // Get module and its unlock cost
            var query = "SELECT course_id, unlock_cost FROM Modules WHERE module_id = @ModuleId AND is_bonus_module = 1";
            decimal unlockCost = 0;
            int courseId = 0;

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ModuleId", moduleId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        courseId = reader.GetInt32(0);
                        unlockCost = reader.GetDecimal(1);
                    }
                    else
                    {
                        // Not a bonus module or doesn't exist
                        return false;
                    }
                }
            }

            // Try to purchase
            if (await _coinRepository.PurchaseCourseAsync(userId, unlockCost))
            {
                // Record the unlock
                var insertQuery = @"INSERT INTO UserModuleUnlocks (users_id, module_id, unlocked_at)
                                  VALUES (@UserId, @ModuleId, @Now)";

                using (var command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ModuleId", moduleId);
                    command.Parameters.AddWithValue("@Now", DateTime.UtcNow);

                    await command.ExecuteNonQueryAsync();
                    return true;
                }
            }

            return false;
        }
    }

    public async Task<bool> IsModuleUnlockedAsync(int userId, int moduleId)
    {
        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            // First check if it's a bonus module
            var query = "SELECT is_bonus_module FROM Modules WHERE module_id = @ModuleId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ModuleId", moduleId);
                var isBonusModule = await command.ExecuteScalarAsync();

                if (isBonusModule == null || Convert.ToBoolean(isBonusModule) == false)
                {
                    // Not a bonus module, so it follows normal module access rules
                    return true;
                }
            }

            // Check if this bonus module is unlocked
            query = "SELECT 1 FROM UserModuleUnlocks WHERE users_id = @UserId AND module_id = @ModuleId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@ModuleId", moduleId);

                var result = await command.ExecuteScalarAsync();
                return result != null;
            }
        }
    }

    public async Task<List<Module>> GetUnlockedBonusModulesAsync(int userId, int courseId)
    {
        var modules = new List<Module>();

        using (var connection = _dbConnection.GetConnection())
        {
            await connection.OpenAsync();

            var query = @"SELECT m.module_id, m.course_id, m.title, m.description, m.position, m.is_bonus_module, m.unlock_cost
                         FROM Modules m
                         JOIN UserModuleUnlocks umu ON m.module_id = umu.module_id
                         WHERE umu.users_id = @UserId AND m.course_id = @CourseId AND m.is_bonus_module = 1";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
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
}