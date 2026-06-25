using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string connString = "Host=aws-1-ap-northeast-2.pooler.supabase.com;Database=postgres;Username=postgres.fchhhvcaeoaiqqypdiyc;Password=Team5_SWP391@123;SSL Mode=Require;Trust Server Certificate=true";
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT pg_get_constraintdef(oid) FROM pg_constraint WHERE conname = 'ck_user_systemrole'", conn);
        var result = cmd.ExecuteScalar();
        Console.WriteLine($"SystemRole constraint: {result}");

        using var cmd2 = new NpgsqlCommand("SELECT pg_get_constraintdef(oid) FROM pg_constraint WHERE conname = 'ck_user_status'", conn);
        var result2 = cmd2.ExecuteScalar();
        Console.WriteLine($"Status constraint: {result2}");
    }
}
