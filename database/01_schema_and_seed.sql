CREATE DATABASE AssetManagement;
GO
USE AssetManagement;
GO

-- DB SETUP --

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
