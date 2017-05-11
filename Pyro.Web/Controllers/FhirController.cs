﻿using System.Net.Http;
using System.Web.Http;
using FhirModel = Hl7.Fhir.Model;
using Pyro.Web.Response;
using Pyro.Common.Interfaces.Service;
using Pyro.Common.Interfaces.UriSupport;
using Pyro.Common.Interfaces.Dto;
using Pyro.Common.Interfaces.Dto.Headers;
using Pyro.Common.Extentions;
using Pyro.Common.Enum;
using Pyro.Web.Extensions;
using Pyro.Web.Attributes;
using System.Net.Http.Formatting;

namespace Pyro.Web.Controllers
{
  [RoutePrefix(Pyro.Common.Web.StaticWebInfo.ServiceRoute)]
  public class FhirController : ApiController
  {
    private readonly IServiceNegotiator _FhirServiceNegotiator;
    //Constructor for dependence injection 
    public FhirController(IServiceNegotiator FhirServiceNegotiator)
    {
      _FhirServiceNegotiator = FhirServiceNegotiator;
      //Configuration.DependencyResolver.GetService
    }

    //Service Root Base
    //// POST: URL//FhirApi
    /// <summary>
    /// The service root can accept a Batch Transaction bundle.
    /// The batch and transaction interactions submit a set of actions to perform on a server in a single 
    /// HTTP request/response. The actions may be performed independently as a "batch", or as a single 
    /// atomic "transaction" where the entire set of changes succeed or fail as a single entity. 
    /// Multiple actions on multiple resources of the same or different types may be submitted, and they 
    /// may be a mix of other interactions defined on this page (e.g. read, search, create, update, delete, etc.).
    /// </summary>
    /// <param name="resource">Batch Transaction bundle</param>
    /// <returns>Status Code 200 (OK) and a Batch Transaction response bundle or an OperationOutcome resource if an error has been encountered. The Transaction response bundle will inform how each resource was actioned within the entry.response elements.</returns>
    [HttpPost, Route("")]
    [ActionLog]
    public HttpResponseMessage Base([FromBody] FhirModel.Resource resource)
    {
      ICommonServices oService = _FhirServiceNegotiator.GetCommonService();
      string BaseRequestUri = this.CalculateBaseURI("metadata");
      IDtoRootUrlStore DtoRootUrlStore = Services.PrimaryServiceRootFactory.Create(oService, BaseRequestUri);
      IFhirRequestUri FhirRequestUri = Common.CommonFactory.GetFhirRequestUri(DtoRootUrlStore.Url, Request.RequestUri.OriginalString);
      IDtoRequestUri DtoRequestUri = Common.CommonFactory.GetRequestUri(DtoRootUrlStore, FhirRequestUri);      
      //IDtoRootUrlStore PrimaryRootUrlStore = Pyro.Common.Cache.StaticCacheCommon.GetPrimaryRootUrlStore(oService);
      IDtoRequestHeaders RequestHeaders = Common.CommonFactory.GetDtoRequestHeaders(Request.Headers);
      IDtoSearchParameterGeneric SearchParameterGeneric = Common.CommonFactory.GetDtoSearchParameterGeneric(Request.GetSearchParams());
      //IDtoRequestUri DtoFhirRequestUri = Common.CommonFactory.GetRequestUri(DtoRootUrlStore);
      IResourceServiceRequestTransactionBundle ResourceServiceRequestTransactionBundle = Common.CommonFactory.GetResourceServiceRequestTransactionBundle(resource, DtoRequestUri, SearchParameterGeneric, RequestHeaders, _FhirServiceNegotiator);
      IBundleTransactionService BundleTransactionService = Common.CommonFactory.GetBundleTransactionService(ResourceServiceRequestTransactionBundle);
      IResourceServiceOutcome ResourceServiceOutcome = BundleTransactionService.Transact();
      return FhirRestResponse.GetHttpResponseMessage(ResourceServiceOutcome, Request, SearchParameterGeneric.SummaryType);
    }

