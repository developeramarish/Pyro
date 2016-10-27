﻿using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Blaze.Common.Interfaces.Services;
using Blaze.Common.Interfaces;
using System.Net;
using Blaze.Common.BusinessEntities.Dto;
using Blaze.Common.Enum;
using Blaze.Common.Tools;

namespace Blaze.Common.BusinessEntities.Service
{
  public class ServiceOperationOutcome : IServiceOperationOutcome
  {
    #region Public Properties

    public string FhirResourceId { get; set; }
    public Uri RequestUri { get; set; }
    public string ResourceVersionNumber { get; set; }
    public RestEnum.CrudOperationType OperationType { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public ISearchParametersValidationOperationOutcome SearchValidationOperationOutcome { get; set; }
    public IResourceValidationOperationOutcome ResourceValidationOperationOutcome { get; set; }
    public ISearchPlanValidationOperationOutcome SearchPlanValidationOperationOutcome { get; set; }
    public IDatabaseOperationOutcome DatabaseOperationOutcome { get; set; }
    public HttpStatusCode HttpStatusCodeToReturn
    {
      get
      {
        return ResolveHttpStatusCodeToReturn();
      }
    }
    public Resource ResourceToReturn()
    {
      return ResolveResourceToReturn();
    }
    #endregion

    #region Constructor
    internal ServiceOperationOutcome()
    {
      this.OperationType = RestEnum.CrudOperationType.None;
    }
    #endregion

    #region Private Methods
    private HttpStatusCode ResolveHttpStatusCodeToReturn()
    {
      if (this.SearchValidationOperationOutcome != null)
      {
        return this.SearchValidationOperationOutcome.HttpStatusCode;
      }
      if (this.SearchPlanValidationOperationOutcome != null)
      {
        return this.SearchPlanValidationOperationOutcome.HttpStatusCode;
      }
      else if (this.ResourceValidationOperationOutcome != null)
      {
        return this.ResourceValidationOperationOutcome.HttpStatusCode;
      }
      else if (this.OperationType == RestEnum.CrudOperationType.Create)
      {
        return HttpStatusCode.Created;
      }
      if (this.OperationType == RestEnum.CrudOperationType.Read)
      {
        if (this.DatabaseOperationOutcome.SingleResourceRead)
        {
          if (this.DatabaseOperationOutcome.ReturnedResource != null)
          {
            if (this.DatabaseOperationOutcome.ReturnedResource.IsDeleted)
            {
              return HttpStatusCode.Gone;
            }
            else
            {
              return HttpStatusCode.OK;
            }
          }
          else
          {
            return HttpStatusCode.NotFound;
          }
        }
        else
        {
          return HttpStatusCode.OK;
        }
      }
      else if (this.OperationType == RestEnum.CrudOperationType.Update)
      {
        return HttpStatusCode.OK;
      }
      else if (this.OperationType == RestEnum.CrudOperationType.Delete)
      {
        return HttpStatusCode.NoContent;
      }
      else
      {
        var OpOutComeIssueComp = new OperationOutcome.IssueComponent();
        OpOutComeIssueComp.Severity = OperationOutcome.IssueSeverity.Fatal;
        OpOutComeIssueComp.Code = OperationOutcome.IssueType.Exception;
        OpOutComeIssueComp.Diagnostics = "Internal Server Error: BlazeServiceOperationoutcome was unable to resolve a Http Status Code for the operation performed.";
        var OpOutCome = new OperationOutcome();
        OpOutCome.Issue = new List<OperationOutcome.IssueComponent>() { OpOutComeIssueComp };
        throw new DtoBlazeException(HttpStatusCode.InternalServerError, OpOutCome, OpOutComeIssueComp.Diagnostics);
      }
    }
    private Resource ResolveResourceToReturn()
    {
      if (this.OperationType == RestEnum.CrudOperationType.Delete)
      {
        return null;
      }
      else if (this.SearchValidationOperationOutcome != null)
      {
        return this.SearchValidationOperationOutcome.FhirOperationOutcome;
      }
      else if (this.SearchPlanValidationOperationOutcome != null)
      {
        return this.SearchPlanValidationOperationOutcome.FhirOperationOutcome;
      }
      else if (this.ResourceValidationOperationOutcome != null)
      {
        return this.ResourceValidationOperationOutcome.FhirOperationOutcome;
      }
      else if (this.DatabaseOperationOutcome != null)
      {
        if (this.DatabaseOperationOutcome.SingleResourceRead)
        {
          if (this.DatabaseOperationOutcome.ReturnedResource != null)
          {
            if (this.DatabaseOperationOutcome.ReturnedResource.IsDeleted)
            {
              return null;
            }
            else
            {
              return SerializeToResource();
            }
          }
          else
          {
            //There is no error, we just did not find a resource for the FHIRResourceId they provided. 
            return null;
          }
        }
        else if (this.DatabaseOperationOutcome.ReturnedResourceList != null)
        {
          return SerializeToBundle();
        }
        else
        {
          var OpOutComeIssueComp = new OperationOutcome.IssueComponent();
          OpOutComeIssueComp.Severity = OperationOutcome.IssueSeverity.Fatal;
          OpOutComeIssueComp.Code = OperationOutcome.IssueType.Exception;
          OpOutComeIssueComp.Diagnostics = "Internal Server Error: BlazeServiceOperationoutcome was unable to resolve any Resource to return for the operation performed even though no error was detected, this s a bug in the server!.";
          var OpOutCome = new OperationOutcome();
          OpOutCome.Issue = new List<OperationOutcome.IssueComponent>() { OpOutComeIssueComp };
          throw new DtoBlazeException(HttpStatusCode.InternalServerError, OpOutCome, OpOutComeIssueComp.Diagnostics);
        }
      }
      else if (this.OperationType == RestEnum.CrudOperationType.Create)
      {
        return null;
      }
      else if (this.OperationType == RestEnum.CrudOperationType.Update)
      {
        return null;
      }
      else
      {
        var OpOutComeIssueComp = new OperationOutcome.IssueComponent();
        OpOutComeIssueComp.Severity = OperationOutcome.IssueSeverity.Fatal;
        OpOutComeIssueComp.Code = OperationOutcome.IssueType.Exception;
        OpOutComeIssueComp.Diagnostics = "Internal Server Error: BlazeServiceOperationoutcome was unable to resolve a Http Status Code for the operation performed.";
        var OpOutCome = new OperationOutcome();
        OpOutCome.Issue = new List<OperationOutcome.IssueComponent>() { OpOutComeIssueComp };
        throw new DtoBlazeException(HttpStatusCode.InternalServerError, OpOutCome, OpOutComeIssueComp.Diagnostics);
      }
    }
    private Resource SerializeToResource()
    {
      try
      {
        //Resource oResource = Hl7.Fhir.Serialization.FhirParser.ParseResourceFromXml(this.DatabaseOperationOutcome.ResourceMatchingSearch.Xml);
        Hl7.Fhir.Serialization.FhirXmlParser FhirXmlParser = new Hl7.Fhir.Serialization.FhirXmlParser();
        Resource oResource = FhirXmlParser.Parse<Resource>(this.DatabaseOperationOutcome.ReturnedResource.Xml);        
        return oResource;
      }
      catch (Exception oExec)
      {
        var OpOutComeIssueComp = new OperationOutcome.IssueComponent();
        OpOutComeIssueComp.Severity = OperationOutcome.IssueSeverity.Fatal;
        OpOutComeIssueComp.Code = OperationOutcome.IssueType.Exception;
        OpOutComeIssueComp.Diagnostics = String.Format("Internal Server Error: Serialization of a Resource retrieved from the servers database failed. The record details were: FhirId: {0}, ResourceVersion: {1}, Received: {2}. The parser exception error was '{3}", this.DatabaseOperationOutcome.ReturnedResource.FhirId, this.DatabaseOperationOutcome.ReturnedResource.Version, this.DatabaseOperationOutcome.ReturnedResource.Received.ToString(), oExec.Message);
        var OpOutcome = new OperationOutcome();
        OpOutcome.Issue = new List<Hl7.Fhir.Model.OperationOutcome.IssueComponent>() { OpOutComeIssueComp };
        throw new DtoBlazeException(System.Net.HttpStatusCode.InternalServerError, OpOutcome, OpOutComeIssueComp.Diagnostics);
      }
    }
    private Bundle SerializeToBundle()
    {
      var FhirBundle = new Bundle() { Type = Bundle.BundleType.Searchset };

      FhirBundle.Total = this.DatabaseOperationOutcome.ReturnedResourceCount;

      //Paging           
      int LastPageNumber = PagingSupport.GetLastPageNumber(this.DatabaseOperationOutcome.PagesTotal);
      FhirBundle.FirstLink = PagingSupport.GetPageNavigationUri(this.RequestUri, 1);
      FhirBundle.LastLink = PagingSupport.GetPageNavigationUri(this.RequestUri, LastPageNumber);
      FhirBundle.NextLink = PagingSupport.GetPageNavigationUri(this.RequestUri, PagingSupport.GetNextPageNumber(this.DatabaseOperationOutcome.PageRequested, this.DatabaseOperationOutcome.PagesTotal));
      FhirBundle.PreviousLink = PagingSupport.GetPageNavigationUri(this.RequestUri, PagingSupport.GetPreviousPageNumber(this.DatabaseOperationOutcome.PageRequested));

      foreach (DtoResource DtoResource in this.DatabaseOperationOutcome.ReturnedResourceList)
      {
        Bundle.EntryComponent oResEntry = new Bundle.EntryComponent();
        try
        {
          //var Resource = Hl7.Fhir.Serialization.FhirParser.ParseResourceFromXml(DtoResource.Xml) as Resource;
          Hl7.Fhir.Serialization.FhirXmlParser FhirXmlParser = new Hl7.Fhir.Serialization.FhirXmlParser();
          Resource oResource = FhirXmlParser.Parse<Resource>(DtoResource.Xml);          
          oResEntry.Resource = oResource;
        }
        catch (Exception oExec)
        {
          var OpOutComeIssueComp = new OperationOutcome.IssueComponent();
          OpOutComeIssueComp.Severity = OperationOutcome.IssueSeverity.Fatal;
          OpOutComeIssueComp.Code = OperationOutcome.IssueType.Exception;
          OpOutComeIssueComp.Diagnostics = String.Format("Internal Server Error: Serialization of a Resource retrieved from the servers database failed. The record details were: Key: {0}, ResourceVersion: {1}, Received: {2}. The parser exception error was '{3}", DtoResource.FhirId, DtoResource.Version, DtoResource.Received.ToString(), oExec.Message);
          var OpOutcome = new OperationOutcome();
          OpOutcome.Issue = new List<Hl7.Fhir.Model.OperationOutcome.IssueComponent>() { OpOutComeIssueComp };
          throw new DtoBlazeException(System.Net.HttpStatusCode.InternalServerError, OpOutcome, OpOutComeIssueComp.Diagnostics);
        }
        oResEntry.Search = new Bundle.SearchComponent();
        oResEntry.Search.Mode = Bundle.SearchEntryMode.Match;
        oResEntry.Link = new List<Bundle.LinkComponent>();

        FhirBundle.Entry.Add(oResEntry);
      }
      return FhirBundle;
    }
    #endregion

  }
}
