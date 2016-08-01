﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Linq.Expressions;
using Blaze.DataModel.DatabaseModel;
using Blaze.DataModel.DatabaseModel.Base;
using Blaze.DataModel.Support;
using Hl7.Fhir.Model;
using Blaze.Common.BusinessEntities;
using Blaze.Common.Interfaces;
using Blaze.Common.Interfaces.Repositories;
using Blaze.Common.Interfaces.UriSupport;
using Hl7.Fhir.Introspection;

namespace Blaze.DataModel.Repository
{
  public partial class ClaimRepository : CommonRepository, IResourceRepository
  {

    public ClaimRepository(DataModel.DatabaseModel.DatabaseContext Context) : base(Context) { }

    public IDatabaseOperationOutcome AddResource(Resource Resource, IDtoFhirRequestUri FhirRequestUri)
    {
      var ResourceTyped = Resource as Claim;
      var ResourceEntity = new Res_Claim();
      this.PopulateResourceEntity(ResourceEntity, "1", ResourceTyped, FhirRequestUri);
      this.DbAddEntity<Res_Claim>(ResourceEntity);
      IDatabaseOperationOutcome DatabaseOperationOutcome = new DatabaseOperationOutcome();
      DatabaseOperationOutcome.SingleResourceRead = true;     
      DatabaseOperationOutcome.ResourceMatchingSearch = IndexSettingSupport.SetDtoResource(ResourceEntity);
      DatabaseOperationOutcome.ResourcesMatchingSearchCount = 1;
      return DatabaseOperationOutcome;
    }

    public IDatabaseOperationOutcome UpdateResource(string ResourceVersion, Resource Resource, IDtoFhirRequestUri FhirRequestUri)
    {
      var ResourceTyped = Resource as Claim;
      var ResourceEntity = LoadCurrentResourceEntity(Resource.Id);
      var ResourceHistoryEntity = new Res_Claim_History();  
      IndexSettingSupport.SetHistoryResourceEntity(ResourceEntity, ResourceHistoryEntity);
      ResourceEntity.Res_Claim_History_List.Add(ResourceHistoryEntity); 
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
      var ResourceHistoryEntity = new Res_Claim_History();
      IndexSettingSupport.SetHistoryResourceEntity(ResourceEntity, ResourceHistoryEntity);
      ResourceEntity.Res_Claim_History_List.Add(ResourceHistoryEntity);
      this.ResetResourceEntity(ResourceEntity);
      ResourceEntity.IsDeleted = true;
      ResourceEntity.versionId = ResourceVersion;
      this.Save();      
    }

    public IDatabaseOperationOutcome GetResourceByFhirIDAndVersionNumber(string FhirResourceId, string ResourceVersionNumber)
    {
      IDatabaseOperationOutcome DatabaseOperationOutcome = new DatabaseOperationOutcome();
      DatabaseOperationOutcome.SingleResourceRead = true;
      var ResourceHistoryEntity = DbGet<Res_Claim_History>(x => x.FhirId == FhirResourceId && x.versionId == ResourceVersionNumber);
      if (ResourceHistoryEntity != null)
      {
        DatabaseOperationOutcome.ResourceMatchingSearch = IndexSettingSupport.SetDtoResource(ResourceHistoryEntity);
      }
      else
      {
        var ResourceEntity = DbGet<Res_Claim>(x => x.FhirId == FhirResourceId && x.versionId == ResourceVersionNumber);
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
        DtoResource = DbGetALL<Res_Claim>(x => x.FhirId == FhirResourceId).Select(x => new Blaze.Common.BusinessEntities.Dto.DtoResource { FhirId = x.FhirId, IsDeleted = x.IsDeleted, IsCurrent = true, Version = x.versionId, Received = x.lastUpdated, Xml = x.XmlBlob }).SingleOrDefault();       
      }
      else
      {
        DtoResource = DbGetALL<Res_Claim>(x => x.FhirId == FhirResourceId).Select(x => new Blaze.Common.BusinessEntities.Dto.DtoResource { FhirId = x.FhirId, IsDeleted = x.IsDeleted, IsCurrent = true, Version = x.versionId, Received = x.lastUpdated }).SingleOrDefault();        
      }
      DatabaseOperationOutcome.ResourceMatchingSearch = DtoResource;
      return DatabaseOperationOutcome;
    }

