namespace CheckerBA.Infrastructure.MongoDB
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;

        // Fail fast when cluster cannot be reached to avoid 30s timeout per message.
        public int ServerSelectionTimeoutMs { get; set; } = 3000;
        public int ConnectTimeoutMs { get; set; } = 5000;
        public int SocketTimeoutMs { get; set; } = 5000;
    }
}