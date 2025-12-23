namespace ITTicketSystem.Models
{
    public class GridHeaderStatusVM
    {
        public string Label { get; }
        public string? CurrentSort { get; }
        public string? CurrentDir { get; }

        public GridHeaderStatusVM(string label, string? currentSort, string? currentDir)
        {
            Label = label;
            CurrentSort = currentSort;
            CurrentDir = currentDir;
        }

        public bool IsSortActive(string sortKey, string dir)
            => string.Equals(CurrentSort, sortKey, System.StringComparison.OrdinalIgnoreCase)
               && string.Equals(CurrentDir, dir, System.StringComparison.OrdinalIgnoreCase);
    }
}
