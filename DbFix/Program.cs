using System;
using System.Threading.Tasks;
using Npgsql;

namespace DbFix
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string connString = "Host=aws-1-ap-northeast-2.pooler.supabase.com;Database=postgres;Username=postgres.fchhhvcaeoaiqqypdiyc;Password=Team5_SWP391@123;SSL Mode=Require;Trust Server Certificate=true;Include Error Detail=true";

            try
            {
                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                // Get all check constraints for the club table
                var cmd = new NpgsqlCommand("SELECT conname, pg_get_constraintdef(c.oid) FROM pg_constraint c JOIN pg_namespace n ON n.oid = c.connamespace WHERE conrelid = 'club'::regclass;", conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                
                string constraintToDrop = null;
                while (await reader.ReadAsync())
                {
                    Console.WriteLine($"Constraint: {reader.GetString(0)} -> {reader.GetString(1)}");
                    if (reader.GetString(0).Contains("status") || reader.GetString(1).Contains("status"))
                    {
                        constraintToDrop = reader.GetString(0);
                    }
                }
                await reader.CloseAsync();

                if (constraintToDrop != null)
                {
                    Console.WriteLine($"Dropping constraint {constraintToDrop}...");
                    var dropCmd = new NpgsqlCommand($"ALTER TABLE club DROP CONSTRAINT {constraintToDrop};", conn);
                    await dropCmd.ExecuteNonQueryAsync();

                    Console.WriteLine("Adding new constraint to allow 'Giải thể'...");
                    var addCmd = new NpgsqlCommand($"ALTER TABLE club ADD CONSTRAINT {constraintToDrop} CHECK ((status)::text = ANY ((ARRAY['Đang hoạt động'::character varying, 'Tạm dừng'::character varying, 'Giải thể'::character varying])::text[]));", conn);
                    await addCmd.ExecuteNonQueryAsync();
                    Console.WriteLine("Successfully updated constraint!");
                }
                else 
                {
                    Console.WriteLine("Could not find status constraint on club table.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
