CREATE DATABASE AssetManagement_Test;
GO
USE AssetManagement_Test;
GO

-- TEST DB SETUP AND STORED PROCEDURES --

-- 1) Departments and Roles
CREATE TABLE Department (
    DepartmentID INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) UNIQUE NOT NULL
);
GO

CREATE TABLE Role (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(100) UNIQUE NOT NULL
);
GO

-- 2) Employee
CREATE TABLE Employee (
    EmployeeID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    DepartmentID INT NOT NULL,
    RoleID INT NOT NULL,
    TerminationDate DATE NULL,
    CONSTRAINT FK_Employee_Department FOREIGN KEY (DepartmentID) REFERENCES Department(DepartmentID),
    CONSTRAINT FK_Employee_Role FOREIGN KEY (RoleID) REFERENCES Role(RoleID)
);
GO

-- 3) DeviceDescription
CREATE TABLE DeviceDescription (
    DeviceDescriptionID INT IDENTITY(1,1) PRIMARY KEY,
    DeviceType NVARCHAR(100) NOT NULL,
    OperatingSystem NVARCHAR(100) NOT NULL,
    Location NVARCHAR(100) NOT NULL
);
GO

-- 4) Device
CREATE TABLE Device (
    DeviceID INT IDENTITY(1,1) PRIMARY KEY,
    DeviceDescriptionID INT NOT NULL,
    DeviceStatus INT NOT NULL,
    IsWiped BIT NOT NULL,
    PurchaseDate DATE NULL,
    ExpectedEndDate DATE NULL,
    CONSTRAINT FK_Device_DeviceDescription FOREIGN KEY (DeviceDescriptionID) REFERENCES DeviceDescription(DeviceDescriptionID)
);
GO

-- 5) Request
CREATE TABLE Request (
    RequestID INT IDENTITY(1,1) PRIMARY KEY,
    RequestDate DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    NeededByDate DATE NOT NULL,
    Justification NVARCHAR(2000) NOT NULL,
    RequestStatus INT NOT NULL DEFAULT 0
);
GO

-- 6) Loan
CREATE TABLE Loan (
    LoanID INT IDENTITY(1,1) PRIMARY KEY,
    RequestID INT NULL,
    BorrowerID INT NOT NULL,       
    ApproverID INT NULL,           
    DeviceID INT NOT NULL,
    LoanStatus INT NOT NULL DEFAULT 0,
    StartDate DATE NULL,
    EndDate DATE NULL,
    CONSTRAINT FK_Loan_Request FOREIGN KEY (RequestID) REFERENCES Request(RequestID),
    CONSTRAINT FK_Loan_Borrower FOREIGN KEY (BorrowerID) REFERENCES Employee(EmployeeID),
    CONSTRAINT FK_Loan_Approver FOREIGN KEY (ApproverID) REFERENCES Employee(EmployeeID),
    CONSTRAINT FK_Loan_Device FOREIGN KEY (DeviceID) REFERENCES Device(DeviceID)
);
GO

-- 7) Decision
CREATE TABLE Decision (
    DecisionID INT IDENTITY(1,1) PRIMARY KEY,
    LoanID INT NOT NULL,
    DecisionStatus INT NOT NULL,
    DecisionDate DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    Comment NVARCHAR(2000) NULL,
    CONSTRAINT FK_Decision_Loan FOREIGN KEY (LoanID) REFERENCES Loan(LoanID)
);
GO


-- DUMMY --

-- Departments --
INSERT INTO Department (DepartmentName)
VALUES 
    ('Engineering'),
    ('Sales'),
    ('Accounting'),
    ('Administration');
GO

-- Roles --
INSERT INTO Role (RoleName)
VALUES
    ('Manager'),
    ('Employee');
GO

