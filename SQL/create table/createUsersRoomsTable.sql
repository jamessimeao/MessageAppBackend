CREATE TABLE dbo.usersrooms
(
	id INT IDENTITY(1,1) PRIMARY KEY,
	userid INT NOT NULL,
	roomid INT NOT NULL,
	roleinroom NVARCHAR(50),
	CONSTRAINT FK_usersrooms_users FOREIGN KEY (userid)
		REFERENCES users(id)
	ON DELETE CASCADE
	ON UPDATE CASCADE,
	CONSTRAINT FK_usersrooms_rooms FOREIGN KEY (roomid)
		REFERENCES rooms(id)
	ON DELETE CASCADE
	ON UPDATE CASCADE,
	CONSTRAINT AK_userroom UNIQUE(userid, roomid)
);