using Data;
using Data.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AssetManagerTest;

[TestClass]
public class DeviceDescriptionRepositoryTest
{
    // Repository instance to be tested
    private DeviceDescriptionRepository _repository = null!;

    // Initialize the repository before each test
    [TestInitialize]
    public void Setup()
    {
        var database = DatabaseConnection.GetInstance();
        _repository = new DeviceDescriptionRepository(database);
    }

    // Test to verify that GetAll method returns the expected device descriptions
    [TestMethod]
    public void GetAll_ShouldReturnAllDeviceDescriptions()
    {
        // Arrange (Handled in Setup)

        // Act
        var result = _repository.GetAll().ToList();

        // Assert
        Assert.AreEqual(16, result.Count, "Expected exactly 16 device descriptions in the test database.");
        Assert.IsTrue(result.All(deviceDescriptions => deviceDescriptions.DeviceDescriptionID > 0));
        Assert.IsTrue(result.All(deviceDescriptions => !string.IsNullOrWhiteSpace(deviceDescriptions.DeviceType)));
        Assert.IsTrue(result.All(deviceDescriptions => !string.IsNullOrWhiteSpace(deviceDescriptions.OperatingSystem)));
        Assert.IsTrue(result.All(deviceDescriptions => !string.IsNullOrWhiteSpace(deviceDescriptions.Location)));
    }

    // Test to verify that GetByID method returns the correct device description for an existing ID
    [TestMethod]
    public void GetByID_ShouldReturnDeviceDescription_WhenIDExists()
    {
        // Arrange (some handled in Setup)
        var first = _repository.GetAll().First();
        int existingId = first.DeviceDescriptionID;

        // Act
        var result = _repository.GetByID(existingId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(existingId, result.DeviceDescriptionID);
    }

    // Test to verify that GetByID method throws ArgumentException for invalid IDs
    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetByID_ShouldThrowArgumentException_WhenIDIsInvalid()
    {
        // Arrange (handled in Setup)

        // Act
        _repository.GetByID(0);

        // Assert is handled by ExpectedException
    }

    // Test to verify that GetByID method returns null for a non-existing ID
    [TestMethod]
    public void GetByID_ShouldReturnNull_WhenIDDoesNotExist()
    {
        // Arrange (handled in Setup)

        // Act
        var result = _repository.GetByID(99999);

        // Assert
        Assert.IsNull(result);
    }
}
