using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InfinityLife.Models
{
    public class EmployeeRole
    {
        [Key]
        public int EmpRoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string? EmpRole { get; set; }
    }
}
