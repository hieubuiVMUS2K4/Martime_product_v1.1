using Npgsql;

var connectionString = "Host=localhost;Port=5433;Database=maritime_edge;Username=edge_user;Password=ChangeMe_EdgePassword123!";

var sql = @"
-- Đảm bảo có role USER
INSERT INTO roles (role_name, role_code, description)
VALUES ('User', 'USER', 'Regular crew member')
ON CONFLICT (role_code) DO NOTHING;

-- Tạo users từ crew_members
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at)
SELECT 
    cm.crew_id,
    encode(digest(
        COALESCE(
            to_char(cm.date_of_birth, 'DDMMYYYY'),
            '123456'
        ), 'sha256'
    ), 'base64'),
    (SELECT id FROM roles WHERE role_code = 'USER'),
    cm.crew_id,
    true,
    NOW()
FROM crew_members cm
WHERE NOT EXISTS (SELECT 1 FROM users WHERE username = cm.crew_id);

-- Verify
SELECT 
    u.username,
    cm.full_name,
    cm.rank,
    cm.position,
    COALESCE(to_char(cm.date_of_birth, 'DDMMYYYY'), '123456') as default_password,
    r.role_name,
    u.is_active
FROM users u
JOIN crew_members cm ON u.crew_id = cm.crew_id
LEFT JOIN roles r ON u.role_id = r.id
ORDER BY u.username;
";

try
{
    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    
    Console.WriteLine("✅ Connected to database");
    
    await using var cmd = new NpgsqlCommand(sql, connection);
    await using var reader = await cmd.ExecuteReaderAsync();
    
    Console.WriteLine("\n📋 Created Users:");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine($"{"Username",-12} {"Full Name",-25} {"Position",-15} {"Password",-12} {"Active"}");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    
    while (await reader.ReadAsync())
    {
        var username = reader.GetString(0);
        var fullName = reader.GetString(1);
        var position = reader.IsDBNull(3) ? "" : reader.GetString(3);
        var password = reader.GetString(4);
        var active = reader.GetBoolean(6);
        
        Console.WriteLine($"{username,-12} {fullName,-25} {position,-15} {password,-12} {active}");
    }
    
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine("\n✅ Users created successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    return 1;
}

return 0;
