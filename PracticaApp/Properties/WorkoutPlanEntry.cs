namespace PracticaApp.Properties
{
    internal sealed class WorkoutPlanEntry
    {
        public int Id { get; set; }
        public string PlanName { get; set; } = "";
        public string TrainingDay { get; set; } = "";
        public int SetsCount { get; set; }
        public int RepsCount { get; set; }
        public int RestSeconds { get; set; }
        public Exercise Exercise { get; set; } = new Exercise();

        public string DisplayText
        {
            get
            {
                string exerciseName = Exercise.Name;

                if (SetsCount <= 0 || RepsCount <= 0)
                    return exerciseName;

                return $"{exerciseName}  -  {SetsCount}x{RepsCount}";
            }
        }
    }
}
