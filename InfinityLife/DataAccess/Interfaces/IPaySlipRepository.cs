namespace InfinityLife.DataAccess.Interfaces
{
    public interface IPaySlipRepository
    {
        Task<PaySlip> CreatePaySlip(PaySlip paySlip);
        Task<PaySlip> GetPaySlipById(string employeeId, DateTime PayPeriod);
        Task<IEnumerable<PaySlip>> GetPaySlipsByEmployeeId(string employeeId);
        //Task<bool> MarkAsRead(int paySlipId);
    }
}
