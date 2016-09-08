﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Blaze.DataModel.DatabaseModel;
using Blaze.DataModel.DatabaseModel.Base;
using Blaze.DataModel.Support;
using Blaze.DataModel.IndexSetter;
using Hl7.Fhir.Model;
using Blaze.Common.BusinessEntities.Search;
using Blaze.Common.Interfaces;
using Blaze.Common.Interfaces.Repositories;
using Blaze.Common.Interfaces.UriSupport;
using Hl7.Fhir.Introspection;

namespace Blaze.DataModel.Repository
{
  public partial class HealthcareServiceRepository : CommonRepository, IResourceRepository
  {

    public HealthcareServiceRepository(DataModel.DatabaseModel.DatabaseContext Context) : base(Context) { }

    public IDatabaseOperationOutcome GetResourceBySearch(DtoSearchParameters DtoSearchParameters)
    {
      IDatabaseOperationOutcome DatabaseOperationOutcome = new DatabaseOperationOutcome();
      DatabaseOperationOutcome.SingleResourceRead = true;
      throw new NotImplementedException("Resource Search not implemented in Db layer");
    }

    public IDatabaseOperationOutcome AddResource(Resource Resource, IDtoFhirRequestUri FhirRequestUri)
    {
      var ResourceTyped = Resource as HealthcareService;
      var ResourceEntity = new Res_HealthcareService();
      this.PopulateResourceEntity(ResourceEntity, "1", ResourceTyped, FhirRequestUri);
      this.DbAddEntity<Res_HealthcareService>(ResourceEntity);
      IDatabaseOperationOutcome DatabaseOperationOutcome = new DatabaseOperationOutcome();
      DatabaseOperationOutcome.SingleResourceRead = true;     
      DatabaseOperationOutcome.ResourceMatchingSearch = IndexSettingSupport.SetDtoResource(ResourceEntity);
      DatabaseOperationOutcome.ResourcesMatchingSearchCount = 1;
      return DatabaseOperationOutcome;
    }

    public IDatabaseOperationOutcome UpdateResource(string ResourceVersion, Resource Resource, IDtoFhirRequestUri FhirRequestUri)
    {
      var ResourceTyped = Resource as HealthcareService;
      var ResourceEntity = LoadCurrentResourceEntity(Resource.Id);
      var ResourceHistoryEntity = new Res_HealthcareService_History();  
      IndexSettingSupport.SetHistoryResourceEntity(ResourceEntity, ResourceHistoryEntity);
      ResourceEntity.Res_HealthcareService_History_List.Add(ResourceHistoryEntity); 
      this.ResetResourceEntity(ResourceEntity);
      this.PopulateResourceEntity(ResourceEntity, ResourceVersion, ResourceTyped, FhirRequestUri);            
      this.Save();            
      IDatabaseOperationOutcome DatabaseOperationOutcome = new DatabaseOperationOutcome();
      DatabaseOperationOutcome.SingleResourceRead = true;
      DatabaseOperationOutcome.ResourceMatchingSearch = IndexSettingSupport.SetDtoResource(ResourceEntity);
      DatabaseOperationOutcome.ResourcesMatchingSearchCount = 1;
      return DatabaseOperationOutcome;
    }

    public void UpdateResouceAsDeleted(string FhirResourceId, string ResourceVersion)
    {
      var ResourceEntity = this.LoadCurrentResourceEntity(FhirResourceId);
      var ResourceHistoryEntity = new Res_HealthcareService_History();
      IndexSettingSupport.SetHistoryResourceEntity(ResourceEntity, ResourceHistoryEntity);
      ResourceEntity.Res_HealthcareService_History_List.Add(ResourceHistoryEntity);
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
      var ResourceHistoryEntity = DbGet<Res_HealthcareService_History>(x => x.FhirId == FhirResourceId && x.versionId == ResourceVersionNumber);
      if (ResourceHistoryEntity != null)
      {
        DatabaseOperationOutcome.ResourceMatchingSearch = IndexSettingSupport.SetDtoResource(ResourceHistoryEntity);
      }
      else
      {
        var ResourceEntity = DbGet<Res_HealthcareService>(x => x.FhirId == FhirResourceId && x.versionId == ResourceVersionNumber);
        if (ResourceEntity != null)
          DatabaseOperationOutcome.ResourceMatchingSearch = IndexSettingSupport.SetDtoResource(ResourceEntity);        
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
        DtoResource = DbGetAll<Res_HealthcareService>(x => x.FhirId == FhirResourceId).Select(x => new Blaze.Common.BusinessEntities.Dto.DtoResource { FhirId = x.FhirId, IsDeleted = x.IsDeleted, IsCurrent = true, Version = x.versionId, Received = x.lastUpdated, Xml = x.XmlBlob }).SingleOrDefault();       
      }
      else
      {
        DtoResource = DbGetAll<Res_HealthcareService>(x => x.FhirId == FhirResourceId).Select(x => new Blaze.Common.BusinessEntities.Dto.DtoResource { FhirId = x.FhirId, IsDeleted = x.IsDeleted, IsCurrent = true, Version = x.versionId, Received = x.lastUpdated }).SingleOrDefault();        
      }
      DatabaseOperationOutcome.ResourceMatchingSearch = DtoResource;
      return DatabaseOperationOutcome;
    }

