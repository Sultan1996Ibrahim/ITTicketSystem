namespace ITTicketSystem.Models
{
    public class GridHeaderVM
    {
        public string Label { get; }
        public string Param { get; }
        public string? CurrentSort { get; }
        public string? CurrentDir { get; }

        public GridHeaderVM(string label, string param, string? currentSort, string? currentDir)
        {
            Label = label;
            Param = param;
            CurrentSort = currentSort;
            CurrentDir = currentDir;
        }

        public bool IsSortActive(string dir)
            => string.Equals(CurrentSort, Param, System.StringComparison.OrdinalIgnoreCase)
               && string.Equals(CurrentDir, dir, System.StringComparison.OrdinalIgnoreCase);
    }
}
