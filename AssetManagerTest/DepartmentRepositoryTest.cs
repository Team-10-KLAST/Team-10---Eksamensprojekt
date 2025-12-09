using System.Data;
using System.Linq;
using System.Reflection;
using Application.Interfaces.Repository;
using Data;
using Data.AdoNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AssetManagerTest;


[TestClass]
public class DepartmentRepositoryTest
{
    private DepartmentRepository _repository;

    [TestInitialize]
    public void Setup()
    {
        var db = DatabaseConnection.GetInstance();
        _repository = new DepartmentRepository(db);
    }

    [TestMethod]
    public void GetAll_ShouldReturnDepartments()
    {
        // Arrange
        var db = DatabaseConnection.GetInstance();
        var repo = new DepartmentRepository(db);

        // Act
        var result = repo.GetAll().ToList();

        // Assert
        Assert.AreEqual(4, result.Count, "Expected exactly 4 departments in the test database.");
        Assert.IsTrue(result.All(department => department.DepartmentID > 0), "All departments should have a valid ID.");
        Assert.IsTrue(result.All(department => !string.IsNullOrWhiteSpace(department.Name)), "All departments should have a name.");
    }
}