    //Metadata 
    // GET: URL//FhirApi/metadata
    /// <summary>
    /// Get the FHIR CapabilityStatement Resource for this Server.
    /// </summary>
    /// <returns>CapabilityStatement Resource</returns>
    [HttpGet, Route("metadata")]
    [ActionLog]
    public HttpResponseMessage Metadata()
    {
      string BaseRequestUri = this.CalculateBaseURI("metadata");
      ICommonServices oService = _FhirServiceNegotiator.GetCommonService();
      IDtoRootUrlStore DtoRootUrlStore = Services.PrimaryServiceRootFactory.Create(oService as ICommonServices, BaseRequestUri);
      //IFhirRequestUri FhirRequestUri = Common.CommonFactory.GetFhirRequestUri(DtoRootUrlStore.Url, Request.RequestUri.OriginalString);
      //IDtoRequestUri DtoRequestUri = Common.CommonFactory.GetRequestUri(DtoRootUrlStore, FhirRequestUri);
      //IDtoRootUrlStore RootUrl = oService.GetPrimaryServiceRootUrl();
      var MetadataService = new Pyro.Engine.Services.MetadataService();
      string ApplicationVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(Pyro.Web.WebApiApplication).Assembly.Location).ProductVersion;
      IDtoSearchParameterGeneric SearchParameterGeneric = Common.CommonFactory.GetDtoSearchParameterGeneric(Request.GetSearchParams());
      IResourceServiceRequestMetadata ResourceServiceRequestMetadata = Common.CommonFactory.GetResourceServiceRequestMetadata(ApplicationVersion, DtoRootUrlStore, SearchParameterGeneric, oService);
      IResourceServiceOutcome oPyroServiceOperationOutcome = MetadataService.GetServersConformanceResource(ResourceServiceRequestMetadata);
      return FhirRestResponse.GetHttpResponseMessage(oPyroServiceOperationOutcome, Request, SearchParameterGeneric.SummaryType);
    }

    // Get By id
    // GET URL/FhirApi/Patient/5
    /// <summary>
    /// Get a Resource by the Resource name and the id of the required resource instance.
    /// For example:
    /// http://SomeServer.net/fhirapi/Patient/123456
    /// </summary>
    /// <param name="ResourceName">The name of a FHIR Resource, for example 'Patient'</param>
    /// <param name="id">The FHIR Resource's id</param>
    /// <returns>Returns the single FHIR Resource identified by the id given or status code 400 (Not found) </returns>
    [HttpGet, Route("{ResourceName}/{id}")]
    [ActionLog]
    public HttpResponseMessage Get(string ResourceName, string id)
    {      
      IResourceServices oService = _FhirServiceNegotiator.GetResourceService(ResourceName);

      string BaseRequestUri = this.CalculateBaseURI("{ResourceName}");
      IDtoRootUrlStore DtoRootUrlStore = Services.PrimaryServiceRootFactory.Create(oService as ICommonServices, BaseRequestUri);
      IFhirRequestUri FhirRequestUri = Common.CommonFactory.GetFhirRequestUri(DtoRootUrlStore.Url, Request.RequestUri.OriginalString);      
      IDtoRequestUri DtoRequestUri = Common.CommonFactory.GetRequestUri(DtoRootUrlStore, FhirRequestUri);      
      IDtoSearchParameterGeneric SearchParameterGeneric = Common.CommonFactory.GetDtoSearchParameterGeneric(Request.GetSearchParams());
      IDtoRequestHeaders RequestHeaders = Common.CommonFactory.GetDtoRequestHeaders(Request.Headers);
      IResourceServiceRequestGetRead PyroServiceRequestGetRead = Common.CommonFactory.GetResourceServiceRequestGetRead(id, DtoRequestUri, SearchParameterGeneric, RequestHeaders);      
      IResourceServiceOutcome oPyroServiceOperationOutcome = oService.GetRead(PyroServiceRequestGetRead);
      return FhirRestResponse.GetHttpResponseMessage(oPyroServiceOperationOutcome, Request, SearchParameterGeneric.SummaryType);
    }

