﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Blaze.DataModel.DatabaseModel;
using Blaze.DataModel.DatabaseModel.Base;
using Blaze.DataModel.Support;
using Blaze.DataModel.IndexSetter;
using Blaze.DataModel.Search;
using Hl7.Fhir.Model;
using Blaze.Common.BusinessEntities.Search;
using Blaze.Common.Interfaces;
using Blaze.Common.Interfaces.Repositories;
using Blaze.Common.Interfaces.UriSupport;
using Hl7.Fhir.Introspection;

namespace Blaze.DataModel.Repository
{
  public partial class ImplementationGuideRepository : CommonRepository, IResourceRepository
  {
    public ImplementationGuideRepository(DataModel.DatabaseModel.DatabaseContext Context) : base(Context) { }

    public IDatabaseOperationOutcome GetResourceBySearch(DtoSearchParameters DtoSearchParameters)
    {
      var Predicate = PredicateGenerator<Res_ImplementationGuide>(DtoSearchParameters);
      int TotalRecordCount = DbGetALLCount<Res_ImplementationGuide>(Predicate);
      var Query = DbGetAll<Res_ImplementationGuide>(Predicate);

      //Todo: Sort not implemented just defaulting to last update order
      Query = Query.OrderBy(x => x.lastUpdated);      
      int ClaculatedPageRequired = PaginationSupport.CalculatePageRequired(DtoSearchParameters.RequiredPageNumber, _NumberOfRecordsPerPage, TotalRecordCount);
      
      Query = Query.Paging(ClaculatedPageRequired, _NumberOfRecordsPerPage);
      var DtoResourceList = new List<Common.BusinessEntities.Dto.DtoResource>();
      Query.ToList().ForEach(x => DtoResourceList.Add(IndexSettingSupport.SetDtoResource(x)));

      IDatabaseOperationOutcome DatabaseOperationOutcome = new DatabaseOperationOutcome();
      DatabaseOperationOutcome.SingleResourceRead = false;
      DatabaseOperationOutcome.PagesTotal = PaginationSupport.CalculateTotalPages(_NumberOfRecordsPerPage, TotalRecordCount); ;
      DatabaseOperationOutcome.PageRequested = ClaculatedPageRequired;
      DatabaseOperationOutcome.ReturnedResourceCount = TotalRecordCount;
      DatabaseOperationOutcome.ReturnedResourceList = DtoResourceList;


      return DatabaseOperationOutcome;  
    }

    public IDatabaseOperationOutcome AddResource(Resource Resource, IDtoFhirRequestUri FhirRequestUri)
    {
      var ResourceTyped = Resource as ImplementationGuide;
      var ResourceEntity = new Res_ImplementationGuide();
      this.PopulateResourceEntity(ResourceEntity, "1", ResourceTyped, FhirRequestUri);
      this.DbAddEntity<Res_ImplementationGuide>(ResourceEntity);
      IDatabaseOperationOutcome DatabaseOperationOutcome = new DatabaseOperationOutcome();
      DatabaseOperationOutcome.SingleResourceRead = true;     
      DatabaseOperationOutcome.ReturnedResource = IndexSettingSupport.SetDtoResource(ResourceEntity);
      DatabaseOperationOutcome.ReturnedResourceCount = 1;
      return DatabaseOperationOutcome;
    }

    public IDatabaseOperationOutcome UpdateResource(string ResourceVersion, Resource Resource, IDtoFhirRequestUri FhirRequestUri)
    {
      var ResourceTyped = Resource as ImplementationGuide;
      var ResourceEntity = LoadCurrentResourceEntity(Resource.Id);
      var ResourceHistoryEntity = new Res_ImplementationGuide_History();  
      IndexSettingSupport.SetHistoryResourceEntity(ResourceEntity, ResourceHistoryEntity);
      ResourceEntity.Res_ImplementationGuide_History_List.Add(ResourceHistoryEntity); 
      this.ResetResourceEntity(ResourceEntity);
      this.PopulateResourceEntity(ResourceEntity, ResourceVersion, ResourceTyped, FhirRequestUri);            
      this.Save();            
      IDatabaseOperationOutcome DatabaseOperationOutcome = new DatabaseOperationOutcome();
      DatabaseOperationOutcome.SingleResourceRead = true;
      DatabaseOperationOutcome.ReturnedResource = IndexSettingSupport.SetDtoResource(ResourceEntity);
      DatabaseOperationOutcome.ReturnedResourceCount = 1;
      return DatabaseOperationOutcome;
    }

