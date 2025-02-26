using System.ComponentModel.DataAnnotations;
namespace InfinityLife.Models
{
    public class TimeSheet
    {
        public int TimeSheetId { get; set; }
        public string EmployeeId { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0, 168)] // Maximum hours in a week
        public decimal TotalHours { get; set; }

        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; }
        public List<TimeSheetEntry> Entries { get; set; }
    }

    public class TimeSheetEntry
    {
        public int EntryId { get; set; }
        public int TimeSheetId { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EntryDate { get; set; }

        [Required]
        public string EntryType { get; set; } // "Project" or "Learning"

        public string ProjectName { get; set; }
        public string LearningName { get; set; }

        [Required]
        public string StatusMessage { get; set; }

        [Required]
        [Range(0, 24)]
        public decimal Hours { get; set; }
    }

    public class TimeSheetViewModel
    {
        public TimeSheet TimeSheet { get; set; }
        public List<TimeSheetEntry> Entries { get; set; }
        public List<DateTime> WeekendDates { get; set; }
        public decimal TotalHoursLogged => Entries?.Sum(e => e.Hours) ?? 0;
        public bool IsComplete => TotalHoursLogged >= 45;
    }
    public class EmployeeTimeSheetViewModel
    {
        public TimeSheet TimeSheet { get; set; }
        public List<TimeSheetEntry> Entries { get; set; }
        public List<DateTime> WeekendDates { get; set; }
        public decimal TotalHoursLogged => Entries?.Sum(e => e.Hours) ?? 0;
        public bool IsComplete => TotalHoursLogged >= 45;
    }
    public class TimeSheetStatusUpdateModel
    {
        public int TimesheetId { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
    }
}