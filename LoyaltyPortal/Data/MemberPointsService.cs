using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using Dapper; 
namespace LoyaltyPortal.Data
{
    public class MemberPointsService
    {
        private readonly IConfiguration _configuration;

        public MemberPointsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<MemberReport> GetCustomerPointsReport(string customerID, DateTime? archiveDate)
        {
            string connectionString = _configuration.GetConnectionString("LoyaltyDatabase");
            string archiveDatesQuery = _configuration["Queries:ArchiveDatesQuery"];
            string maxDateQuery = _configuration["Queries:MaxDateQuery"];
            string interactionQuery = _configuration["Queries:InteractionQuery"];

            MemberReport report = new MemberReport();

            using (var conn = new SqlConnection(connectionString))
            {
               

                try
                {

                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    // Using parameterized queries for security
                    var maxDate = (await conn.QueryAsync<DateTime>(maxDateQuery, new { CustomerID = customerID })).FirstOrDefault();
                    if (maxDate < archiveDate)
                    {
                        archiveDate = maxDate;
                    }

                    report.ArchiveDates = (await conn.QueryAsync<DateTime>(archiveDatesQuery, new { CustomerID = customerID })).ToList();
                    report.Interactions = (await conn.QueryAsync<Interaction>(interactionQuery, new { CustomerID = customerID, MaxDate = maxDate })).ToList();
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }

            return report;
        }
    }

    public record MemberReport {

        public int CustomerID { get; set; }
        public int AvailablePoints { get; set; }
        public int LifetimePoints { get; set; }
        public int RedeemedPoints { get; set; }


        public List<DateTime> ArchiveDates { get; set; }  
        public List<Interaction> Interactions { get; set; }
        public List<InvoiceItem> InvoiceItems { get; set;  }


    }

    public record Interaction {
        public double Number { get; set; }
        public string Date { get; set; }
        public string Name { get; set; }
        public string UserID { get; set; }
        public string Email { get; set; }
        public double ActionID { get; set; }
        public string Actions { get; set; }
        public string OrderID { get; set; }
        public string Coupon { get; set; }
        public double Points { get; set; }
        public string PointExpirationDate { get; set; }
        public string Reason { get; set; }
        public string OptInStatus { get; set; }
        public DateTime TransDate { get; set; }
    }
    public record InvoiceItem { 
        public string OrderID { get; set; }
    }
}
