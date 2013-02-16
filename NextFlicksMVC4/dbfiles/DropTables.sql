-- disable all constraints
EXEC sp_msforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

-- delete data in all tables
EXEC sp_MSForEachTable "DELETE FROM ?"

-- enable all constraints
exec sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"

DBCC CHECKIDENT ( 'BoxArts', RESEED, 0)
DBCC CHECKIDENT ( 'Movies', RESEED, 0)
DBCC CHECKIDENT ( 'MovieTags', RESEED, 0)
DBCC CHECKIDENT ( 'MovieToGenres', RESEED, 0)
DBCC CHECKIDENT ( 'OmdbEntries', RESEED, 0)
DBCC CHECKIDENT ( 'UserLogs', RESEED, 0)
DBCC CHECKIDENT ( 'UserProfile', RESEED, 0)
DBCC CHECKIDENT ( 'UserToMovieToTags', RESEED, 0)
DBCC CHECKIDENT ( 'UtMtTIsAnons', RESEED, 0)
DBCC CHECKIDENT ( 'webpages_Roles', RESEED, 0)

