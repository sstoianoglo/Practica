using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace PracticaApp.Properties
{
    internal sealed class ExerciseRepository
    {
        public void EnsureTable()
        {
            using MySqlConnection connection = new MySqlConnection(DBConnection.ConnectionString);
            connection.Open();

            string muscleGroupsQuery = @"
                CREATE TABLE IF NOT EXISTS MuscleGroups
                (
                    Id INT PRIMARY KEY AUTO_INCREMENT,
                    GroupName VARCHAR(50) NOT NULL
                )
                ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

            string exercisesQuery = @"
                CREATE TABLE IF NOT EXISTS Exercises
                (
                    Id INT PRIMARY KEY AUTO_INCREMENT,
                    ExerciseName VARCHAR(100) NOT NULL,
                    MuscleGroupId INT NOT NULL,
                    DifficultyLevel VARCHAR(30),
                    Equipment VARCHAR(50),
                    FOREIGN KEY (MuscleGroupId) REFERENCES MuscleGroups(Id)
                )
                ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

            string workoutPlansQuery = @"
                CREATE TABLE IF NOT EXISTS WorkoutPlans
                (
                    Id INT PRIMARY KEY AUTO_INCREMENT,
                    PlanName VARCHAR(100) NOT NULL,
                    UserLogin VARCHAR(191) NOT NULL DEFAULT '',
                    TrainingDay VARCHAR(30),
                    ExerciseId INT NOT NULL,
                    SetsCount INT,
                    RepsCount INT,
                    RestSeconds INT,
                    FOREIGN KEY (ExerciseId) REFERENCES Exercises(Id)
                )
                ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

            string favoriteExercisesQuery = @"
                CREATE TABLE IF NOT EXISTS FavoriteExercises
                (
                    Id INT PRIMARY KEY AUTO_INCREMENT,
                    UserLogin VARCHAR(191) NOT NULL,
                    ExerciseId INT NOT NULL,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UNIQUE KEY UX_FavoriteExercises_User_Exercise (UserLogin, ExerciseId),
                    FOREIGN KEY (ExerciseId) REFERENCES Exercises(Id)
                )
                ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

            using (MySqlCommand command = new MySqlCommand(muscleGroupsQuery, connection))
                command.ExecuteNonQuery();

            using (MySqlCommand command = new MySqlCommand(exercisesQuery, connection))
                command.ExecuteNonQuery();

            using (MySqlCommand command = new MySqlCommand(workoutPlansQuery, connection))
                command.ExecuteNonQuery();

            using (MySqlCommand command = new MySqlCommand(favoriteExercisesQuery, connection))
                command.ExecuteNonQuery();

            EnsureWorkoutPlansUserColumn(connection);
            SeedMuscleGroupsIfEmpty(connection);
        }

        public List<Exercise> GetAll()
        {
            List<Exercise> exercises = new List<Exercise>();

            using MySqlConnection connection = new MySqlConnection(DBConnection.ConnectionString);
            connection.Open();

            string query = @"
                SELECT 
                    Exercises.Id,
                    Exercises.ExerciseName,
                    Exercises.MuscleGroupId,
                    MuscleGroups.GroupName,
                    Exercises.DifficultyLevel,
                    Exercises.Equipment
                FROM Exercises
                INNER JOIN MuscleGroups ON Exercises.MuscleGroupId = MuscleGroups.Id
                ORDER BY Exercises.Id;";

            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int muscleGroupId = reader.GetInt32("MuscleGroupId");
                string muscleGroupName = reader.GetString("GroupName");

                exercises.Add(new Exercise
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("ExerciseName"),
                    MuscleGroupId = muscleGroupId,
                    MuscleGroupName = muscleGroupName,
                    DifficultyLevel = GetNullableString(reader, "DifficultyLevel"),
                    Equipment = GetNullableString(reader, "Equipment"),
                    IconKey = GetIconKey(muscleGroupName),
                    DisplayOrder = reader.GetInt32("Id")
                });
            }

            return exercises;
        }

        public List<int> GetFavoriteExerciseIds(string userLogin)
        {
            List<int> favoriteIds = new List<int>();
            string normalizedUserLogin = NormalizeUserLogin(userLogin);

            if (normalizedUserLogin == "")
                return favoriteIds;

            using MySqlConnection connection = new MySqlConnection(DBConnection.ConnectionString);
            connection.Open();

            string query = @"
                SELECT ExerciseId
                FROM FavoriteExercises
                WHERE UserLogin = @userLogin;";

            using MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userLogin", normalizedUserLogin);

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                favoriteIds.Add(reader.GetInt32("ExerciseId"));
            }

            return favoriteIds;
        }

        public List<Exercise> GetFavoriteExercises(string userLogin)
        {
            List<Exercise> exercises = new List<Exercise>();
            string normalizedUserLogin = NormalizeUserLogin(userLogin);

            if (normalizedUserLogin == "")
                return exercises;

            using MySqlConnection connection = new MySqlConnection(DBConnection.ConnectionString);
            connection.Open();

            string query = @"
                SELECT 
                    Exercises.Id,
                    Exercises.ExerciseName,
                    Exercises.MuscleGroupId,
                    MuscleGroups.GroupName,
                    Exercises.DifficultyLevel,
                    Exercises.Equipment
                FROM FavoriteExercises
                INNER JOIN Exercises ON FavoriteExercises.ExerciseId = Exercises.Id
                INNER JOIN MuscleGroups ON Exercises.MuscleGroupId = MuscleGroups.Id
                WHERE FavoriteExercises.UserLogin = @userLogin
                ORDER BY FavoriteExercises.CreatedAt DESC, Exercises.ExerciseName;";

            using MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userLogin", normalizedUserLogin);

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                exercises.Add(ReadExercise(reader));
            }

            return exercises;
        }

        public void SetFavorite(string userLogin, int exerciseId, bool isFavorite)
        {
            string normalizedUserLogin = NormalizeUserLogin(userLogin);

            if (normalizedUserLogin == "")
                throw new InvalidOperationException("User login is empty.");

            using MySqlConnection connection = new MySqlConnection(DBConnection.ConnectionString);
            connection.Open();

            if (isFavorite)
            {
                string insertQuery = @"
                    INSERT IGNORE INTO FavoriteExercises
                    (UserLogin, ExerciseId)
                    VALUES
                    (@userLogin, @exerciseId);";

                using MySqlCommand command = new MySqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@userLogin", normalizedUserLogin);
                command.Parameters.AddWithValue("@exerciseId", exerciseId);
                command.ExecuteNonQuery();
                return;
            }

            string deleteQuery = @"
                DELETE FROM FavoriteExercises
                WHERE UserLogin = @userLogin
                AND ExerciseId = @exerciseId;";

            using MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, connection);
            deleteCommand.Parameters.AddWithValue("@userLogin", normalizedUserLogin);
            deleteCommand.Parameters.AddWithValue("@exerciseId", exerciseId);
            deleteCommand.ExecuteNonQuery();
        }

        public List<WorkoutPlanEntry> GetWorkoutPlan(string userLogin, string trainingDay)
        {
            List<WorkoutPlanEntry> workoutPlan = new List<WorkoutPlanEntry>();
            string normalizedUserLogin = NormalizeUserLogin(userLogin);

            if (normalizedUserLogin == "")
                return workoutPlan;

            using MySqlConnection connection = new MySqlConnection(DBConnection.ConnectionString);
            connection.Open();

            string query = @"
                SELECT
                    WorkoutPlans.Id,
                    WorkoutPlans.PlanName,
                    WorkoutPlans.TrainingDay,
                    WorkoutPlans.SetsCount,
                    WorkoutPlans.RepsCount,
                    WorkoutPlans.RestSeconds,
                    Exercises.Id AS ExerciseId,
                    Exercises.ExerciseName,
                    Exercises.MuscleGroupId,
                    MuscleGroups.GroupName,
                    Exercises.DifficultyLevel,
                    Exercises.Equipment
                FROM WorkoutPlans
                INNER JOIN Exercises ON WorkoutPlans.ExerciseId = Exercises.Id
                INNER JOIN MuscleGroups ON Exercises.MuscleGroupId = MuscleGroups.Id
                WHERE WorkoutPlans.UserLogin = @userLogin
                AND WorkoutPlans.TrainingDay = @trainingDay
                ORDER BY WorkoutPlans.Id;";

            using MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userLogin", normalizedUserLogin);
            command.Parameters.AddWithValue("@trainingDay", trainingDay.Trim());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Exercise exercise = new Exercise
                {
                    Id = reader.GetInt32("ExerciseId"),
                    Name = reader.GetString("ExerciseName"),
                    MuscleGroupId = reader.GetInt32("MuscleGroupId"),
                    MuscleGroupName = reader.GetString("GroupName"),
                    DifficultyLevel = GetNullableString(reader, "DifficultyLevel"),
                    Equipment = GetNullableString(reader, "Equipment"),
                    IconKey = GetIconKey(reader.GetString("GroupName")),
                    DisplayOrder = reader.GetInt32("ExerciseId")
                };

                workoutPlan.Add(new WorkoutPlanEntry
                {
                    Id = reader.GetInt32("Id"),
                    PlanName = reader.GetString("PlanName"),
                    TrainingDay = GetNullableString(reader, "TrainingDay"),
                    SetsCount = GetNullableInt(reader, "SetsCount"),
                    RepsCount = GetNullableInt(reader, "RepsCount"),
                    RestSeconds = GetNullableInt(reader, "RestSeconds"),
                    Exercise = exercise
                });
            }

            return workoutPlan;
        }

        public bool AddWorkoutPlanExercise(string userLogin, string trainingDay, int exerciseId)
        {
            string normalizedUserLogin = NormalizeUserLogin(userLogin);
            string normalizedTrainingDay = trainingDay.Trim();

            if (normalizedUserLogin == "")
                throw new InvalidOperationException("User login is empty.");

            if (normalizedTrainingDay == "")
                throw new InvalidOperationException("Training day is empty.");

            using MySqlConnection connection = new MySqlConnection(DBConnection.ConnectionString);
            connection.Open();

            string existsQuery = @"
                SELECT COUNT(*)
                FROM WorkoutPlans
                WHERE UserLogin = @userLogin
                AND TrainingDay = @trainingDay
                AND ExerciseId = @exerciseId;";

            using (MySqlCommand existsCommand = new MySqlCommand(existsQuery, connection))
            {
                existsCommand.Parameters.AddWithValue("@userLogin", normalizedUserLogin);
                existsCommand.Parameters.AddWithValue("@trainingDay", normalizedTrainingDay);
                existsCommand.Parameters.AddWithValue("@exerciseId", exerciseId);

                if (Convert.ToInt32(existsCommand.ExecuteScalar()) > 0)
                    return false;
            }

            string insertQuery = @"
                INSERT INTO WorkoutPlans
                (PlanName, UserLogin, TrainingDay, ExerciseId, SetsCount, RepsCount, RestSeconds)
                VALUES
                (@planName, @userLogin, @trainingDay, @exerciseId, 3, 10, 60);";

            using MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@planName", $"{normalizedTrainingDay} workout");
            insertCommand.Parameters.AddWithValue("@userLogin", normalizedUserLogin);
            insertCommand.Parameters.AddWithValue("@trainingDay", normalizedTrainingDay);
            insertCommand.Parameters.AddWithValue("@exerciseId", exerciseId);
            insertCommand.ExecuteNonQuery();

            return true;
        }

        public void RemoveWorkoutPlanExercise(string userLogin, int workoutPlanId)
        {
            string normalizedUserLogin = NormalizeUserLogin(userLogin);

            if (normalizedUserLogin == "")
                throw new InvalidOperationException("User login is empty.");

            using MySqlConnection connection = new MySqlConnection(DBConnection.ConnectionString);
            connection.Open();

            string deleteQuery = @"
                DELETE FROM WorkoutPlans
                WHERE Id = @id
                AND UserLogin = @userLogin;";

            using MySqlCommand command = new MySqlCommand(deleteQuery, connection);
            command.Parameters.AddWithValue("@id", workoutPlanId);
            command.Parameters.AddWithValue("@userLogin", normalizedUserLogin);
            command.ExecuteNonQuery();
        }

        public List<MuscleGroup> GetMuscleGroups()
        {
            List<MuscleGroup> muscleGroups = new List<MuscleGroup>();

            using MySqlConnection connection = new MySqlConnection(DBConnection.ConnectionString);
            connection.Open();

            using MySqlCommand command = new MySqlCommand("SELECT Id, GroupName FROM MuscleGroups ORDER BY Id", connection);
            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                muscleGroups.Add(new MuscleGroup
                {
                    Id = reader.GetInt32("Id"),
                    GroupName = reader.GetString("GroupName")
                });
            }

            return muscleGroups;
        }

        public void Add(string name, int muscleGroupId, string difficultyLevel, string equipment)
        {
            using MySqlConnection connection = new MySqlConnection(DBConnection.ConnectionString);
            connection.Open();

            string query = @"
                INSERT INTO Exercises
                (ExerciseName, MuscleGroupId, DifficultyLevel, Equipment)
                VALUES
                (@name, @muscleGroupId, @difficultyLevel, @equipment);";

            using MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", name.Trim());
            command.Parameters.AddWithValue("@muscleGroupId", muscleGroupId);
            command.Parameters.AddWithValue("@difficultyLevel", EmptyToNull(difficultyLevel));
            command.Parameters.AddWithValue("@equipment", EmptyToNull(equipment));
            command.ExecuteNonQuery();
        }

        public void Update(int id, string name, int muscleGroupId, string difficultyLevel, string equipment)
        {
            using MySqlConnection connection = new MySqlConnection(DBConnection.ConnectionString);
            connection.Open();

            string query = @"
                UPDATE Exercises
                SET ExerciseName = @name,
                    MuscleGroupId = @muscleGroupId,
                    DifficultyLevel = @difficultyLevel,
                    Equipment = @equipment
                WHERE Id = @id;";

            using MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@name", name.Trim());
            command.Parameters.AddWithValue("@muscleGroupId", muscleGroupId);
            command.Parameters.AddWithValue("@difficultyLevel", EmptyToNull(difficultyLevel));
            command.Parameters.AddWithValue("@equipment", EmptyToNull(equipment));
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using MySqlConnection connection = new MySqlConnection(DBConnection.ConnectionString);
            connection.Open();

            using MySqlTransaction transaction = connection.BeginTransaction();

            try
            {
                using (MySqlCommand deletePlansCommand = new MySqlCommand("DELETE FROM WorkoutPlans WHERE ExerciseId = @id", connection, transaction))
                {
                    deletePlansCommand.Parameters.AddWithValue("@id", id);
                    deletePlansCommand.ExecuteNonQuery();
                }

                using (MySqlCommand deleteFavoritesCommand = new MySqlCommand("DELETE FROM FavoriteExercises WHERE ExerciseId = @id", connection, transaction))
                {
                    deleteFavoritesCommand.Parameters.AddWithValue("@id", id);
                    deleteFavoritesCommand.ExecuteNonQuery();
                }

                using (MySqlCommand deleteExerciseCommand = new MySqlCommand("DELETE FROM Exercises WHERE Id = @id", connection, transaction))
                {
                    deleteExerciseCommand.Parameters.AddWithValue("@id", id);
                    deleteExerciseCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private void SeedMuscleGroupsIfEmpty(MySqlConnection connection)
        {
            using MySqlCommand countCommand = new MySqlCommand("SELECT COUNT(*) FROM MuscleGroups", connection);
            int count = Convert.ToInt32(countCommand.ExecuteScalar());

            if (count > 0)
                return;

            string[] groups = { "Chest", "Back", "Legs", "Shoulders", "Arms" };

            foreach (string group in groups)
            {
                using MySqlCommand command = new MySqlCommand("INSERT INTO MuscleGroups (GroupName) VALUES (@groupName)", connection);
                command.Parameters.AddWithValue("@groupName", group);
                command.ExecuteNonQuery();
            }
        }

        private void EnsureWorkoutPlansUserColumn(MySqlConnection connection)
        {
            string columnExistsQuery = @"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                AND TABLE_NAME = 'WorkoutPlans'
                AND COLUMN_NAME = 'UserLogin';";

            using MySqlCommand existsCommand = new MySqlCommand(columnExistsQuery, connection);

            if (Convert.ToInt32(existsCommand.ExecuteScalar()) > 0)
                return;

            using MySqlCommand alterCommand = new MySqlCommand(
                "ALTER TABLE WorkoutPlans ADD COLUMN UserLogin VARCHAR(191) NOT NULL DEFAULT '' AFTER PlanName;",
                connection
            );
            alterCommand.ExecuteNonQuery();
        }

        private Exercise ReadExercise(MySqlDataReader reader)
        {
            int muscleGroupId = reader.GetInt32("MuscleGroupId");
            string muscleGroupName = reader.GetString("GroupName");

            return new Exercise
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("ExerciseName"),
                MuscleGroupId = muscleGroupId,
                MuscleGroupName = muscleGroupName,
                DifficultyLevel = GetNullableString(reader, "DifficultyLevel"),
                Equipment = GetNullableString(reader, "Equipment"),
                IconKey = GetIconKey(muscleGroupName),
                DisplayOrder = reader.GetInt32("Id")
            };
        }

        private string GetNullableString(MySqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal);
        }

        private int GetNullableInt(MySqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
        }

        private object EmptyToNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
        }

        private string NormalizeUserLogin(string userLogin)
        {
            return userLogin.Trim();
        }

        private string GetIconKey(string muscleGroupName)
        {
            return muscleGroupName.Trim().ToLowerInvariant() switch
            {
                "chest" => "icon1",
                "back" => "icon2",
                "legs" => "icon3",
                "shoulders" => "icon4",
                "arms" => "icon5",
                _ => "icon1"
            };
        }
    }
}
