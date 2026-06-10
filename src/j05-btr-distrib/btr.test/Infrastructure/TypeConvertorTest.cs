using System;
using System.Data;
using btr.nuna.Infrastructure;
using Xunit;

namespace btr.test.Infrastructure
{
    public class TypeConvertorTest
    {
        [Fact]
        public void ToDbType_SqlDbTypeDate_ReturnsDbTypeDate()
        {
            Assert.Equal(DbType.Date, TypeConvertor.ToDbType(SqlDbType.Date));
        }

        [Fact]
        public void ToNetType_SqlDbTypeDate_ReturnsDateTime()
        {
            Assert.Equal(typeof(DateTime), TypeConvertor.ToNetType(SqlDbType.Date));
        }

        [Fact]
        public void ToSqlDbType_DbTypeDate_ReturnsSqlDbTypeDate()
        {
            Assert.Equal(SqlDbType.Date, TypeConvertor.ToSqlDbType(DbType.Date));
        }

        [Fact]
        public void ToSqlDbType_DateTimeType_ReturnsSqlDbTypeDateTime()
        {
            Assert.Equal(SqlDbType.DateTime, TypeConvertor.ToSqlDbType(typeof(DateTime)));
        }
    }
}
