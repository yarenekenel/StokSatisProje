using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public abstract class BaseRepository
{
    private readonly IDapperContext _context;

    protected BaseRepository(IDapperContext context)
    {
        _context = context;
    }

    protected async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null)
    {
        using var connection = (SqlConnection)_context.CreateConnection();
        var rows = await connection.QueryAsync<T>(sql, param);
        return rows.AsList();
    }

    protected async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
    {
        using var connection = (SqlConnection)_context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
    }

    protected async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        using var connection = (SqlConnection)_context.CreateConnection();
        return await connection.ExecuteAsync(sql, param);
    }

    protected async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null)
    {
        using var connection = (SqlConnection)_context.CreateConnection();
        return await connection.ExecuteScalarAsync<T>(sql, param);
    }
}