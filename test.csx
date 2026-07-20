using System;
using System.Data;
using Npgsql;
var connString = "Host=localhost;Database=club_system_db;Username=postgres;Password=123";
using var conn = new NpgsqlConnection(connString);
conn.Open();
using var cmd = new NpgsqlCommand("SELECT conname, pg_get_constraintdef(oid) FROM pg_constraint WHERE conrelid = 'participant'::regclass;", conn);
using var reader = cmd.ExecuteReader();
while(reader.Read()) {
    Console.WriteLine(reader.GetString(0) + :  + reader.GetString(1));
}