    public void UpdateResouceAsDeleted(string FhirResourceId, string ResourceVersion)
    {
      var ResourceEntity = this.LoadCurrentResourceEntity(FhirResourceId);
      var ResourceHistoryEntity = new Res_ImplementationGuide_History();
      IndexSettingSupport.SetHistoryResourceEntity(ResourceEntity, ResourceHistoryEntity);
      ResourceEntity.Res_ImplementationGuide_History_List.Add(ResourceHistoryEntity);
      this.ResetResourceEntity(ResourceEntity);
      ResourceEntity.IsDeleted = true;
      ResourceEntity.versionId = ResourceVersion;
      ResourceEntity.XmlBlob = string.Empty;
      this.Save();      
    }

    public IDatabaseOperationOutcome GetResourceByFhirIDAndVersionNumber(string FhirResourceId, string ResourceVersionNumber)
    {
      IDatabaseOperationOutcome DatabaseOperationOutcome = new DatabaseOperationOutcome();
      DatabaseOperationOutcome.SingleResourceRead = true;
      var ResourceHistoryEntity = DbGet<Res_ImplementationGuide_History>(x => x.FhirId == FhirResourceId && x.versionId == ResourceVersionNumber);
      if (ResourceHistoryEntity != null)
      {
        DatabaseOperationOutcome.ReturnedResource = IndexSettingSupport.SetDtoResource(ResourceHistoryEntity);
      }
      else
      {
        var ResourceEntity = DbGet<Res_ImplementationGuide>(x => x.FhirId == FhirResourceId && x.versionId == ResourceVersionNumber);
        if (ResourceEntity != null)
          DatabaseOperationOutcome.ReturnedResource = IndexSettingSupport.SetDtoResource(ResourceEntity);        
      }
      return DatabaseOperationOutcome;
    }

    public IDatabaseOperationOutcome GetResourceByFhirID(string FhirResourceId, bool WithXml = false)
    {
      IDatabaseOperationOutcome DatabaseOperationOutcome = new DatabaseOperationOutcome();
      DatabaseOperationOutcome.SingleResourceRead = true;
      Blaze.Common.BusinessEntities.Dto.DtoResource DtoResource = null;
      if (WithXml)
      {        
        DtoResource = DbGetAll<Res_ImplementationGuide>(x => x.FhirId == FhirResourceId).Select(x => new Blaze.Common.BusinessEntities.Dto.DtoResource { FhirId = x.FhirId, IsDeleted = x.IsDeleted, IsCurrent = true, Version = x.versionId, Received = x.lastUpdated, Xml = x.XmlBlob }).SingleOrDefault();       
      }
      else
      {
        DtoResource = DbGetAll<Res_ImplementationGuide>(x => x.FhirId == FhirResourceId).Select(x => new Blaze.Common.BusinessEntities.Dto.DtoResource { FhirId = x.FhirId, IsDeleted = x.IsDeleted, IsCurrent = true, Version = x.versionId, Received = x.lastUpdated }).SingleOrDefault();        
      }
      DatabaseOperationOutcome.ReturnedResource = DtoResource;
      return DatabaseOperationOutcome;
    }

    private Res_ImplementationGuide LoadCurrentResourceEntity(string FhirId)
    {

      var IncludeList = new List<Expression<Func<Res_ImplementationGuide, object>>>();
      IncludeList.Add(x => x.context_List);
      IncludeList.Add(x => x.dependency_List);
      IncludeList.Add(x => x.resource_List);
      IncludeList.Add(x => x.resource_List);
      IncludeList.Add(x => x._profile_List);
      IncludeList.Add(x => x._security_List);
      IncludeList.Add(x => x._tag_List);
    
      var ResourceEntity = DbQueryEntityWithInclude<Res_ImplementationGuide>(x => x.FhirId == FhirId, IncludeList);

      return ResourceEntity;
    }


