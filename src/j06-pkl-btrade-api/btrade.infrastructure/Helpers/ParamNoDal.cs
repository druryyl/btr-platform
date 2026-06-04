using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Options;
using Nuna.Lib.AutoNumberHelper;
using Nuna.Lib.DataAccessHelper;

namespace btrade.infrastructure.Helpers;

public class ParamNoDal : INunaCounterDal, INunaCounterDecDal
{
    private readonly DatabaseOptions _opt;
    public ParamNoDal(IOptions<DatabaseOptions> opt)
    {
        _opt = opt.Value;
    }
    public string? GetNewHexNumber(string prefix)
    {
        const string sql = @"
            SELECT
                Prefix, HexVal
            FROM
                BTRADE_ParamNo
            WHERE
                Prefix = @Prefix ";

        var dp = new DynamicParameters();
        dp.AddParam("@Prefix", prefix, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        var dr = conn.ReadSingle<ParamNoDto>(sql, dp);
        return dr?.HexVal;
    }

    public void UpdateNewHexNumber(string prefix, string hexValue)
    {
        const string sql = @"
            UPDATE
                APTOL_ParamNo
            SET
                HexVal = @HexVal
            WHERE
                Prefix = @Prefix ";

        var dp = new DynamicParameters();
        dp.AddParam("@Prefix", prefix, SqlDbType.VarChar);
        dp.AddParam("@HexVal", hexValue, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void InsertNewHexNumber(string prefix, string hexValue)
    {


        const string sql = @"
            INSERT INTO 
                APTOL_ParamNo (
                    Prefix, HexVal)
            VALUES (
                    @Prefix, @HexVal)";

        var dp = new DynamicParameters();
        dp.AddParam("@Prefix", prefix, SqlDbType.VarChar);
        dp.AddParam("@HexVal", hexValue, SqlDbType.VarChar);


        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    private class ParamNoDto
    {
        public string Prefix { get; set; } = string.Empty;
        public string HexVal { get; set; } = string.Empty;
    }

    public long GetNewDecNumber(string prefix)
    {
        throw new NotImplementedException();
    }

    public void UpdateNewDecNumber(string prefix, long decValue)
    {
        throw new NotImplementedException();
    }

    public void InsertNewDecNumber(string prefix, long decValue)
    {
        throw new NotImplementedException();
    }
}