using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Data.SqlClient;
using Application.Interfaces; // For IDbAccess
using Application.Interfaces.Repository;
using Application.Models;


// --- UNIT TEST KLASSE MED MOQ ---
// Vi tester her den refaktorerede version, som afhænger af IDbAccess
[TestClass]
public class RequestRepositoryMoqTest
{
    private Mock<IDbAccess> _mockDbAccess;
    private Data.AdoNet.Refactored.RequestRepository _repository;

    [TestInitialize]
    public void Setup()
    {
        // 1. Opret Mock-instansen af databaseadgangsinterfacet
        _mockDbAccess = new Mock<IDbAccess>();

        // 2. Initialiser Repository med Mock-objektet (Dependency Injection)
        // Vi bruger Data.AdoNet.Refactored.RequestRepository her.
        _repository = new Data.AdoNet.Refactored.RequestRepository(_mockDbAccess.Object);
    }

    // --- Hjælpefunktion til at mocke IDataReader adfærd ---
    private Mock<IDataReader> SetupMockReader(List<object[]> dataRows)
    {
        var mockReader = new Mock<IDataReader>();
        var queue = new Queue<object[]>(dataRows);

        // Simuler Read() kald: Returner true, så længe der er rækker
        mockReader.Setup(r => r.Read())
            .Returns(() => queue.Count > 0)
            .Callback(() =>
            {
                if (queue.Count > 0)
                {
                    var currentRow = queue.Dequeue();
                    // Opsæt værdireturnering baseret på kolonne-indeks
                    mockReader.Setup(r => r.GetInt32(It.Is<int>(i => i == 0))).Returns((int)currentRow[0]);
                    mockReader.Setup(r => r.GetDateTime(It.Is<int>(i => i == 1))).Returns((DateTime)currentRow[1]);
                    mockReader.Setup(r => r.GetString(It.Is<int>(i => i == 2))).Returns((string)currentRow[2]);
                    mockReader.Setup(r => r.GetInt32(It.Is<int>(i => i == 3))).Returns((int)currentRow[3]);
                }
            });

        // Opsæt GetOrdinal til at returnere de korrekte kolonneindeks
        mockReader.Setup(r => r.GetOrdinal("RequestID")).Returns(0);
        mockReader.Setup(r => r.GetOrdinal("RequestDate")).Returns(1);
        mockReader.Setup(r => r.GetOrdinal("Justification")).Returns(2);
        mockReader.Setup(r => r.GetOrdinal("RequestStatus")).Returns(3);

        return mockReader;
    }


    [TestMethod]
    public void GetByID_ShouldReturnRequest_WhenFound()
    {
        // Arrange
        const int expectedId = 101;
        var expectedDate = DateTime.Today;

        var data = new List<object[]>
        {
            // RequestID, RequestDate, Justification, RequestStatus (som int)
            new object[] { expectedId, expectedDate, "Test Justification", (int)RequestStatus.PENDING }
        };

        var mockReader = SetupMockReader(data);

        // Mock: Konfigurer ExecuteReader til at returnere vores mockede læser
        _mockDbAccess
            .Setup(db => db.ExecuteReader(
                "uspGetRequestByID",
                // Verificer at RequestID parameter blev sendt
                It.Is<Dictionary<string, object>>(dict => (int)dict["@RequestID"] == expectedId)
            ))
            .Returns(mockReader.Object);

        // Act
        var result = _repository.GetByID(expectedId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedId, result.RequestID);
    }

    [TestMethod]
    public void GetAll_ShouldReturnListOfRequests_WhenDataExists()
    {
        // Arrange
        var today = DateTime.Today;
        var data = new List<object[]>
        {
            new object[] { 1, today.AddDays(-1), "Request A", (int)RequestStatus.PENDING },
            new object[] { 2, today, "Request B", (int)RequestStatus.CLOSED }
        };

        var mockReader = SetupMockReader(data);

        // Mock: Konfigurer ExecuteReader til at returnere vores mockede læser for GetAll
        _mockDbAccess
            .Setup(db => db.ExecuteReader("uspGetAllRequests", null))
            .Returns(mockReader.Object);

        // Act
        var result = _repository.GetAll().ToList();

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(2, result[1].RequestID);
        Assert.AreEqual(RequestStatus.CLOSED, result[1].Status);
    }

    [TestMethod]
    public void Add_ShouldSetRequestID_WhenSuccessful()
    {
        // Arrange
        const int generatedId = 55;
        var todayDateOnly = DateOnly.FromDateTime(DateTime.Today);
        var newRequest = new Request
        {
            RequestDate = todayDateOnly,
            Justification = "New item",
            Status = RequestStatus.PENDING
        };

        // Mock: Konfigurer ExecuteScalar til at returnere det autogenererede ID (55)
        _mockDbAccess
            .Setup(db => db.ExecuteScalar(
                "uspAddRequest",
                // Verificer at Justification og Status er korrekt i parametrene
                It.Is<Dictionary<string, object>>(dict =>
                    (string)dict["@Justification"] == newRequest.Justification &&
                    (int)dict["@RequestStatus"] == (int)RequestStatus.PENDING
                )
            ))
            .Returns(generatedId);

        // Act
        _repository.Add(newRequest);

        // Assert
        Assert.AreEqual(generatedId, newRequest.RequestID); // Verificer at ID'et blev sat
        _mockDbAccess.Verify(db => db.ExecuteScalar(
            "uspAddRequest",
            It.IsAny<Dictionary<string, object>>()
        ), Times.Once); // Verificer at metoden blev kaldt
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void Delete_ShouldThrowException_WhenNoRowsAffected()
    {
        // Arrange
        const int idToDelete = 999;

        // Mock: Simuler at ExecuteNonQuery returnerer 0 (ingen rækker fundet/slettet)
        _mockDbAccess
            .Setup(db => db.ExecuteNonQuery(
                "uspDeleteRequest",
                It.Is<Dictionary<string, object>>(dict => (int)dict["@RequestID"] == idToDelete)
            ))
            .Returns(0);

        // Act
        _repository.Delete(idToDelete);

        // Assert (Handled by [ExpectedException])
    }

    [TestMethod]
    public void Update_ShouldCallExecuteNonQuery_Once()
    {
        // Arrange
        var requestToUpdate = new Request { RequestID = 1, Justification = "Updated" };

        // Mock: Simuler at ExecuteNonQuery returnerer 1 (én række opdateret)
        _mockDbAccess
            .Setup(db => db.ExecuteNonQuery(
                "uspUpdateRequest",
                // Verificer at RequestID parameteren er korrekt
                It.Is<Dictionary<string, object>>(dict => (int)dict["@RequestID"] == requestToUpdate.RequestID)
            ))
            .Returns(1); // Returnerer 1 for at indikere succes

        // Act
        _repository.Update(requestToUpdate);

        // Assert
        // Verificer at ExecuteNonQuery blev kaldt præcis én gang
        _mockDbAccess.Verify(db => db.ExecuteNonQuery(
            "uspUpdateRequest",
            It.IsAny<Dictionary<string, object>>()
        ), Times.Once);
    }
}