    private Res_Claim LoadCurrentResourceEntity(string FhirId)
    {

      var IncludeList = new List<Expression<Func<Res_Claim, object>>>();
      IncludeList.Add(x => x.identifier_List);
      IncludeList.Add(x => x.profile_List);
      IncludeList.Add(x => x.security_List);
      IncludeList.Add(x => x.tag_List);
    
      var ResourceEntity = DbQueryEntityWithInclude<Res_Claim>(x => x.FhirId == FhirId, IncludeList);

      return ResourceEntity;
    }


    private void ResetResourceEntity(Res_Claim ResourceEntity)
    {
      ResourceEntity.created_DateTimeOffset = null;      
      ResourceEntity.facilityidentifier_Code = null;      
      ResourceEntity.facilityidentifier_System = null;      
      ResourceEntity.facilityreference_FhirId = null;      
      ResourceEntity.facilityreference_Type = null;      
      ResourceEntity.facilityreference_Url = null;      
      ResourceEntity.facilityreference_Url_Blaze_RootUrlStoreID = null;      
      ResourceEntity.organizationidentifier_Code = null;      
      ResourceEntity.organizationidentifier_System = null;      
      ResourceEntity.organizationreference_FhirId = null;      
      ResourceEntity.organizationreference_Type = null;      
      ResourceEntity.organizationreference_Url = null;      
      ResourceEntity.organizationreference_Url_Blaze_RootUrlStoreID = null;      
      ResourceEntity.patientidentifier_Code = null;      
      ResourceEntity.patientidentifier_System = null;      
      ResourceEntity.patientreference_FhirId = null;      
      ResourceEntity.patientreference_Type = null;      
      ResourceEntity.patientreference_Url = null;      
      ResourceEntity.patientreference_Url_Blaze_RootUrlStoreID = null;      
      ResourceEntity.priority_Code = null;      
      ResourceEntity.priority_System = null;      
      ResourceEntity.provideridentifier_Code = null;      
      ResourceEntity.provideridentifier_System = null;      
      ResourceEntity.providerreference_FhirId = null;      
      ResourceEntity.providerreference_Type = null;      
      ResourceEntity.providerreference_Url = null;      
      ResourceEntity.providerreference_Url_Blaze_RootUrlStoreID = null;      
      ResourceEntity.targetidentifier_Code = null;      
      ResourceEntity.targetidentifier_System = null;      
      ResourceEntity.targetreference_FhirId = null;      
      ResourceEntity.targetreference_Type = null;      
      ResourceEntity.targetreference_Url = null;      
      ResourceEntity.targetreference_Url_Blaze_RootUrlStoreID = null;      
      ResourceEntity.use_Code = null;      
      ResourceEntity.use_System = null;      
      ResourceEntity.XmlBlob = null;      
 
      
      _Context.Res_Claim_Index_identifier.RemoveRange(ResourceEntity.identifier_List);            
      _Context.Res_Claim_Index_profile.RemoveRange(ResourceEntity.profile_List);            
      _Context.Res_Claim_Index_security.RemoveRange(ResourceEntity.security_List);            
      _Context.Res_Claim_Index_tag.RemoveRange(ResourceEntity.tag_List);            
 
    }

