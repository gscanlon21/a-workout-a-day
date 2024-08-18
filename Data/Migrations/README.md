# Migrations

Add-Migration SquashMigrations -Project Data

Remove-Migration -Project Data

Update-Database -Project Data


## Squash Migrations

- Delete all migration files including the CoreContextModelSnapshot.cs
- Add a new migration to generate a migration for the entire database.
- Delete all old migrations from the __EFMigrationsHistory table.
- Add the new migration to the __EFMigrationsHistory table.
- Update the database and check no migrations were applied.