using System.Data;
using System.Linq;
using System.Reflection;
using Application.Interfaces.Repository;
using Data;
using Data.AdoNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AssetManagement_Test;


[TestClass]
public class DepartmentRepositoryTest
{
    // Repository instance to be tested
    private DepartmentRepository _repository = null!;

    // Arrange: Initialize the repository before each test
    [TestInitialize]
    public void Setup()
    {
        var database = DatabaseConnection.GetInstance();
        _repository = new DepartmentRepository(database);
    }

    // Test to verify that GetAll method returns the expected departments
    [TestMethod]
    public void GetAll_ShouldReturnDepartments()
    {
        // Arrange (Handled in Setup)

        // Act
        var result = _repository.GetAll().ToList();

        // Assert
        Assert.AreEqual(4, result.Count, "Expected exactly 4 departments in the test database.");
        Assert.IsTrue(result.All(department => department.DepartmentID > 0), "All departments should have a valid ID.");
        Assert.IsTrue(result.All(department => !string.IsNullOrWhiteSpace(department.Name)), "All departments should have a name.");
    }
}