    //Search
    // GET: URL//FhirApi/Patient?family=Smith&given=John
    /// <summary>
    /// Search for a FHIR resource using the Resources defined search parameters.
    /// For example: 
    /// http://SomeServer.net/fhirapi/Patient?family=Smith&amp;given=John
    /// </summary>
    /// <param name="ResourceName">The name of a FHIR Resource, for example 'Patient'</param>
    /// <returns>Returns a bundle containing all resource that match the search criteria.</returns>
    [HttpGet, Route("{ResourceName}")]
    [ActionLog]
    public HttpResponseMessage Search(string ResourceName)
    {
      IResourceServices oService = _FhirServiceNegotiator.GetResourceService(ResourceName);
      string BaseRequestUri = this.CalculateBaseURI("{ResourceName}");
      IDtoRootUrlStore DtoRootUrlStore = Services.PrimaryServiceRootFactory.Create(oService as ICommonServices, BaseRequestUri);
      IFhirRequestUri FhirRequestUri = Common.CommonFactory.GetFhirRequestUri(DtoRootUrlStore.Url, Request.RequestUri.OriginalString);
      IDtoRequestUri DtoRequestUri = Common.CommonFactory.GetRequestUri(DtoRootUrlStore, FhirRequestUri);        
      IDtoSearchParameterGeneric SearchParameterGeneric = Common.CommonFactory.GetDtoSearchParameterGeneric(Request.GetSearchParams());
      IResourceServiceRequestGetSearch ResourceServiceRequestGetSearch = Common.CommonFactory.GetResourceServiceRequestGetSearch(DtoRequestUri, SearchParameterGeneric);
      IResourceServiceOutcome oPyroServiceOperationOutcome = oService.GetSearch(ResourceServiceRequestGetSearch);
      return FhirRestResponse.GetHttpResponseMessage(oPyroServiceOperationOutcome, Request, SearchParameterGeneric.SummaryType);
    }

    // Get By id and _history vid
    // GET URL/FhirApi/Patient/5/_history/2
    // GET URL/FhirApi/Patient/5/_history
    /// <summary>
    /// Get a Resource's entire history or an given history instance for the Resource by its version id (vid)
    /// For example: 
    /// http://SomeServer.net/fhirapi/Patient/123456/_history/5
    /// </summary>
    /// <param name="ResourceName">The name of a FHIR Resource, for example 'Patient'</param>
    /// <param name="id">The FHIR Resource's id</param>
    /// <param name="vid">The version id if requesting a single version instance, leave empty if you require the entire history for the resource instance.</param>
    /// <returns>Returns the single FHIR Resource identified by the id and the vid (Version Number) or returns the entire history for the resource instance as a history bundle if vid is empty or status code 400 (Not found) </returns>
    [HttpGet, Route("{ResourceName}/{id}/_history/{vid?}")]
    [ActionLog]
    public HttpResponseMessage Get(string ResourceName, string id, string vid = "")
    {
      IResourceServices oService = _FhirServiceNegotiator.GetResourceService(ResourceName);
      string BaseRequestUri = this.CalculateBaseURI("{ResourceName}");
      IDtoRootUrlStore DtoRootUrlStore = Services.PrimaryServiceRootFactory.Create(oService as ICommonServices, BaseRequestUri);
      IFhirRequestUri FhirRequestUri = Common.CommonFactory.GetFhirRequestUri(DtoRootUrlStore.Url, Request.RequestUri.OriginalString);
      IDtoRequestUri DtoRequestUri = Common.CommonFactory.GetRequestUri(DtoRootUrlStore, FhirRequestUri);      
      IDtoSearchParameterGeneric SearchParameterGeneric = Common.CommonFactory.GetDtoSearchParameterGeneric(Request.GetSearchParams());
      IResourceServiceRequestGetHistory ResourceServiceRequestGetHistory = Common.CommonFactory.GetResourceServiceRequestGetHistory(id, vid, DtoRequestUri, SearchParameterGeneric);
      IResourceServiceOutcome ResourceServiceOutcome = oService.GetHistory(ResourceServiceRequestGetHistory);
      return FhirRestResponse.GetHttpResponseMessage(ResourceServiceOutcome, Request, SearchParameterGeneric.SummaryType);
    }