-- Employees --
INSERT INTO Employee (FirstName, LastName, Email, DepartmentID, RoleID)
VALUES
    ('John', 'Doe', 'john.doe@example.com', 1, 2),   -- Engineering, Employee
    ('Sarah', 'Miller', 'sarah.miller@example.com', 2, 1), -- Sales, Manager
    ('David', 'Kim', 'david.kim@example.com', 3, 2),  -- Accounting, Employee
    ('Emily', 'Clark', 'emily.clark@example.com', 4, 2), -- Administration, Employee
    ('Michael', 'Brown', 'michael.brown@example.com', 1, 1), -- Engineering, Manager
    ('Laura', 'Hansen', 'laura.hansen@example.com', 2, 2),   -- Sales, Employee
    ('Thomas', 'Nielsen', 'thomas.nielsen@example.com', 3, 1), -- Accounting, Manager
    ('Sofie', 'Lund', 'sofie.lund@example.com', 4, 2),       -- Administration, Employee
    ('Anders', 'Jensen', 'anders.jensen@example.com', 1, 2), -- Engineering, Employee
    ('Maria', 'Poulsen', 'maria.poulsen@example.com', 2, 2); -- Sales, Employee
GO

-- DeviceDescriptions --
-- 1. Laptop (Windows, iOS, Android)
INSERT INTO DeviceDescription (DeviceType, OperatingSystem, Location) VALUES
('Laptop', 'Windows', 'Denmark'),
('Laptop', 'Windows', 'Norway'),
('Laptop', 'Windows', 'Sweden'),
('Laptop', 'Windows', 'Germany');

INSERT INTO DeviceDescription (DeviceType, OperatingSystem, Location) VALUES
('Laptop', 'Android', 'Denmark'),
('Laptop', 'Android', 'Norway'),
('Laptop', 'Android', 'Sweden'),
('Laptop', 'Android', 'Germany');

INSERT INTO DeviceDescription (DeviceType, OperatingSystem, Location) VALUES
('Laptop', 'IOS', 'Denmark'),
('Laptop', 'IOS', 'Norway'),
('Laptop', 'IOS', 'Sweden'),
('Laptop', 'IOS', 'Germany');

-- 2. Mobile (iOS, Android)

INSERT INTO DeviceDescription (DeviceType, OperatingSystem, Location) VALUES
('Mobile', 'IOS', 'Denmark'),
('Mobile', 'IOS', 'Norway'),
('Mobile', 'IOS', 'Sweden'),
('Mobile', 'IOS', 'Germany');

INSERT INTO DeviceDescription (DeviceType, OperatingSystem, Location) VALUES
('Mobile', 'Android', 'Denmark'),
('Mobile', 'Android', 'Norway'),
('Mobile', 'Android', 'Sweden'),
('Mobile', 'Android', 'Germany');
GO

-- Devices --
INSERT INTO Device (DeviceDescriptionID, DeviceStatus, IsWiped, PurchaseDate, ExpectedEndDate)
VALUES
-- 3 devices in use (Loan 1,2,3)
(1, 5, 0, '2023-01-10', '2026-01-10'),  -- Device for Loan 1
(2, 5, 0, '2022-05-01', '2025-05-01'),  -- Device for Loan 2
(3, 5, 0, '2021-03-15', '2024-03-15'),  -- Device for Loan 3

-- 3 devices not in use (Loan 4,5,6)
(4, 6, 1, '2024-11-20', '2027-11-20'),  -- Device for Loan 4
(5, 0, 1, '2023-02-01', '2026-02-01'),  -- Device for Loan 5
(9, 0, 1, '2023-06-01', '2026-06-01');  -- Device for Loan 6
GO

-- Requests --
INSERT INTO Request (RequestDate, NeededByDate, Justification, RequestStatus)
VALUES
('2025-01-10', '2025-01-20', 'Need a laptop for new project', 0),
('2025-01-12', '2025-01-25', 'Mobile needed for field work', 0),

('2024-05-01', '2024-05-10', 'Replacement laptop', 1),
('2024-03-15', '2024-03-20', 'New mobile device', 1),
('2023-11-01', '2023-11-15', 'Upgrade request', 1),
('2023-08-10', '2023-08-20', 'Damaged device replacement', 1);
GO

-- Loans --
INSERT INTO Loan (RequestID, BorrowerID, ApproverID, DeviceID, LoanStatus, StartDate, EndDate)
VALUES
-- Active loans
(3, 1, 5, 1, 1, '2024-05-05', NULL),
(4, 2, 5, 2, 1, '2024-03-18', NULL),
(5, 3, 7, 3, 1, '2023-11-05', NULL),

