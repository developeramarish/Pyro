﻿using Hl7.Fhir.Model;
using Pyro.Common.CompositionRoot;
using Pyro.Common.Interfaces.Repositories;
using Pyro.DataLayer.DbModel.DatabaseContext;
using Pyro.Common.Exceptions;
using System;
using System.Data.Entity;

namespace Pyro.DataLayer.DbModel.UnitOfWork
{
  public partial class UnitOfWork : IUnitOfWork, IDisposable
  {
    private IPyroDbContext _context = null;
    private IResourceRepositoryFactory IResourceRepositoryFactory;

    public UnitOfWork(IPyroDbContext PyroDbContext, IResourceRepositoryFactory IResourceRepositoryFactory)
    {
      _context = PyroDbContext;
      this.IResourceRepositoryFactory = IResourceRepositoryFactory;
    }

    public DbContextTransaction BeginTransaction()
    {
      try
      {
        //return _context.Database.CurrentTransaction ?? _context.Database.BeginTransaction();
        return _context.Database.BeginTransaction();
      }
      catch (Exception Exec)
      {
        string Message = Exec.Message;
        throw new PyroException(System.Net.HttpStatusCode.InternalServerError,
          Pyro.Common.Tools.FhirOperationOutcomeSupport.Create(OperationOutcome.IssueSeverity.Error, OperationOutcome.IssueType.Exception, Message), Message);
      }
    }

    //public IDtoCommonRepository CommonRepository
    //{
    //  get
    //  {
    //    return IResourceRepositoryFactory.CreateDtoCommonRepository();
    //  }
    //}

    #region Implementing IDiosposable...

    #region private dispose variable declaration...
    private bool disposed = false;
    #endregion

    /// <summary>
    /// Protected Virtual Dispose method
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposed)
      {
        if (disposing)
        {
          _context.Dispose();
        }
      }
      this.disposed = true;
    }

    /// <summary>
    /// Dispose method
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~UnitOfWork() // the finalizer
    {
      Dispose(false);
    }
    #endregion

  }
}

