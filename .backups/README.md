# Backups

```powershell
 ./pg_dump.exe --no-privileges --no-owner --exclude-table-data 'public.user*' --host [URL] --port [PORT] --username [USERNAME] --file [FILENAME.sql] [DBNAME]
```

## MAKE SURE THERE IS NO PII (PERSONALLY IDENTIFIABLE INFORMATION) IN THE BACKUP