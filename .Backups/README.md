# Backups

./pg_dump.exe --exclude-table-data 'public.user*' --exclude-table-data 'public.newsletter*' --host [URL] --username [USERNAME] --file [FILENAME.sql] [DBNAME]

## MAKE SURE THERE IS NO PII (PERSONALLY IDENTIFYABLE INFORMATION) IN THE BACKUP