    // Create
    // POST: URL/FhirApi/Patient
    /// <summary>
    /// Creates a new Resource in the server. The server will assign its own id and that id will be a GUID.
    /// For example: 
    /// http://SomeServer.net/fhirapi/Patient
    /// Where the HTTP body is a FHIR Patient resource instance in JSON or XML format.
    /// If the client wishes to have control over the id of a newly submitted Resource, it should use the PUT (update) interaction instead.     
    /// </summary>
    /// <param name="ResourceName">The name of the FHIR resource that is contained in the HTTP body</param>
    /// <param name="resource">The actual Resource in the HTTP body</param>
    /// <returns>Status Code 200 (OK) and an echo of the created FHIR resource or an OperationOutcome resource if an error has been encountered.</returns>
    [HttpPost, Route("{ResourceName}")]
    [ActionLog]
    public HttpResponseMessage Post(string ResourceName, [FromBody] FhirModel.Resource resource)
    {
      IResourceServices oService = _FhirServiceNegotiator.GetResourceService(ResourceName);
      string BaseRequestUri = this.CalculateBaseURI("{ResourceName}");
      IDtoRootUrlStore DtoRootUrlStore = Services.PrimaryServiceRootFactory.Create(oService as ICommonServices, BaseRequestUri);
      IFhirRequestUri FhirRequestUri = Common.CommonFactory.GetFhirRequestUri(DtoRootUrlStore.Url, Request.RequestUri.OriginalString);
      IDtoRequestUri DtoRequestUri = Common.CommonFactory.GetRequestUri(DtoRootUrlStore, FhirRequestUri);      
      IDtoRequestHeaders RequestHeaders = Common.CommonFactory.GetDtoRequestHeaders(Request.Headers); 
      IDtoSearchParameterGeneric SearchParameterGeneric = Common.CommonFactory.GetDtoSearchParameterGeneric(Request.GetSearchParams());
      IResourceServiceRequestPost ResourceServiceRequestPost = Common.CommonFactory.GetResourceServiceRequestPost(resource, DtoRequestUri, SearchParameterGeneric, RequestHeaders);
      IResourceServiceOutcome ResourceServiceOutcome = oService.Post(ResourceServiceRequestPost);
      return FhirRestResponse.GetHttpResponseMessage(ResourceServiceOutcome, Request, SearchParameterGeneric.SummaryType);
    }

    // Post Form Data Search
    // POST: URL/FhirApi/Patient/_search
    /// <summary>
    /// Search for a set of resources by providing the search parameters as form data (Content-Type: application/x-www-form-urlencoded).
    /// </summary>
    /// <param name="ResourceName">The name of a FHIR Resource, for example 'Patient'</param>
    /// <param name="FormDataCollection">The HTTP body is to be search parameters in a form data format.</param>
    /// <returns>Returns a bundle containing all resource that match the search criteria.</returns>
    [HttpPost, Route("{ResourceName}/_search")]
    [ActionLog]
    public HttpResponseMessage PostFormSearch(string ResourceName, [FromBody] FormDataCollection FormDataCollection)
    {
      IResourceServices oService = _FhirServiceNegotiator.GetResourceService(ResourceName);
      string BaseRequestUri = this.CalculateBaseURI("{ResourceName}");
      IDtoRootUrlStore DtoRootUrlStore = Services.PrimaryServiceRootFactory.Create(oService as ICommonServices, BaseRequestUri);
      IFhirRequestUri FhirRequestUri = Common.CommonFactory.GetFhirRequestUri(DtoRootUrlStore.Url, Request.RequestUri.OriginalString);
      IDtoRequestUri DtoRequestUri = Common.CommonFactory.GetRequestUri(DtoRootUrlStore, FhirRequestUri);      
      IDtoSearchParameterGeneric SearchParameterGeneric = Common.CommonFactory.GetDtoSearchParameterGeneric(Hl7.Fhir.Rest.SearchParams.FromUriParamList(FormDataCollection.GetAsTupleCollection()));
      IResourceServiceRequestGetSearch ResourceServiceRequestGetSearch = Common.CommonFactory.GetResourceServiceRequestGetSearch(DtoRequestUri, SearchParameterGeneric);
      IResourceServiceOutcome oPyroServiceOperationOutcome = oService.GetSearch(ResourceServiceRequestGetSearch);
      return FhirRestResponse.GetHttpResponseMessage(oPyroServiceOperationOutcome, Request, SearchParameterGeneric.SummaryType);
    }