-- Inactive loans
(6, 4, 7, 4, 0, '2024-11-20', '2025-05-20'),
(1, 8, NULL, 5, 0, NULL, NULL),
(2, 9, NULL, 6, 0, NULL, NULL);
GO

-- Decision --
INSERT INTO Decision (LoanID, DecisionStatus, DecisionDate, Comment)
VALUES
(1, 0, '2024-05-06', 'Approved by manager'),
(2, 0, '2024-03-19', 'Approved for field work'),
(3, 0, '2023-11-06', 'Approved replacement'),
(4, 0, '2023-08-16', 'Approved due to damage');
GO




-- STORED PROCEDURES --

-- Employee --

CREATE OR ALTER PROCEDURE uspAddEmployee
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(100),
    @DepartmentID INT,
    @RoleID INT,
    @TerminationDate DATE=NULL
AS
BEGIN
    INSERT INTO Employee (FirstName, LastName, Email, DepartmentID, RoleID, TerminationDate)
    VALUES (@FirstName, @LastName, @Email, @DepartmentID, @RoleID, @TerminationDate);

    SELECT SCOPE_IDENTITY() AS NewEmployeeID;
END
GO

CREATE OR ALTER PROCEDURE uspDeleteEmployee
    @EmployeeID INT
AS
BEGIN 
    DELETE FROM Employee
    WHERE EmployeeID = @EmployeeID;
END
GO

CREATE OR ALTER PROCEDURE uspGetAllEmployees
AS
BEGIN
    SELECT 
        EmployeeID,
        FirstName,
        LastName,
        Email,
        DepartmentID,
        RoleID,
        TerminationDate
    FROM Employee;
END;
GO

CREATE OR ALTER PROCEDURE uspGetEmployeeByID
    @EmployeeID INT
AS
BEGIN
    SELECT 
        EmployeeID,
        FirstName,
        LastName,
        Email,
        DepartmentID,
        RoleID,
        TerminationDate
    FROM Employee
    WHERE EmployeeID = @EmployeeID;
END;
GO

CREATE OR ALTER PROCEDURE uspUpdateEmployee
    @EmployeeID INT,
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(100),
    @DepartmentID INT,
    @RoleID INT,
    @TerminationDate DATE = NULL
AS
BEGIN
    UPDATE Employee
    SET FirstName = @FirstName,
        LastName = @LastName,
        Email = @Email,
        DepartmentID = @DepartmentID,
        RoleID = @RoleID,
        TerminationDate = @TerminationDate
    WHERE EmployeeID = @EmployeeID;
END
GO

-- ROLE + DEPARTMENT --

CREATE OR ALTER PROCEDURE uspGetAllDepartments
AS
BEGIN
    SELECT DepartmentID, DepartmentName
    FROM Department
    ORDER BY DepartmentName;
END;
GO

CREATE OR ALTER PROCEDURE uspGetAllRoles
AS
BEGIN
    SELECT RoleID, RoleName
    FROM Role
    ORDER BY RoleName;
END;
GO

-- DEVICE DESCRIPTION --

CREATE OR ALTER PROCEDURE uspGetAllDeviceDescriptions
AS
BEGIN
    SELECT 
        DeviceDescriptionID,
        DeviceType,
        OperatingSystem,
        Location
    FROM DeviceDescription;
END;
GO

CREATE OR ALTER PROCEDURE uspGetDeviceDescriptionByID
    @DeviceDescriptionID INT
AS
BEGIN
    SELECT 
        DeviceDescriptionID,
        DeviceType,
        OperatingSystem,
        Location
    FROM DeviceDescription
    WHERE DeviceDescriptionID = @DeviceDescriptionID;
END;
GO

-- DEVICE --

-- Device
CREATE OR ALTER PROCEDURE uspAddDevice
    @DeviceDescriptionID INT,
    @DeviceStatus INT,
    @IsWiped BIT,
    @PurchaseDate DATE = NULL,
    @ExpectedEndDate DATE = NULL
