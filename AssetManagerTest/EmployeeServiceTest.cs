using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Application.Models;
using Application.Interfaces;
using Application.Services;
using Application.Interfaces.Repository;

namespace AssetManagerTest;

//Arrange

public class DummyEmployeeRepository : IRepository<Employee>
{
    private readonly List<Employee> _employees= new List<Employee>();

    public DummyEmployeeRepository() { }

    public void Add(Employee entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(int id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Employee> GetAll()
    {
        throw new NotImplementedException();
    }

    public Employee? GetById(int id)
    {
        throw new NotImplementedException();
    }

    public void Update(Employee entity)
    {
        throw new NotImplementedException();
    }
}

[TestClass]
public class EmployeeServiceTest
{

    [TestMethod]
    public void TestMethod1()
    {
    }
}
