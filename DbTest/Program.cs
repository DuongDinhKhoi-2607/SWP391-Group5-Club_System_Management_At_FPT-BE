using System;
using Npgsql;
string connString = "Host=aws-1-ap-northeast-2.pooler.supabase.com;Database=postgres;Username=postgres.fchhhvcaeoaiqqypdiyc;Password=Team5_SWP391@123;SSL Mode=Require;Trust Server Certificate=true";
try {
    using var conn = new NpgsqlConnection(connString);
    conn.Open();
    using var cmd = new NpgsqlCommand("SELECT pg_get_constraintdef(oid) FROM pg_constraint WHERE conname = 'ck_participant_attendancestatus';", conn);
    using var reader = cmd.ExecuteReader();
    while(reader.Read()) {
        Console.WriteLine(reader.GetString(0));
    }
} catch (Exception ex) {
    Console.WriteLine(ex.Message);
}
