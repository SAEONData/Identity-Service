Add-Migration -Name "InitialCreate" -OutputDir "Data\Migrations\Configuration" -Context ConfigurationDbContext
Add-Migration -Name "InitialCreate" -OutputDir "Data\Migrations\PersistedGrant" -Context PersistedGrantDbContext
Add-Migration -Name "InitialCreate" -OutputDir "Data\Migrations\Identity" -Context SAEONDbContext

Update-Database -Context ConfigurationDbContext
Update-Database -Context PersistedGrantDbContext
Update-Database -Context SAEONDbContext