    private void PopulateResourceEntity(Res_Claim ResourseEntity, string ResourceVersion, Claim ResourceTyped, IDtoFhirRequestUri FhirRequestUri)
    {
       IndexSettingSupport.SetResourceBaseAddOrUpdate(ResourceTyped, ResourseEntity, ResourceVersion, false);

          if (ResourceTyped.Created != null)
      {
        if (ResourceTyped.CreatedElement is Hl7.Fhir.Model.FhirDateTime)
        {
          var Index = new DateIndex();
          Index = IndexSettingSupport.SetIndex(Index, ResourceTyped.CreatedElement) as DateIndex;
          if (Index != null)
          {
            ResourseEntity.created_DateTimeOffset = Index.DateTimeOffset;
          }
        }
      }

      if (ResourceTyped.Facility != null)
      {
        if (ResourceTyped.Facility is Hl7.Fhir.Model.Identifier)
        {
          var Index = new TokenIndex();
          Index = IndexSettingSupport.SetIndex(Index, ResourceTyped.Facility) as TokenIndex;
          if (Index != null)
          {
            ResourseEntity.facilityidentifier_Code = Index.Code;
            ResourseEntity.facilityidentifier_System = Index.System;
          }
        }
      }

      if (ResourceTyped.Facility != null)
      {
        if (ResourceTyped.Facility is Hl7.Fhir.Model.ResourceReference)
        {
          var Index = new ReferenceIndex();
          Index = IndexSettingSupport.SetIndex(Index, ResourceTyped.Facility, FhirRequestUri, this) as ReferenceIndex;
          if (Index != null)
          {
            ResourseEntity.facilityreference_Type = Index.Type;
            ResourseEntity.facilityreference_FhirId = Index.FhirId;
            if (Index.Url != null)
            {
              ResourseEntity.facilityreference_Url = Index.Url;
            }
            else
            {
              ResourseEntity.facilityreference_Url_Blaze_RootUrlStoreID = Index.Url_Blaze_RootUrlStoreID;
            }
          }
        }
      }

      if (ResourceTyped.Organization != null)
      {
        if (ResourceTyped.Organization is Hl7.Fhir.Model.Identifier)
        {
          var Index = new TokenIndex();
          Index = IndexSettingSupport.SetIndex(Index, ResourceTyped.Organization) as TokenIndex;
          if (Index != null)
          {
            ResourseEntity.organizationidentifier_Code = Index.Code;
            ResourseEntity.organizationidentifier_System = Index.System;
          }
        }
      }

      if (ResourceTyped.Organization != null)
      {
        if (ResourceTyped.Organization is Hl7.Fhir.Model.ResourceReference)
        {
          var Index = new ReferenceIndex();
          Index = IndexSettingSupport.SetIndex(Index, ResourceTyped.Organization, FhirRequestUri, this) as ReferenceIndex;
          if (Index != null)
          {
            ResourseEntity.organizationreference_Type = Index.Type;
            ResourseEntity.organizationreference_FhirId = Index.FhirId;
            if (Index.Url != null)
            {
              ResourseEntity.organizationreference_Url = Index.Url;
            }
            else
            {
              ResourseEntity.organizationreference_Url_Blaze_RootUrlStoreID = Index.Url_Blaze_RootUrlStoreID;
            }
          }
        }
      }

      if (ResourceTyped.Patient != null)
      {
        if (ResourceTyped.Patient is Hl7.Fhir.Model.Identifier)
        {
          var Index = new TokenIndex();
          Index = IndexSettingSupport.SetIndex(Index, ResourceTyped.Patient) as TokenIndex;
          if (Index != null)
          {
            ResourseEntity.patientidentifier_Code = Index.Code;
            ResourseEntity.patientidentifier_System = Index.System;
          }
        }
      }

      if (ResourceTyped.Patient != null)
      {
        if (ResourceTyped.Patient is Hl7.Fhir.Model.ResourceReference)
        {
          var Index = new ReferenceIndex();
          Index = IndexSettingSupport.SetIndex(Index, ResourceTyped.Patient, FhirRequestUri, this) as ReferenceIndex;
          if (Index != null)
          {
            ResourseEntity.patientreference_Type = Index.Type;
            ResourseEntity.patientreference_FhirId = Index.FhirId;
            if (Index.Url != null)
            {
              ResourseEntity.patientreference_Url = Index.Url;
            }
            else
            {
              ResourseEntity.patientreference_Url_Blaze_RootUrlStoreID = Index.Url_Blaze_RootUrlStoreID;
            }
          }
        }
      }

      if (ResourceTyped.Priority != null)
      {
        if (ResourceTyped.Priority is Hl7.Fhir.Model.Coding)
        {
          var Index = new TokenIndex();
          Index = IndexSettingSupport.SetIndex(Index, ResourceTyped.Priority) as TokenIndex;
          if (Index != null)
          {
            ResourseEntity.priority_Code = Index.Code;
            ResourseEntity.priority_System = Index.System;
          }
        }
      }

      if (ResourceTyped.Provider != null)
      {
        if (ResourceTyped.Provider is Hl7.Fhir.Model.Identifier)
        {
          var Index = new TokenIndex();
          Index = IndexSettingSupport.SetIndex(Index, ResourceTyped.Provider) as TokenIndex;
          if (Index != null)
          {
            ResourseEntity.provideridentifier_Code = Index.Code;
            ResourseEntity.provideridentifier_System = Index.System;
          }
        }
      }

      if (ResourceTyped.Provider != null)
      {
        if (ResourceTyped.Provider is Hl7.Fhir.Model.ResourceReference)
        {
          var Index = new ReferenceIndex();
          Index = IndexSettingSupport.SetIndex(Index, ResourceTyped.Provider, FhirRequestUri, this) as ReferenceIndex;
          if (Index != null)
          {
            ResourseEntity.providerreference_Type = Index.Type;
            ResourseEntity.providerreference_FhirId = Index.FhirId;
            if (Index.Url != null)
            {
              ResourseEntity.providerreference_Url = Index.Url;
            }
            else
            {
              ResourseEntity.providerreference_Url_Blaze_RootUrlStoreID = Index.Url_Blaze_RootUrlStoreID;
            }
          }
        }
      }

      if (ResourceTyped.Target != null)
      {
        if (ResourceTyped.Target is Hl7.Fhir.Model.Identifier)
        {
          var Index = new TokenIndex();
          Index = IndexSettingSupport.SetIndex(Index, ResourceTyped.Target) as TokenIndex;
          if (Index != null)
          {
            ResourseEntity.targetidentifier_Code = Index.Code;
            ResourseEntity.targetidentifier_System = Index.System;
          }
        }
      }

      if (ResourceTyped.Target != null)
      {
        if (ResourceTyped.Target is Hl7.Fhir.Model.ResourceReference)
        {
          var Index = new ReferenceIndex();
          Index = IndexSettingSupport.SetIndex(Index, ResourceTyped.Target, FhirRequestUri, this) as ReferenceIndex;
          if (Index != null)
          {
            ResourseEntity.targetreference_Type = Index.Type;
            ResourseEntity.targetreference_FhirId = Index.FhirId;
            if (Index.Url != null)
            {
              ResourseEntity.targetreference_Url = Index.Url;
            }
            else
            {
              ResourseEntity.targetreference_Url_Blaze_RootUrlStoreID = Index.Url_Blaze_RootUrlStoreID;
            }
          }
        }
      }

      if (ResourceTyped.Use_ != null)
      {
        if (ResourceTyped.Use_Element is Hl7.Fhir.Model.Code<Hl7.Fhir.Model.Claim.Use>)
        {
          var Index = new TokenIndex();
          Index = IndexSettingSupport.SetIndex(Index, ResourceTyped.Use_Element) as TokenIndex;
          if (Index != null)
          {
            ResourseEntity.use_Code = Index.Code;
            ResourseEntity.use_System = Index.System;
          }
        }
      }

      if (ResourceTyped.Identifier != null)
      {
        foreach (var item3 in ResourceTyped.Identifier)
        {
          if (item3 is Hl7.Fhir.Model.Identifier)
          {
            var Index = new Res_Claim_Index_identifier();
            Index = IndexSettingSupport.SetIndex(Index, item3) as Res_Claim_Index_identifier;
            ResourseEntity.identifier_List.Add(Index);
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
              var Index = new Res_Claim_Index_profile();
              Index = IndexSettingSupport.SetIndex(Index, item4) as Res_Claim_Index_profile;
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
              var Index = new Res_Claim_Index_security();
              Index = IndexSettingSupport.SetIndex(Index, item4) as Res_Claim_Index_security;
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
              var Index = new Res_Claim_Index_tag();
              Index = IndexSettingSupport.SetIndex(Index, item4) as Res_Claim_Index_tag;
              ResourseEntity.tag_List.Add(Index);
            }
          }
        }
      }


      

    }


  }
} 

