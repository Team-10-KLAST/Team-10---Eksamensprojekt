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
using Data.Repositories;
using AssetManagerTest;
using Data;

namespace AssetManagerTest;

[TestClass]
public class RequestRepositoryTest
{
    private RequestRepository _requestRepository;

    //setup the repository
    [TestInitialize]
    public void Setup()
    {
        var dbConnection = DatabaseConnection.GetInstance();
        _requestRepository = new RequestRepository(dbConnection);
    }


    // --- Add and GetByID Test (Initial data check) ---

    [TestMethod]
    public void Add_ReturnAValidID()
    {
        //Arrange
        var newRequest = new Request
        {
            RequestDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            NeededByDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            Justification = "Test",
            Status = RequestStatus.PENDING,
        };
        //Act add request to DB and retrieve it back
        _requestRepository.Add(newRequest);
        var retrievedRequest = _requestRepository.GetByID(newRequest.RequestID);

        //Assert
        // Assert 1: A valid ID was generated (> 0). This is the key validity check.
        Assert.IsTrue(newRequest.RequestID > 0, "RequestID should be set by the Add method.");

        // Assert 2: The request was successfully retrieved
        Assert.IsNotNull(retrievedRequest, $"Request with ID {newRequest.RequestID} should be retrieved by ID.");

        // Assert 3: Data integrity check
        Assert.AreEqual(newRequest.RequestID, retrievedRequest.RequestID);
        Assert.AreEqual(newRequest.Justification, retrievedRequest.Justification);
        Assert.AreEqual(newRequest.Status, retrievedRequest.Status);
        Assert.AreEqual(newRequest.RequestDate, retrievedRequest.RequestDate);
    }

    [TestMethod]
    public void Delete_RemovesRequestFromDatabase()
    {
        // Arrange
        var requestToDelete = new Request { RequestDate = DateOnly.FromDateTime(DateTime.Today), NeededByDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)), Justification = "DeleteTest", Status = RequestStatus.PENDING };
        _requestRepository.Add(requestToDelete);
        var idToDelete = requestToDelete.RequestID;

        // Pre-Assert
        Assert.IsNotNull(_requestRepository.GetByID(idToDelete), "Request should exist before deletion.");

        // Act
        _requestRepository.Delete(idToDelete);

        // Assert
        Assert.IsNull(_requestRepository.GetByID(idToDelete), "Request should be null after deletion.");
    }
        

    [TestMethod]
    public void Update_RequestAttributesChange()
    {
        // Arrange
        var newRequest = new Request
        {
            RequestDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            NeededByDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            Justification = "Test",
            Status = RequestStatus.PENDING,
        };
        
        // Act
        _requestRepository.Add(newRequest);
        int ID = newRequest.RequestID;

        newRequest.RequestDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));
        newRequest.NeededByDate = DateOnly.FromDateTime(DateTime.Today.AddDays(17));
        newRequest.Justification = "Updated";
        newRequest.Status = RequestStatus.CLOSED;

        _requestRepository.Update(newRequest);

        var retrievedRequest = _requestRepository.GetByID(ID);

        // Assert
        // 1. Ensure the retrieved object is not null
        Assert.IsNotNull(retrievedRequest, $"Updated Request with ID {ID} should be retrieved.");

        // 2. Assert that all attributes were correctly updated
        Assert.AreEqual(ID, retrievedRequest.RequestID, "RequestID should remain unchanged.");
        Assert.AreEqual(newRequest.RequestDate, retrievedRequest.RequestDate, "RequestDate was not updated correctly.");
        Assert.AreEqual(newRequest.NeededByDate, retrievedRequest.NeededByDate, "NeededByDate was not updated correctly.");
        Assert.AreEqual(newRequest.Justification, retrievedRequest.Justification, "Justification was not updated correctly.");
        Assert.AreEqual(newRequest.Status, retrievedRequest.Status, "Status was not updated correctly.");
    }
}