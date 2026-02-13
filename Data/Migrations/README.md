# Migrations

Add-Migration SquashMigrations -Project Data -Context CoreContext

Remove-Migration -Project Data -Context CoreContext

Update-Database -Project Data -Context CoreContext


## Squash Migrations

- Delete all migration files including the CoreContextModelSnapshot.cs
- Add a new migration to generate a migration for the entire database.
- Delete all old migrations from the __EFMigrationsHistory table.
- Add the new migration to the __EFMigrationsHistory table.
- Update the database and check no migrations were applied.