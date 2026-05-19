namespace PracticaApp.Properties
{
    internal sealed class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int MuscleGroupId { get; set; }
        public string MuscleGroupName { get; set; } = "";
        public string DifficultyLevel { get; set; } = "";
        public string Equipment { get; set; } = "";
        public string IconKey { get; set; } = "icon1";
        public int DisplayOrder { get; set; }
    }
}
