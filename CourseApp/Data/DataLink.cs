using Microsoft.Data.SqlClient;

namespace CourseApp.Data
{
    public class DataLink
    {
        private static readonly string connectionString = "Data Source= DESKTOP-AR22U8G ;Initial Catalog=CourseApp;Integrated Security=True;TrustServerCertificate=True";
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
