using System.Data;

namespace StokTakip.Data.Context;

public interface IDapperContext
{
    IDbConnection CreateConnection();
}