    //Update
    // PUT: URL/FhirApi/Patient/5
    /// <summary>
    /// Updates creates a new current version for an existing resource or creates an initial version if no resource already exists for the given id.
    /// For example: 
    /// http://SomeServer.net/fhirapi/Patient/123456
    /// Where the HTTP body is a FHIR Patient resource instance in JSON or XML format and the id in the resource equals the [id] in the URL, 123456 in this case.
    /// </summary>
    /// <param name="ResourceName">The name of the FHIR resource that is contained in the HTTP body</param>
    /// <param name="id">The FHIR Resource's id</param>
    /// <param name="resource">The actual Resource in the HTTP body</param>
    /// <returns>Status Code 200 (OK) and an echo of the created FHIR resource in the HTTP body or an OperationOutcome resource if an error has been encountered.</returns>
    [HttpPut, Route("{ResourceName}/{id}")]
    [ActionLog]
    public HttpResponseMessage Put(string ResourceName, string id, [FromBody] FhirModel.Resource resource)
    {
      IResourceServices oService = _FhirServiceNegotiator.GetResourceService(ResourceName);
      string BaseRequestUri = this.CalculateBaseURI("{ResourceName}");
      IDtoRootUrlStore DtoRootUrlStore = Services.PrimaryServiceRootFactory.Create(oService as ICommonServices, BaseRequestUri);
      IFhirRequestUri FhirRequestUri = Common.CommonFactory.GetFhirRequestUri(DtoRootUrlStore.Url, Request.RequestUri.OriginalString);
      IDtoRequestUri DtoRequestUri = Common.CommonFactory.GetRequestUri(DtoRootUrlStore, FhirRequestUri);      
      IDtoSearchParameterGeneric SearchParameterGeneric = Common.CommonFactory.GetDtoSearchParameterGeneric(Request.GetSearchParams());
      IDtoRequestHeaders RequestHeaders = Common.CommonFactory.GetDtoRequestHeaders(Request.Headers);
      IResourceServiceRequestPut ResourceServiceRequestPut = Common.CommonFactory.GetResourceServiceRequestPut(id, resource, DtoRequestUri, SearchParameterGeneric, RequestHeaders);
      IResourceServiceOutcome ResourceServiceOutcome = oService.Put(ResourceServiceRequestPut);
      return FhirRestResponse.GetHttpResponseMessage(ResourceServiceOutcome, Request, SearchParameterGeneric.SummaryType);
    }

    //Delete
    // DELETE: URL/FhirApi/Patient/5
    /// <summary>
    /// Deletes removes an existing resource at the given id.
    /// This server supports version history. 
    /// The delete interaction does not remove a resource's version history. From a version history respect, deleting a resource is the equivalent of creating a special kind of history entry that has no content and is marked as deleted. 
    /// </summary>
    /// <param name="ResourceName">The name of the FHIR resource that is being removed.</param>
    /// <param name="id">The FHIR Resource's id to be removed.</param>
    /// <returns>HTTP Status code 204 (No Content)</returns>
    [HttpDelete, Route("{ResourceName}/{id}")]
    [ActionLog]
    public HttpResponseMessage Delete(string ResourceName, string id)
    {
      IResourceServices oService = _FhirServiceNegotiator.GetResourceService(ResourceName);
      string BaseRequestUri = this.CalculateBaseURI("{ResourceName}");
      IDtoRootUrlStore DtoRootUrlStore = Services.PrimaryServiceRootFactory.Create(oService as ICommonServices, BaseRequestUri);
      IFhirRequestUri FhirRequestUri = Common.CommonFactory.GetFhirRequestUri(DtoRootUrlStore.Url, Request.RequestUri.OriginalString);
      IDtoRequestUri DtoRequestUri = Common.CommonFactory.GetRequestUri(DtoRootUrlStore, FhirRequestUri);      
      IDtoSearchParameterGeneric SearchParameterGeneric = Common.CommonFactory.GetDtoSearchParameterGeneric(Request.GetSearchParams());
      IResourceServiceRequestDelete ResourceServiceRequestDelete = Common.CommonFactory.GetResourceServiceRequestDelete(id, DtoRequestUri, SearchParameterGeneric);
      IResourceServiceOutcome ResourceServiceOutcome = oService.Delete(ResourceServiceRequestDelete);
      return FhirRestResponse.GetHttpResponseMessage(ResourceServiceOutcome, Request, SearchParameterGeneric.SummaryType);
    }

