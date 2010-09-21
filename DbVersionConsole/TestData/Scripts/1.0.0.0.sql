Create Table Users (
	User_ID int,
	User_Name varchar(100),
	User_DateCreated datetime
);

Insert Into Users (User_ID, User_Name, User_DateCreated) Values (1, 'bmontgomery', getdate());
Insert Into Users (User_ID, User_Name, User_DateCreated) Values (2, 'atorres', getdate());
Insert Into Users (User_ID, User_Name, User_DateCreated) Values (3, 'astock', getdate());
Insert Into Users (User_ID, User_Name, User_DateCreated) Values (4, 'meland', getdate());
Insert Into Users (User_ID, User_Name, User_DateCreated) Values (5, 'msayers', getdate());