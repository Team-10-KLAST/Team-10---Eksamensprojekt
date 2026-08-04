USE AssetManagement;
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



