using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfinityLife.Models
{
    public class LeaveBalance
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; }
        public int TotalLeaves { get; set; } = 10; 
        
        public int RemainingLeaves { get; set; }
        public int Year {  get; set; }
    }
}