    //Conditional Update
    // PUT: URL/FhirApi/Patient/5
    /// <summary>
    /// Conditional Update takes a set of search parameters and if a single resource is located then it will update this resource as a new version.
    /// If many resources are located then a HTTP Status '412: Precondition Failed' will be returned. 
    /// If no resource is located then a create is performed creating an initial version.
    /// For example: 
    /// http://SomeServer.net/fhirapi/Patient?identifer=www.acmehealth.com.au/identifier/mrn|123456
    /// Where the HTTP body is a FHIR Patient resource instance in JSON or XML format search for resources with an 'identifier' with the system of 'www.acmehealth.com.au/identifier/mrn' and value of '123456' 
    /// and update if a single resource is found or create if not resource is found (Server will assign it's own id). If many resource are found by the search
    /// then a http status code of '412: Precondition Failed' will be returned.
    /// </summary>
    /// <param name="ResourceName">The name of the FHIR resource that is contained in the HTTP body</param>    
    /// <param name="resource">The actual Resource in the HTTP body</param>
    /// <returns>Status Code 200 (OK) and an echo of the created FHIR resource in the HTTP body or, 412: Precondition Failed or an OperationOutcome resource if an error has been encountered.</returns>
    [HttpPut, Route("{ResourceName}")]
    [ActionLog]
    public HttpResponseMessage ConditionalPut(string ResourceName, [FromBody] FhirModel.Resource resource)
    {
      IResourceServices oService = _FhirServiceNegotiator.GetResourceService(ResourceName);
      string BaseRequestUri = this.CalculateBaseURI("{ResourceName}");
      IDtoRootUrlStore DtoRootUrlStore = Services.PrimaryServiceRootFactory.Create(oService as ICommonServices, BaseRequestUri);
      IFhirRequestUri FhirRequestUri = Common.CommonFactory.GetFhirRequestUri(DtoRootUrlStore.Url, Request.RequestUri.OriginalString);
      IDtoRequestUri DtoRequestUri = Common.CommonFactory.GetRequestUri(DtoRootUrlStore, FhirRequestUri);      
      IDtoSearchParameterGeneric SearchParameterGeneric = Common.CommonFactory.GetDtoSearchParameterGeneric(Request.GetSearchParams());
      IResourceServiceRequestConditionalPut ResourceServiceRequestConditionalPut = Common.CommonFactory.GetResourceServiceRequestConditionalPut(resource, DtoRequestUri, SearchParameterGeneric);
      IResourceServiceOutcome ResourceServiceOutcome = oService.ConditionalPut(ResourceServiceRequestConditionalPut);
      return FhirRestResponse.GetHttpResponseMessage(ResourceServiceOutcome, Request, SearchParameterGeneric.SummaryType);
    }

    //Delete
    // DELETE: URL/FhirApi/Patient/5
    /// <summary>
    /// Conditional Delete removes an existing resource or set of Resources given a set of search parameters for the given Resource.
    /// This operation must be given at least one Resources defined search parameter to work and all parameter must be understood by this server.
    /// If not a HTTP status code of 412 Precondition Failed will be returned. 
    /// This server supports version history. 
    /// The delete interaction does not remove a resource's version history. From a version history respect, deleting a resource is the equivalent of creating a special kind of history entry that has no content and is marked as deleted. 
    /// </summary>
    /// <param name="ResourceName">The name of the FHIR resource that is being removed.</param>    
    /// <returns>HTTP Status codes: 204 (No Content) or 404 Not Found or 412 Precondition Failed</returns>
    [HttpDelete, Route("{ResourceName}")]
    [ActionLog]
    public HttpResponseMessage ConditionalDelete(string ResourceName)
    {
      IResourceServices oService = _FhirServiceNegotiator.GetResourceService(ResourceName);
      string BaseRequestUri = this.CalculateBaseURI("{ResourceName}");
      IDtoRootUrlStore DtoRootUrlStore = Services.PrimaryServiceRootFactory.Create(oService as ICommonServices, BaseRequestUri);
      IFhirRequestUri FhirRequestUri = Common.CommonFactory.GetFhirRequestUri(DtoRootUrlStore.Url, Request.RequestUri.OriginalString);
      IDtoRequestUri DtoRequestUri = Common.CommonFactory.GetRequestUri(DtoRootUrlStore, FhirRequestUri);      
      IDtoSearchParameterGeneric SearchParameterGeneric = Common.CommonFactory.GetDtoSearchParameterGeneric(Request.GetSearchParams());
      IResourceServiceRequestConditionalDelete ResourceServiceRequestConditionalDelete = Common.CommonFactory.GetResourceServiceRequestConditionalDelete(DtoRequestUri, SearchParameterGeneric);
      IResourceServiceOutcome ResourceServiceOutcome = oService.ConditionalDelete(ResourceServiceRequestConditionalDelete);
      return FhirRestResponse.GetHttpResponseMessage(ResourceServiceOutcome, Request, SearchParameterGeneric.SummaryType);
    }


