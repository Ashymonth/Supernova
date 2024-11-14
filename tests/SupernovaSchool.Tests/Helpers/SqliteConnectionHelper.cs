using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace SupernovaSchool.Tests.Helpers;

internal static class SqliteConnectionHelper
{
    public static DbConnection CreateConnection()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        return connection;
    }
}