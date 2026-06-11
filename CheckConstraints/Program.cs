using Npgsql;

var connStr = "Host=aws-1-ap-northeast-2.pooler.supabase.com;Database=postgres;Username=postgres.fchhhvcaeoaiqqypdiyc;Password=Team5_SWP391@123;SSL Mode=Require;Trust Server Certificate=true;Include Error Detail=true";

await using var conn = new NpgsqlConnection(connStr);
await conn.OpenAsync();

// Query 1: Xem constraint definition
Console.WriteLine("=== ck_semester_status constraint ===");
await using (var cmd = new NpgsqlCommand(
    "SELECT pg_get_constraintdef(oid) FROM pg_constraint WHERE conname = 'ck_semester_status'", conn))
await using (var reader = await cmd.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
        Console.WriteLine(reader.GetString(0));
}

// Query 2: Xem constraint của reportperiod
Console.WriteLine("\n=== ck_reportperiod_status constraint ===");
await using (var cmd2 = new NpgsqlCommand(
    "SELECT pg_get_constraintdef(oid) FROM pg_constraint WHERE conname = 'ck_reportperiod_status'", conn))
await using (var reader2 = await cmd2.ExecuteReaderAsync())
{
    while (await reader2.ReadAsync())
        Console.WriteLine(reader2.GetString(0));
}
