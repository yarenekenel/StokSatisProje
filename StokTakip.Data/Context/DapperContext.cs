using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace StokTakip.Data.Context;

public sealed class DapperContext : IDapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("StokSatisDb")
            ?? throw new InvalidOperationException("StokSatisDb bağlantı dizesi appsettings.json içinde bulunamadı.");
    }

    public IDbConnection CreateConnection()
        => new SqlConnection(_connectionString);
}