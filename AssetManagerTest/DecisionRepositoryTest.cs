using Application.Models;
using Data.AdoNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using Application.Interfaces;
using Application.Services;
using Data;

namespace AssetManagerTest;

[TestClass]
public class DecisionRepositoryTest
{
    // Repository instance to be tested
    private DecisionRepository _repository = null!;

    // Initialize the repository before each test
    [TestInitialize]
    public void Setup()
    {
        var database = DatabaseConnection.GetInstance();
        _repository = new DecisionRepository(database);
    }

    [TestMethod]
    public void Add_ShouldInsertDecision()
    {
        // Arrange
        var decision = new Decision
        {
            Status = DecisionStatus.APPROVED,
            DecisionDate = DateOnly.FromDateTime(DateTime.Today),
            Comment = "Test add",
            LoanID = 1
        };

        // Act
        _repository.Add(decision);

        // Assert
        var all = _repository.GetAll().ToList();
        Assert.IsTrue(all.Any(d => d.Comment == "Test add"));
    }

    [TestMethod]
    public void GetByID_ShouldReturnCorrectDecision()
    {
        // Arrange
        var decision = new Decision
        {
            Status = DecisionStatus.REJECTED,
            DecisionDate = DateOnly.FromDateTime(DateTime.Today),
            Comment = "Test get by id",
            LoanID = 1
        };
        _repository.Add(decision);
        // Find den rigtige beslutning baseret på unikke felter
        var added = _repository.GetAll().Last(d => d.Comment == "Test get by id" && d.LoanID == 1);

        // Act
        var result = _repository.GetByID(added.DecisionID);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Test get by id", result!.Comment);
    }

    [TestMethod]
    public void Update_ShouldModifyDecision()
    {
        // Arrange
        var decision = new Decision
        {
            Status = DecisionStatus.PENDING,
            DecisionDate = DateOnly.FromDateTime(DateTime.Today),
            Comment = "Test update",
            LoanID = 1
        };
        _repository.Add(decision);
        var added = _repository.GetAll().Last();

        // Act
        added.Comment = "Updated comment";
        _repository.Update(added);

        // Assert
        var updated = _repository.GetByID(added.DecisionID);
        Assert.IsNotNull(updated);
        Assert.AreEqual("Updated comment", updated!.Comment);
    }

    [TestMethod]
    public void Delete_ShouldRemoveDecision()
    {
        // Arrange
        var decision = new Decision
        {
            Status = DecisionStatus.PENDING,
            DecisionDate = DateOnly.FromDateTime(DateTime.Today),
            Comment = "Test delete",
            LoanID = 1
        };
        _repository.Add(decision);
        var added = _repository.GetAll().Last();

        // Act
        _repository.Delete(added.DecisionID);

        // Assert
        var deleted = _repository.GetByID(added.DecisionID);
        Assert.IsNull(deleted);
    }

    [TestMethod]
    public void GetByID_ShouldReturnNullForNonExisting()
    {
        // Act
        var result = _repository.GetByID(-1);

        // Assert
        Assert.IsNull(result);
    }

    // Test
    [TestMethod]
    public void GetAll_ShouldReturnAllDecisions()
    {
        //Arrange (Handled in Setup)

        //Act
        var result = _repository.GetAll().ToList();

        //Assert
        Assert.IsTrue(result.Count > 0, "Should return at least one decision.");
        Assert.IsTrue(result.All(decision => Enum.IsDefined(typeof(DecisionStatus), decision.Status)), "All decisions should have a valid status.");
    }
}

