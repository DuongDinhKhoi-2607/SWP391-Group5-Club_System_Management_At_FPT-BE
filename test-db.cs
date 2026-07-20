using System;
using Npgsql;
try {
    string connString = "Host=aws-1-ap-northeast-2.pooler.supabase.com;Database=postgres;Username=postgres.fchhhvcaeoaiqqypdiyc;Password=Team5_SWP391@123;SSL Mode=Require;Trust Server Certificate=true";
    using var conn = new NpgsqlConnection(connString);
    conn.Open();
    Console.WriteLine("Connected!");
    using var cmd = new NpgsqlCommand("INSERT INTO participant (eventid, userid, roleinevent, attendancestatus) VALUES (1, 3, 'Thành viên tham gia', 'Ðã dang ký');", conn);
    cmd.ExecuteNonQuery();
    Console.WriteLine("Inserted!");
} catch (Exception ex) {
    Console.WriteLine(ex.ToString());
}
