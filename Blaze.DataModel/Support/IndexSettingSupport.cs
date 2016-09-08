﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Blaze.Common.BusinessEntities.Dto;
using Blaze.Common.Interfaces.UriSupport;
using Blaze.DataModel.DatabaseModel;
using Blaze.DataModel.DatabaseModel.Base;
using Blaze.DataModel.Repository;
using Blaze.DataModel.IndexSetter;


namespace Blaze.DataModel.Support
{
  public static class IndexSettingSupport
  {

    private static void SetResourceBase(Resource Resource, ResourceIndexBase ResourceIndexBase, string FhirResourceId, string Version, bool IsDeleted)
    {
      if (!IsDeleted)
      {
        ResourceIndexBase.FhirId = Resource.Id;
        ResourceIndexBase.IsDeleted = IsDeleted;
        ResourceIndexBase.XmlBlob = Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToXml(Resource);
        ResourceIndexBase.lastUpdated = (DateTimeOffset)Resource.Meta.LastUpdated;
        ResourceIndexBase.versionId = Version;
      }
      else
      {
        ResourceIndexBase.FhirId = FhirResourceId;
        ResourceIndexBase.IsDeleted = IsDeleted;
        ResourceIndexBase.XmlBlob = "";
        ResourceIndexBase.lastUpdated = DateTimeOffset.Now;
        ResourceIndexBase.versionId = Version;
      }
    }

    public static DtoResource SetDtoResource(DatabaseModel.Base.ResourceIndexBase ResourceIndexBase)
    {
      var DtoResource = new DtoResource();
      DtoResource.FhirId = ResourceIndexBase.FhirId;
      DtoResource.IsCurrent = true;
      DtoResource.IsDeleted = ResourceIndexBase.IsDeleted;
      DtoResource.Received = ResourceIndexBase.lastUpdated;
      DtoResource.Version = ResourceIndexBase.versionId;
      DtoResource.Xml = ResourceIndexBase.XmlBlob;
      return DtoResource;
    }

    public static void SetResourceBaseAsDelete(ResourceIndexBase ResourceIndexBase, string FhirResourceId, string Version)
    {
      SetResourceBase(null, ResourceIndexBase, FhirResourceId, Version, true);
    }

    public static void SetResourceBaseAddOrUpdate(Resource Resource, ResourceIndexBase ResourceIndexBase, string Version, bool IsDeleted)
    {
      SetResourceBase(Resource, ResourceIndexBase, null, Version, false);
    }

    public static void SetHistoryResourceEntity(DatabaseModel.Base.ResourceIndexBase ResourceEntity, DatabaseModel.Base.ResourceIndexBase HistoryResourceEntity)
    {
      HistoryResourceEntity.FhirId = ResourceEntity.FhirId;
      HistoryResourceEntity.IsDeleted = ResourceEntity.IsDeleted;
      HistoryResourceEntity.XmlBlob = ResourceEntity.XmlBlob;
      HistoryResourceEntity.lastUpdated = ResourceEntity.lastUpdated;
      HistoryResourceEntity.versionId = ResourceEntity.versionId;
    }

  }
}