    private Res_HealthcareService LoadCurrentResourceEntity(string FhirId)
    {

      var IncludeList = new List<Expression<Func<Res_HealthcareService, object>>>();
      IncludeList.Add(x => x.characteristic_List);
      IncludeList.Add(x => x.identifier_List);
      IncludeList.Add(x => x.location_List);
      IncludeList.Add(x => x.programname_List);
      IncludeList.Add(x => x.servicecategory_List);
      IncludeList.Add(x => x.servicetype_List);
      IncludeList.Add(x => x.profile_List);
      IncludeList.Add(x => x.security_List);
      IncludeList.Add(x => x.tag_List);
    
      var ResourceEntity = DbQueryEntityWithInclude<Res_HealthcareService>(x => x.FhirId == FhirId, IncludeList);

      return ResourceEntity;
    }


    private void ResetResourceEntity(Res_HealthcareService ResourceEntity)
    {
      ResourceEntity.name_String = null;      
      ResourceEntity.organization_VersionId = null;      
      ResourceEntity.organization_FhirId = null;      
      ResourceEntity.organization_Type = null;      
      ResourceEntity.organization_Url = null;      
      ResourceEntity.organization_ServiceRootURL_StoreID = null;      
      ResourceEntity.XmlBlob = null;      
 
      
      _Context.Res_HealthcareService_Index_characteristic.RemoveRange(ResourceEntity.characteristic_List);            
      _Context.Res_HealthcareService_Index_identifier.RemoveRange(ResourceEntity.identifier_List);            
      _Context.Res_HealthcareService_Index_location.RemoveRange(ResourceEntity.location_List);            
      _Context.Res_HealthcareService_Index_programname.RemoveRange(ResourceEntity.programname_List);            
      _Context.Res_HealthcareService_Index_servicecategory.RemoveRange(ResourceEntity.servicecategory_List);            
      _Context.Res_HealthcareService_Index_servicetype.RemoveRange(ResourceEntity.servicetype_List);            
      _Context.Res_HealthcareService_Index_profile.RemoveRange(ResourceEntity.profile_List);            
      _Context.Res_HealthcareService_Index_security.RemoveRange(ResourceEntity.security_List);            
      _Context.Res_HealthcareService_Index_tag.RemoveRange(ResourceEntity.tag_List);            
 
    }