    private void ResetResourceEntity(Res_ImplementationGuide ResourceEntity)
    {
      ResourceEntity.date_DateTimeOffset = null;      
      ResourceEntity.description_String = null;      
      ResourceEntity.experimental_Code = null;      
      ResourceEntity.experimental_System = null;      
      ResourceEntity.name_String = null;      
      ResourceEntity.publisher_String = null;      
      ResourceEntity.status_Code = null;      
      ResourceEntity.status_System = null;      
      ResourceEntity.url_Uri = null;      
      ResourceEntity.version_Code = null;      
      ResourceEntity.version_System = null;      
      ResourceEntity.XmlBlob = null;      
 
      
      _Context.Res_ImplementationGuide_Index_context.RemoveRange(ResourceEntity.context_List);            
      _Context.Res_ImplementationGuide_Index_dependency.RemoveRange(ResourceEntity.dependency_List);            
      _Context.Res_ImplementationGuide_Index_resource.RemoveRange(ResourceEntity.resource_List);            
      _Context.Res_ImplementationGuide_Index_resource.RemoveRange(ResourceEntity.resource_List);            
      _Context.Res_ImplementationGuide_Index__profile.RemoveRange(ResourceEntity._profile_List);            
      _Context.Res_ImplementationGuide_Index__security.RemoveRange(ResourceEntity._security_List);            
      _Context.Res_ImplementationGuide_Index__tag.RemoveRange(ResourceEntity._tag_List);            
 
    }

