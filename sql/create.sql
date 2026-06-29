CREATE DATABASE TaskManager;

USE TaskManager;

CREATE LOGIN appuser WITH PASSWORD = 'CoolPassword!';
CREATE USER appuser FOR LOGIN appuser;
/*
 * Only select and insert are granted by default to all tables.
 * Those requiring update and/or delete are granted as-needed.
 */
GRANT SELECT, INSERT ON SCHEMA::dbo TO appuser;

CREATE TABLE OpenTask (
	Id UNIQUEIDENTIFIER PRIMARY KEY,
	Created DATETIME NOT NULL,
	[Type] TINYINT NOT NULL,
	Due DATETIME NOT NULL
);


CREATE TABLE WorkflowStatus (
	Id TINYINT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
	[Name] VARCHAR(25) NOT NULL
);
INSERT INTO WorkflowStatus ([Name]) VALUES('New'), ('In Progress'), ('Complete')

CREATE TABLE [User] (
	Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
	Email VARCHAR(255) NOT NULL
)

CREATE TABLE OpenTaskWorkflow (
	Id INT IDENTITY(1,1) PRIMARY KEY,
    OpenTaskId UNIQUEIDENTIFIER NOT NULL REFERENCES OpenTask(Id),
	WorkflowStatusId TINYINT NOT NULL DEFAULT 0 REFERENCES WorkflowStatus(Id),
	UserId INT REFERENCES [User](Id)
)
GRANT UPDATE ON OpenTaskWorkflow TO appuser;


CREATE TABLE Todo (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OpenTaskId UNIQUEIDENTIFIER NOT NULL REFERENCES OpenTask(Id),
    IsOrderedByPriority BIT NOT NULL
)


CREATE TABLE TodoItem (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TodoId INT NOT NULL REFERENCES Todo(Id),
    Item NVARCHAR(500) NOT NULL,
)


CREATE TABLE HoneyDo (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OpenTaskId UNIQUEIDENTIFIER NOT NULL REFERENCES OpenTask(Id),
    IsUrgent BIT NOT NULL,
    Description NVARCHAR(500) NOT NULL
)


CREATE TABLE MountainDew (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OpenTaskId UNIQUEIDENTIFIER NOT NULL REFERENCES OpenTask(Id),
    ExtremeCaffeine BIT NOT NULL,
    Flavor NVARCHAR(100) NOT NULL,
    Comment NVARCHAR(500) NOT NULL
)
