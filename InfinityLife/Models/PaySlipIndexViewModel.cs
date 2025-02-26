namespace InfinityLife.Models
{
    public class PaySlipIndexViewModel
    {
        public IEnumerable<Employee> Employees { get; set; } = new List<Employee>();
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
    }
}
