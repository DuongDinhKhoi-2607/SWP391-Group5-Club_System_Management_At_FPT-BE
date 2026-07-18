using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string connString = "Host=aws-1-ap-northeast-2.pooler.supabase.com;Database=postgres;Username=postgres.fchhhvcaeoaiqqypdiyc;Password=Team5_SWP391@123;SSL Mode=Require;Trust Server Certificate=true";
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        using var cmdAlter = new NpgsqlCommand(@"
            ALTER TABLE membership DROP CONSTRAINT IF EXISTS ck_membership_status;
            ALTER TABLE membership ADD CONSTRAINT ck_membership_status 
            CHECK (status IN ('Đang sinh hoạt', 'Đã rút lui', 'Chờ kích hoạt', 'Bị khóa'));
        ", conn);
        cmdAlter.ExecuteNonQuery();
        Console.WriteLine("Successfully updated ck_membership_status constraint!");
    }
}
