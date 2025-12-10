using System.Data;
using System.Linq;
using System.Reflection;
using Application.Interfaces.Repository;
using Data;
using Data.AdoNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AssetManagement_Test;

[TestClass]
public class RoleRepositoryTest
{
    // Repository instance to be tested
    private RoleRepository _repository = null!;

    // Initialize the repository before each test
    [TestInitialize]
    public void Setup()
    {
        var database = DatabaseConnection.GetInstance();
        _repository = new RoleRepository(database);
    }

    // Test to verify that GetAll method returns the expected roles
    [TestMethod]
    public void GetAll_ShouldReturnRoles()
    {
        //Act
        var result = _repository.GetAll().ToList();

        //Assert
        Assert.AreEqual(2, result.Count, "Expected exactly 2 roles in the test database.");
        Assert.IsTrue(result.All(role => role.RoleID > 0), "All roles should have a name");
        Assert.IsTrue(result.All(role => !string.IsNullOrWhiteSpace(role.Name)), "All roles must have a valid ID.");
    }
}

