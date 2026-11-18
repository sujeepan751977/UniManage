$password = "Admin@123"
$salt = New-Object Byte[] 16
(New-Object System.Security.Cryptography.RNGCryptoServiceProvider).GetBytes($salt)
$pbkdf2 = New-Object System.Security.Cryptography.Rfc2898DeriveBytes($password, $salt, 1000)
$hash = $pbkdf2.GetBytes(32)
$bytes = New-Object Byte[] 49
$bytes[0] = 0
[System.Array]::Copy($salt, 0, $bytes, 1, 16)
[System.Array]::Copy($hash, 0, $bytes, 17, 32)
Write-Output ([System.Convert]::ToBase64String($bytes))
