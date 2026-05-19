namespace PracticaApp.Properties
{
    internal sealed class MuscleGroup
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = "";

        public override string ToString()
        {
            return GroupName;
        }
    }
}
