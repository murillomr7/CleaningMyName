-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CleaningMyNameDb')
BEGIN
    CREATE DATABASE CleaningMyNameDb;
END
GO

USE CleaningMyNameDb;
GO

-- Create Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(320) NOT NULL,
        PasswordHash NVARCHAR(100) NOT NULL,
        LastLoginUtc DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedOnUtc DATETIME2 NOT NULL,
        ModifiedOnUtc DATETIME2 NULL
    );

    CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
END
GO

-- Create Roles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL,
        Description NVARCHAR(200) NULL,
        CreatedOnUtc DATETIME2 NOT NULL,
        ModifiedOnUtc DATETIME2 NULL
    );

    CREATE UNIQUE INDEX IX_Roles_Name ON Roles(Name);
END
GO

-- Create UserRoles Table (Junction table for many-to-many relationship)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
BEGIN
    CREATE TABLE UserRoles (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        RoleId UNIQUEIDENTIFIER NOT NULL,
        CreatedOnUtc DATETIME2 NOT NULL,
        ModifiedOnUtc DATETIME2 NULL,
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX IX_UserRoles_UserId_RoleId ON UserRoles(UserId, RoleId);
END
GO

-- Create RefreshTokens Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        Token NVARCHAR(500) NOT NULL,
        ExpiryDate DATETIME2 NOT NULL,
        IsUsed BIT NOT NULL DEFAULT 0,
        IsRevoked BIT NOT NULL DEFAULT 0,
        CreatedOnUtc DATETIME2 NOT NULL,
        ModifiedOnUtc DATETIME2 NULL,
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
END
GO

-- Insert default roles
IF NOT EXISTS (SELECT * FROM Roles WHERE Name = 'Admin')
BEGIN
    INSERT INTO Roles (Id, Name, Description, CreatedOnUtc)
    VALUES (NEWID(), 'Admin', 'Administrator with full access', GETUTCDATE());
END

IF NOT EXISTS (SELECT * FROM Roles WHERE Name = 'User')
BEGIN
    INSERT INTO Roles (Id, Name, Description, CreatedOnUtc)
    VALUES (NEWID(), 'User', 'Regular user with limited access', GETUTCDATE());
END
GO