AS
BEGIN
    INSERT INTO Device (DeviceDescriptionID, DeviceStatus, IsWiped, PurchaseDate, ExpectedEndDate)
    VALUES (@DeviceDescriptionID, @DeviceStatus, @IsWiped, @PurchaseDate, @ExpectedEndDate);

    SELECT SCOPE_IDENTITY() AS NewDeviceID;
END;
GO

CREATE OR ALTER PROCEDURE uspGetAllDevices
AS
BEGIN
    SELECT 
        DeviceID,
        DeviceDescriptionID,
        DeviceStatus,
        IsWiped,
        PurchaseDate,
        ExpectedEndDate
    FROM Device;
END;
GO

CREATE OR ALTER PROCEDURE uspGetDeviceByID
    @DeviceID INT
AS
BEGIN
    SELECT 
        DeviceID,
        DeviceDescriptionID,
        DeviceStatus,
        IsWiped,
        PurchaseDate,
        ExpectedEndDate
    FROM Device
    WHERE DeviceID = @DeviceID;
END;
GO

CREATE OR ALTER PROCEDURE uspUpdateDevice
    @DeviceID INT,
    @DeviceDescriptionID INT = NULL,
    @DeviceStatus INT = NULL,
    @IsWiped BIT = NULL,
    @PurchaseDate DATE = NULL,
    @ExpectedEndDate DATE = NULL
AS
BEGIN
    UPDATE Device
    SET 
        DeviceDescriptionID = ISNULL(@DeviceDescriptionID, DeviceDescriptionID),
        DeviceStatus        = ISNULL(@DeviceStatus, DeviceStatus),
        IsWiped               = ISNULL(@IsWiped, IsWiped),
        PurchaseDate        = ISNULL(@PurchaseDate, PurchaseDate),
        ExpectedEndDate     = ISNULL(@ExpectedEndDate, ExpectedEndDate)
    WHERE DeviceID = @DeviceID;
END;
GO

CREATE OR ALTER PROCEDURE uspDeleteDevice
    @DeviceID INT
AS
BEGIN
    DELETE FROM Device
    WHERE DeviceID = @DeviceID;
END;
GO

-- DECISION --

CREATE OR ALTER PROCEDURE uspAddDecision
    @LoanID INT,
    @DecisionStatus INT,
    @DecisionDate DATE = NULL,
    @Comment NVARCHAR(2000) = NULL
AS
BEGIN
    IF @DecisionDate IS NULL
    BEGIN
        INSERT INTO Decision (LoanID, DecisionStatus, Comment)
        VALUES (@LoanID, @DecisionStatus, @Comment);
    END
    ELSE
    BEGIN
        INSERT INTO Decision (LoanID, DecisionStatus, DecisionDate, Comment)
        VALUES (@LoanID, @DecisionStatus, @DecisionDate, @Comment);
    END

    SELECT SCOPE_IDENTITY() AS NewDecisionID;
END;
GO

CREATE OR ALTER PROCEDURE uspGetAllDecisions
AS
BEGIN
    SELECT 
        DecisionID, 
        LoanID, 
        DecisionStatus, 
        DecisionDate, 
        Comment
    FROM Decision;
END;
GO

CREATE OR ALTER PROCEDURE uspGetDecisionByID
    @DecisionID INT
AS
BEGIN
    SELECT 
        DecisionID, 
        LoanID, 
        DecisionStatus, 
        DecisionDate, 
        Comment
    FROM Decision
    WHERE DecisionID = @DecisionID;
END;
GO

CREATE OR ALTER PROCEDURE uspUpdateDecision
    @DecisionID INT,
    @LoanID INT,
    @DecisionStatus INT,
    @DecisionDate DATE,
    @Comment NVARCHAR(2000) = NULL
AS
BEGIN
    UPDATE Decision
    SET LoanID = @LoanID,
        DecisionStatus = @DecisionStatus,
        DecisionDate = @DecisionDate,
        Comment = @Comment
    WHERE DecisionID = @DecisionID;
END;
GO


CREATE OR ALTER PROCEDURE uspDeleteDecision
    @DecisionID INT
AS
BEGIN
    DELETE FROM Decision
    WHERE DecisionID = @DecisionID;
END;
GO

-- REQUEST --

