using System.Data;
using System.Linq;
using System.Reflection;
using Application.Interfaces.Repository;
using Application.Models;
using Data;
using Data.AdoNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AssetManagement_Test;

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

