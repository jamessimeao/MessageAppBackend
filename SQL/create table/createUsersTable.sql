CREATE TABLE dbo.users
(
	id INT IDENTITY(1,1) PRIMARY KEY,
	email NVARCHAR(320) UNIQUE NOT NULL, -- 320 is the maximum length an email can have
	passwordhash NVARCHAR(500) NOT NULL,
	username NVARCHAR(100) NOT NULL,
	userrole NVARCHAR(50) NOT NULL,
	refreshtoken NVARCHAR(500) NULL,
	refreshtokenexpirationtime DATETIME NULL
);