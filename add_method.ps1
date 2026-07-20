$content = Get-Content "e:\SWP391\BE_Merge\BussinessLayer\Services\UserService.cs" -Raw
$newMethod = "
    public async Task ChangePasswordAsync(long userId, ChangePasswordDto dto)
    {
        var user = await _repo.GetUserByIdAsync(userId);
        if (user == null)
            throw new Exception(\"Không tìm th?y ngu?i dùng.\");

        if (user.Passwordhash != HashSha256(dto.OldPassword))
            throw new Exception(\"M?t kh?u cu không chính xác.\");

        user.Passwordhash = HashSha256(dto.NewPassword);
        await _repo.UpdateUserAsync(user);
    }
"
$content = $content.Replace("private static string HashSha256(string input)", $newMethod + "
    private static string HashSha256(string input)")
Set-Content -Path "e:\SWP391\BE_Merge\BussinessLayer\Services\UserService.cs" -Value $content -Encoding UTF8
