using BtrGudang.Helper.Common;
using BtrGudang.Winform.Helper;
using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BtrGudang.Helper.Common
{
    public static class DapperHelper
    {
        public static void AddParam(this DynamicParameters cmd, string param, object value, SqlDbType type)
        {
            DbType dbType = TypeConvertor.ToDbType(type);
            if (dbType == DbType.AnsiString)
            {
                var length = value.ToString().Length;
                cmd.Add(param, value, dbType, ParameterDirection.Input, length);
            }
            else
            {
                cmd.Add(param, value, dbType, ParameterDirection.Input);
            }
        }

        public static IEnumerable<T> Read<T>(this IDbConnection conn, string sql, DynamicParameters param = null)
        {
            var result = conn.Query<T>(sql, param);
            if (result.Any())
                return result;
            else
                return default;
        }
        public static T ReadSingle<T>(this IDbConnection conn, string sql, DynamicParameters param)
        {
            return conn.QueryFirstOrDefault<T>(sql, param);
        }

        public static int InsertBulk<T>(this IDbConnection conn, string sql, IEnumerable<T> listData)
        {
            return conn.Execute(sql, listData);
        }

        public static void Execute(this string sql, string connString, DynamicParameters param = null)
        {
            using (var conn = new SqlConnection(connString))
                conn.Execute(sql, param);
        }

        public static T ReadSingle<T>(this string sql, string connString, DynamicParameters param = null)
        {
            using (var conn = new SqlConnection(connString))
                return conn.ReadSingle<T>(sql, param);
        }

        public static IEnumerable<T> Read<T>(this string sql, string connString, DynamicParameters param = null)
        {
            using (var conn = new SqlConnection(connString))
                return conn.Read<T>(sql, param);
        }
        public static void AddMap(this SqlBulkCopy bcp, string source, string v)
        {
            var target = source;
            bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping(source, target));
        }
        public static DataTable AsDataTable<T>(this IEnumerable<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