    //Resource Operations
    // DELETE: URL/FhirApi/Patient/5
    /// <summary>
    /// Resource operation endpont. This is for operations that are to be perfomed on a Resource type, for example: Patient
    /// </summary>
    /// <param name="ResourceName">The name of the FHIR resource that the operation apllies to.</param>
    /// <param name="operation">The name of the operation, must be prefixed wiht a '$'. For example: '$my-operation-name' </param>
    /// <param name="Resource">Typicaly this is a Parameters resource given in the body</param>
    /// <returns></returns>
    [HttpPost, Route("{ResourceName}/${operation}")]
    [ActionLog]
    public HttpResponseMessage PerformResourceOperationWithParameters(string ResourceName, string operation, [FromBody] FhirModel.Resource Resource)
    {
      IResourceServices oService = _FhirServiceNegotiator.GetResourceService(ResourceName);
      string BaseRequestUri = this.CalculateBaseURI("{ResourceName}");
      IDtoRootUrlStore DtoRootUrlStore = Services.PrimaryServiceRootFactory.Create(oService as ICommonServices, BaseRequestUri);
      IFhirRequestUri FhirRequestUri = Common.CommonFactory.GetFhirRequestUri(DtoRootUrlStore.Url, Request.RequestUri.OriginalString);
      IDtoRequestUri DtoRequestUri = Common.CommonFactory.GetRequestUri(DtoRootUrlStore, FhirRequestUri);            
      IDtoSearchParameterGeneric SearchParameterGeneric = Common.CommonFactory.GetDtoSearchParameterGeneric(Request.GetSearchParams());
      IDtoRequestHeaders RequestHeaders = Common.CommonFactory.GetDtoRequestHeaders(Request.Headers);      
      IResourceOperationsServiceRequest ResourceOperationsServiceRequest = Common.CommonFactory.GetResourceOperationsServiceRequest(operation, Resource, oService, DtoRequestUri, SearchParameterGeneric, RequestHeaders);
      var OperationService = new Engine.Services.FhirResourceOperationService();
      IResourceServiceOutcome ResourceServiceOutcome = OperationService.Process(ResourceOperationsServiceRequest);      
      return FhirRestResponse.GetHttpResponseMessage(ResourceServiceOutcome, Request, SearchParameterGeneric.SummaryType);
    }

    
  }

  public static class Util
  {
    public static string CalculateBaseURI(this System.Web.Http.ApiController me, string resourceName)
    {
      System.Uri ri = me.ControllerContext.Request.RequestUri;
      if (resourceName == "metadata" || resourceName == "${operation}")
      {
        return System.String.Format("{0}://{1}{2}{3}{4}",
            ri.Scheme,
            ri.Host,
            ri.IsDefaultPort ? "" : ":" + ri.Port.ToString(),
            me.ControllerContext.RequestContext.VirtualPathRoot.TrimEnd('/') + '/',
            me.ControllerContext.RouteData.Route.RouteTemplate.Replace("/metadata", "").Replace("/${operation}", ""));
      }

      if (me.ControllerContext.RouteData.Route.RouteTemplate.Contains("{ResourceName}"))
        resourceName = "{ResourceName}";
      string baseUri = System.String.Format("{0}://{1}{2}{3}{4}",
          ri.Scheme,
          ri.Host,
          ri.IsDefaultPort ? "" : ":" + ri.Port.ToString(),
          me.ControllerContext.RequestContext.VirtualPathRoot.TrimEnd('/') + '/',
          me.ControllerContext.RouteData.Route.RouteTemplate.Substring(0, me.ControllerContext.RouteData.Route.RouteTemplate.LastIndexOf(resourceName)));

      return baseUri.TrimEnd('/');
    }

  }
}
