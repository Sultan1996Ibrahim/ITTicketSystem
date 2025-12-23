namespace ITTicketSystem.Models
{
    public enum TicketStatus
    {
        New = 0,                 // تذكرة جديدة
        AssignedToDepartment = 1,// المدير وافق وأسندها لقسم/موظف
        InProgress = 2,          // الموظف بدأ يشتغل عليها
        Closed = 3               // التذكرة أُغلقت
    }
}