CREATE OR ALTER PROCEDURE uspGetAllRequests
AS
BEGIN
	SELECT 
		RequestID,
		RequestDate,
                NeededByDate,
		Justification,
		RequestStatus
	FROM Request;
END;
GO



CREATE OR ALTER PROCEDURE uspGetRequestByID
 @RequestID INT
AS
BEGIN
	SELECT
		RequestID,
		RequestDate,
                NeededByDate,
		Justification,
		RequestStatus
	FROM Request
	WHERE RequestID = @RequestID;
END;
GO


CREATE OR ALTER PROCEDURE uspAddRequest
    @RequestDate DATE,
    @NeededByDate DATE,
    @Justification NVARCHAR(2000),
    @RequestStatus INT
AS
BEGIN
    INSERT INTO Request (RequestDate, NeededByDate, Justification, RequestStatus)
    VALUES (@RequestDate, @NeededByDate, @Justification, @RequestStatus);

    SELECT SCOPE_IDENTITY() AS NewRequestID;
END;
GO


CREATE OR ALTER PROCEDURE uspUpdateRequest
    @RequestID INT,
    @RequestDate DATE,
    @NeededByDate DATE,
    @Justification NVARCHAR(2000),
    @RequestStatus INT
AS
BEGIN
    UPDATE Request
    SET 
        RequestDate = @RequestDate,
        NeededByDate = @NeededByDate,
        Justification = @Justification,
        RequestStatus = @RequestStatus
    WHERE RequestID = @RequestID;
END;
GO


CREATE OR ALTER PROCEDURE uspDeleteRequest
    @RequestID INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Loan WHERE RequestID = @RequestID)
    BEGIN
        DELETE FROM Request
        WHERE RequestID = @RequestID;
    END
END;
GO

-- LOAN -- 

-- Hent alle loans
CREATE OR ALTER PROCEDURE uspGetAllLoans
AS
BEGIN
    SELECT
        LoanID,
        RequestID,
        BorrowerID,
        ApproverID,
        DeviceID,
        LoanStatus,
        StartDate,
        EndDate
    FROM Loan;
END;
GO


-- Hent et loan p� ID
CREATE OR ALTER PROCEDURE uspGetLoanByID
    @LoanID INT
AS
BEGIN
    SELECT
        LoanID,
        RequestID,
        BorrowerID,
        ApproverID,
        DeviceID,
        LoanStatus,
        StartDate,
        EndDate
    FROM Loan
    WHERE LoanID = @LoanID;
END;
GO


-- Opret nyt loan
CREATE OR ALTER PROCEDURE uspAddLoan
    @Status     INT,
    @RequestID   INT = NULL,
    @BorrowerID  INT,
    @ApproverID  INT = NULL,
    @DeviceID    INT,
    @StartDate   DATE,
    @EndDate     DATE = NULL
AS
BEGIN
    INSERT INTO Loan (
        RequestID,
        BorrowerID,
        ApproverID,
        DeviceID,
        LoanStatus,
        StartDate,
        EndDate
    )
    VALUES (
        @RequestID,
        @BorrowerID,
        @ApproverID,
        @DeviceID,
        @Status,
        @StartDate,
        @EndDate
    );

    SELECT SCOPE_IDENTITY() AS NewLoanID;
END;
GO


-- Opdater eksisterende loan
CREATE OR ALTER PROCEDURE uspUpdateLoan
    @LoanID      INT,
    @Status      INT,
    @RequestID   INT = NULL,
    @BorrowerID  INT,
    @ApproverID  INT = NULL,
    @DeviceID    INT,
    @StartDate   DATE,
    @EndDate     DATE = NULL
AS
BEGIN
    UPDATE Loan
    SET
        RequestID  = @RequestID,
        BorrowerID = @BorrowerID,
        ApproverID = @ApproverID,
        DeviceID   = @DeviceID,
        LoanStatus = @Status,
        StartDate  = @StartDate,
        EndDate = @EndDate
    WHERE LoanID = @LoanID;
END;
GO


-- Slet loan
CREATE OR ALTER PROCEDURE uspDeleteLoan
    @LoanID INT
AS
BEGIN
    DELETE FROM Loan
    WHERE LoanID = @LoanID;
END;
GO