    private void PopulateResourceEntity(Res_ImplementationGuide ResourseEntity, string ResourceVersion, ImplementationGuide ResourceTyped, IDtoFhirRequestUri FhirRequestUri)
    {
       IndexSettingSupport.SetResourceBaseAddOrUpdate(ResourceTyped, ResourseEntity, ResourceVersion, false);

          if (ResourceTyped.Date != null)
      {
        if (ResourceTyped.DateElement is Hl7.Fhir.Model.FhirDateTime)
        {
          var Index = new DateTimeIndex();
          Index = IndexSetterFactory.Create(typeof(DateTimeIndex)).Set(ResourceTyped.DateElement, Index) as DateTimeIndex;
          if (Index != null)
          {
            ResourseEntity.date_DateTimeOffset = Index.DateTimeOffset;
          }
        }
      }

      if (ResourceTyped.Description != null)
      {
        if (ResourceTyped.Description is Hl7.Fhir.Model.Markdown)
        {
          var Index = new StringIndex();
          Index = IndexSetterFactory.Create(typeof(StringIndex)).Set(ResourceTyped.Description, Index) as StringIndex;
          if (Index != null)
          {
            ResourseEntity.description_String = Index.String;
          }
        }
      }

      if (ResourceTyped.Experimental != null)
      {
        if (ResourceTyped.ExperimentalElement is Hl7.Fhir.Model.FhirBoolean)
        {
          var Index = new TokenIndex();
          Index = IndexSetterFactory.Create(typeof(TokenIndex)).Set(ResourceTyped.ExperimentalElement, Index) as TokenIndex;
          if (Index != null)
          {
            ResourseEntity.experimental_Code = Index.Code;
            ResourseEntity.experimental_System = Index.System;
          }
        }
      }

      if (ResourceTyped.Name != null)
      {
        if (ResourceTyped.NameElement is Hl7.Fhir.Model.FhirString)
        {
          var Index = new StringIndex();
          Index = IndexSetterFactory.Create(typeof(StringIndex)).Set(ResourceTyped.NameElement, Index) as StringIndex;
          if (Index != null)
          {
            ResourseEntity.name_String = Index.String;
          }
        }
      }

      if (ResourceTyped.Publisher != null)
      {
        if (ResourceTyped.PublisherElement is Hl7.Fhir.Model.FhirString)
        {
          var Index = new StringIndex();
          Index = IndexSetterFactory.Create(typeof(StringIndex)).Set(ResourceTyped.PublisherElement, Index) as StringIndex;
          if (Index != null)
          {
            ResourseEntity.publisher_String = Index.String;
          }
        }
      }

      if (ResourceTyped.Status != null)
      {
        if (ResourceTyped.StatusElement is Hl7.Fhir.Model.Code<Hl7.Fhir.Model.ConformanceResourceStatus>)
        {
          var Index = new TokenIndex();
          Index = IndexSetterFactory.Create(typeof(TokenIndex)).Set(ResourceTyped.StatusElement, Index) as TokenIndex;
          if (Index != null)
          {
            ResourseEntity.status_Code = Index.Code;
            ResourseEntity.status_System = Index.System;
          }
        }
      }

      if (ResourceTyped.Url != null)
      {
        if (ResourceTyped.UrlElement is Hl7.Fhir.Model.FhirUri)
        {
          var Index = new UriIndex();
          Index = IndexSetterFactory.Create(typeof(UriIndex)).Set(ResourceTyped.UrlElement, Index) as UriIndex;
          if (Index != null)
          {
            ResourseEntity.url_Uri = Index.Uri;
          }
        }
      }

      if (ResourceTyped.Version != null)
      {
        if (ResourceTyped.VersionElement is Hl7.Fhir.Model.FhirString)
        {
          var Index = new TokenIndex();
          Index = IndexSetterFactory.Create(typeof(TokenIndex)).Set(ResourceTyped.VersionElement, Index) as TokenIndex;
          if (Index != null)
          {
            ResourseEntity.version_Code = Index.Code;
            ResourseEntity.version_System = Index.System;
          }
        }
      }

      if (ResourceTyped.UseContext != null)
      {
        foreach (var item3 in ResourceTyped.UseContext)
        {
          if (item3 != null)
          {
            foreach (var item4 in item3.Coding)
            {
              var Index = new Res_ImplementationGuide_Index_context();
              Index = IndexSetterFactory.Create(typeof(TokenIndex)).Set(item4, Index) as Res_ImplementationGuide_Index_context;
              ResourseEntity.context_List.Add(Index);
            }
          }
        }
      }

      foreach (var item1 in ResourceTyped.Dependency)
      {
        if (item1.Uri != null)
        {
          if (item1.UriElement is Hl7.Fhir.Model.FhirUri)
          {
            var Index = new Res_ImplementationGuide_Index_dependency();
            Index = IndexSetterFactory.Create(typeof(UriIndex)).Set(item1.UriElement, Index) as Res_ImplementationGuide_Index_dependency;
            ResourseEntity.dependency_List.Add(Index);
          }
        }
      }

      foreach (var item1 in ResourceTyped.Package)
      {
        foreach (var item2 in item1.Resource)
        {
          if (item2.Source != null)
          {
            if (item2.Source is Hl7.Fhir.Model.FhirUri)
            {
              var Index = new Res_ImplementationGuide_Index_resource();
              Index = IndexSetterFactory.Create(typeof(ReferenceIndex)).Set(item2.Source, Index) as Res_ImplementationGuide_Index_resource;
              ResourseEntity.resource_List.Add(Index);
            }
          }
        }
      }

      foreach (var item1 in ResourceTyped.Package)
      {
        foreach (var item2 in item1.Resource)
        {
          if (item2.Source != null)
          {
            if (item2.Source is ResourceReference)
            {
              var Index = new Res_ImplementationGuide_Index_resource();
              Index = IndexSetterFactory.Create(typeof(ReferenceIndex)).Set(item2.Source, Index, FhirRequestUri, this) as Res_ImplementationGuide_Index_resource;
              if (Index != null)
              {
                ResourseEntity.resource_List.Add(Index);
              }
            }
          }
        }
      }

      if (ResourceTyped.Meta != null)
      {
        if (ResourceTyped.Meta.Profile != null)
        {
          foreach (var item4 in ResourceTyped.Meta.ProfileElement)
          {
            if (item4 is Hl7.Fhir.Model.FhirUri)
            {
              var Index = new Res_ImplementationGuide_Index__profile();
              Index = IndexSetterFactory.Create(typeof(UriIndex)).Set(item4, Index) as Res_ImplementationGuide_Index__profile;
              ResourseEntity._profile_List.Add(Index);
            }
          }
        }
      }

      if (ResourceTyped.Meta != null)
      {
        if (ResourceTyped.Meta.Security != null)
        {
          foreach (var item4 in ResourceTyped.Meta.Security)
          {
            if (item4 is Hl7.Fhir.Model.Coding)
            {
              var Index = new Res_ImplementationGuide_Index__security();
              Index = IndexSetterFactory.Create(typeof(TokenIndex)).Set(item4, Index) as Res_ImplementationGuide_Index__security;
              ResourseEntity._security_List.Add(Index);
            }
          }
        }
      }

      if (ResourceTyped.Meta != null)
      {
        if (ResourceTyped.Meta.Tag != null)
        {
          foreach (var item4 in ResourceTyped.Meta.Tag)
          {
            if (item4 is Hl7.Fhir.Model.Coding)
            {
              var Index = new Res_ImplementationGuide_Index__tag();
              Index = IndexSetterFactory.Create(typeof(TokenIndex)).Set(item4, Index) as Res_ImplementationGuide_Index__tag;
              ResourseEntity._tag_List.Add(Index);
            }
          }
        }
      }


      

    }


  }
} 