    private void PopulateResourceEntity(Res_HealthcareService ResourseEntity, string ResourceVersion, HealthcareService ResourceTyped, IDtoFhirRequestUri FhirRequestUri)
    {
       IndexSettingSupport.SetResourceBaseAddOrUpdate(ResourceTyped, ResourseEntity, ResourceVersion, false);

          if (ResourceTyped.ServiceName != null)
      {
        if (ResourceTyped.ServiceNameElement is Hl7.Fhir.Model.FhirString)
        {
          var Index = new StringIndex();
          Index = IndexSetterFactory.Create(typeof(StringIndex)).Set(ResourceTyped.ServiceNameElement, Index) as StringIndex;
          if (Index != null)
          {
            ResourseEntity.name_String = Index.String;
          }
        }
      }

      if (ResourceTyped.ProvidedBy != null)
      {
        if (ResourceTyped.ProvidedBy is Hl7.Fhir.Model.ResourceReference)
        {
          var Index = new ReferenceIndex();
          Index = IndexSetterFactory.Create(typeof(ReferenceIndex)).Set(ResourceTyped.ProvidedBy, Index, FhirRequestUri, this) as ReferenceIndex;
          if (Index != null)
          {
            ResourseEntity.organization_Type = Index.Type;
            ResourseEntity.organization_FhirId = Index.FhirId;
            if (Index.Url != null)
            {
              ResourseEntity.organization_Url = Index.Url;
            }
            else
            {
              ResourseEntity.organization_ServiceRootURL_StoreID = Index.ServiceRootURL_StoreID;
            }
          }
        }
      }

      if (ResourceTyped.Characteristic != null)
      {
        foreach (var item3 in ResourceTyped.Characteristic)
        {
          if (item3 != null)
          {
            foreach (var item4 in item3.Coding)
            {
              var Index = new Res_HealthcareService_Index_characteristic();
              Index = IndexSetterFactory.Create(typeof(TokenIndex)).Set(item4, Index) as Res_HealthcareService_Index_characteristic;
              ResourseEntity.characteristic_List.Add(Index);
            }
          }
        }
      }

      if (ResourceTyped.Identifier != null)
      {
        foreach (var item3 in ResourceTyped.Identifier)
        {
          if (item3 is Hl7.Fhir.Model.Identifier)
          {
            var Index = new Res_HealthcareService_Index_identifier();
            Index = IndexSetterFactory.Create(typeof(TokenIndex)).Set(item3, Index) as Res_HealthcareService_Index_identifier;
            ResourseEntity.identifier_List.Add(Index);
          }
        }
      }

      if (ResourceTyped.Location != null)
      {
        foreach (var item in ResourceTyped.Location)
        {
          if (item is ResourceReference)
          {
            var Index = new Res_HealthcareService_Index_location();
            Index = IndexSetterFactory.Create(typeof(ReferenceIndex)).Set(item, Index, FhirRequestUri, this) as Res_HealthcareService_Index_location;
            if (Index != null)
            {
              ResourseEntity.location_List.Add(Index);
            }
          }
        }
      }

      if (ResourceTyped.ProgramName != null)
      {
        foreach (var item3 in ResourceTyped.ProgramNameElement)
        {
          if (item3 is Hl7.Fhir.Model.FhirString)
          {
            var Index = new Res_HealthcareService_Index_programname();
            Index = IndexSetterFactory.Create(typeof(StringIndex)).Set(item3, Index) as Res_HealthcareService_Index_programname;
            ResourseEntity.programname_List.Add(Index);
          }
        }
      }

      if (ResourceTyped.ServiceCategory != null)
      {
        foreach (var item3 in ResourceTyped.ServiceCategory.Coding)
        {
          var Index = new Res_HealthcareService_Index_servicecategory();
          Index = IndexSetterFactory.Create(typeof(TokenIndex)).Set(item3, Index) as Res_HealthcareService_Index_servicecategory;
          ResourseEntity.servicecategory_List.Add(Index);
        }
      }

      if (ResourceTyped.ServiceType != null)
      {
        foreach (var item3 in ResourceTyped.ServiceType)
        {
          if (item3 != null)
          {
            foreach (var item4 in item3.Coding)
            {
              var Index = new Res_HealthcareService_Index_servicetype();
              Index = IndexSetterFactory.Create(typeof(TokenIndex)).Set(item4, Index) as Res_HealthcareService_Index_servicetype;
              ResourseEntity.servicetype_List.Add(Index);
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
              var Index = new Res_HealthcareService_Index_profile();
              Index = IndexSetterFactory.Create(typeof(UriIndex)).Set(item4, Index) as Res_HealthcareService_Index_profile;
              ResourseEntity.profile_List.Add(Index);
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
              var Index = new Res_HealthcareService_Index_security();
              Index = IndexSetterFactory.Create(typeof(TokenIndex)).Set(item4, Index) as Res_HealthcareService_Index_security;
              ResourseEntity.security_List.Add(Index);
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
              var Index = new Res_HealthcareService_Index_tag();
              Index = IndexSetterFactory.Create(typeof(TokenIndex)).Set(item4, Index) as Res_HealthcareService_Index_tag;
              ResourseEntity.tag_List.Add(Index);
            }
          }
        }
      }


      

    }


  }
} 
