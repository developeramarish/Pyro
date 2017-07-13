﻿using System;
using Pyro.DataLayer.DbModel.EntityBase;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Pyro.Common.BusinessEntities.Dto;
using System.Collections.Generic;
using System.Linq;
using Pyro.Common.Interfaces.UriSupport;
using Pyro.Common.Tools;
using Pyro.Common.BusinessEntities.UriSupport;
using Pyro.Common.Interfaces.Repositories;
using Pyro.DataLayer.Repository.Interfaces;
using Pyro.Common.ServiceRoot;
using Pyro.Common.CompositionRoot;

namespace Pyro.DataLayer.IndexSetter
{
  public class ReferenceSetter : IReferenceSetter
  {
    private readonly ICommonRepository ICommonRepository;
    private readonly IPrimaryServiceRootCache IPrimaryServiceRootCache;
    private readonly ICommonFactory ICommonFactory;

    public ReferenceSetter(ICommonRepository ICommonRepository, IPrimaryServiceRootCache IPrimaryServiceRootCache, ICommonFactory ICommonFactory)
    {
      this.ICommonRepository = ICommonRepository;
      this.IPrimaryServiceRootCache = IPrimaryServiceRootCache;
      this.ICommonFactory = ICommonFactory;
    }

    public IList<ResourceIndexType> Set<ResourceCurrentType, ResourceIndexType>(IElementNavigator oElement, DtoServiceSearchParameterLight SearchParameter)
      where ResourceCurrentType : ResourceCurrentBase<ResourceCurrentType, ResourceIndexType>, new()
      where ResourceIndexType : ResourceIndexBase<ResourceCurrentType, ResourceIndexType>, new()
    {
      var ResourceIndexList = new List<ResourceIndexType>();
      var ServiceSearchParameterId = SearchParameter.Id;

      if (oElement is Hl7.Fhir.FhirPath.PocoNavigator Poco && Poco.FhirValue != null)
      {
        if (Poco.FhirValue is FhirUri FhirUri)
        {
          SetFhirUri<ResourceCurrentType, ResourceIndexType>(FhirUri, ResourceIndexList);
        }
        else if (Poco.FhirValue is ResourceReference ResourceReference)
        {
          SetResourcereference<ResourceCurrentType, ResourceIndexType>(ResourceReference, ResourceIndexList);
        }
        else if (Poco.FhirValue is Resource Resource)
        {
          SetResource<ResourceCurrentType, ResourceIndexType>(Resource, ResourceIndexList);
        }
        else
        {
          throw new FormatException($"Unknown FhirType: '{oElement.Type}' for SearchParameterType: '{SearchParameter.Type}'");
        }
        ResourceIndexList.ForEach(x => x.ServiceSearchParameterId = ServiceSearchParameterId);
        return ResourceIndexList;
      }
      else
      {
        throw new FormatException($"Unknown FhirType: '{oElement.Type}' for SearchParameterType: '{SearchParameter.Type}'");
      }
    }

    private static void SetResource<ResourceCurrentType, ResourceIndexType>(Resource resource, List<ResourceIndexType> ResourceIndexList)
      where ResourceCurrentType : ResourceCurrentBase<ResourceCurrentType, ResourceIndexType>, new()
      where ResourceIndexType : ResourceIndexBase<ResourceCurrentType, ResourceIndexType>, new()
    {
      if (resource.ResourceType == ResourceType.Composition || resource.ResourceType == ResourceType.MessageHeader)
      {
        //ToDo: What do we do with this Resource as a ResourceReferance??
        //FHIR Spec says:
        //The first resource in the bundle, if the bundle type is "document" - this is a composition, and this parameter provides access to searches its contents
        //and
        //The first resource in the bundle, if the bundle type is "message" - this is a message header, and this parameter provides access to search its contents
      }
    }

    private void SetResourcereference<ResourceCurrentType, ResourceIndexType>(ResourceReference ResourceReference, List<ResourceIndexType> ResourceIndexList)
      where ResourceCurrentType : ResourceCurrentBase<ResourceCurrentType, ResourceIndexType>, new()
      where ResourceIndexType : ResourceIndexBase<ResourceCurrentType, ResourceIndexType>, new()
    {
      //Check the Uri is actual a Fhir resource reference 
      if (Hl7.Fhir.Rest.HttpUtil.IsRestResourceIdentity(ResourceReference.Reference))
      {
        if (!ResourceReference.IsContainedReference && ResourceReference.Url != null)
        {
          SetReferance<ResourceCurrentType, ResourceIndexType>(ResourceReference.Url.OriginalString, ResourceIndexList);
        }
      }
    }

    private void SetFhirUri<ResourceCurrentType, ResourceIndexType>(FhirUri FhirUri, List<ResourceIndexType> ResourceIndexList)
      where ResourceCurrentType : ResourceCurrentBase<ResourceCurrentType, ResourceIndexType>, new()
      where ResourceIndexType : ResourceIndexBase<ResourceCurrentType, ResourceIndexType>, new()
    {
      if (!string.IsNullOrWhiteSpace(FhirUri.Value))
      {
        SetReferance<ResourceCurrentType, ResourceIndexType>(FhirUri.Value, ResourceIndexList);
      }
    }

    private void SetReferance<ResourceCurrentType, ResourceIndexType>(string UriString, List<ResourceIndexType> ResourceIndexList)
      where ResourceCurrentType : ResourceCurrentBase<ResourceCurrentType, ResourceIndexType>, new()
      where ResourceIndexType : ResourceIndexBase<ResourceCurrentType, ResourceIndexType>, new()
    {
      //Check the Uri is actual a Fhir resource reference         
      if (Hl7.Fhir.Rest.HttpUtil.IsRestResourceIdentity(UriString))
      {
        IFhirRequestUri ReferanceUri = ICommonFactory.CreateFhirRequestUri();
        if (Uri.IsWellFormedUriString(UriString, UriKind.Relative))
        {
          if (ReferanceUri.Parse(UriString.Trim()))
          {
            var ResourceIndex = new ResourceIndexType();
            SetResourceIndentityElements<ResourceCurrentType, ResourceIndexType>(ResourceIndex, ReferanceUri);
            ResourceIndex.ReferenceServiceBaseUrlId = IPrimaryServiceRootCache.GetPrimaryRootUrlFromDatabase().Id;
            ResourceIndexList.Add(ResourceIndex);
          }
        }
        else if (Uri.IsWellFormedUriString(UriString, UriKind.Absolute))
        {
          if (ReferanceUri.Parse(UriString.Trim()))
          {
            var ResourceIndex = new ResourceIndexType();
            SetResourceIndentityElements<ResourceCurrentType, ResourceIndexType>(ResourceIndex, ReferanceUri);
            if (ReferanceUri.IsRelativeToServer)
            {
              ResourceIndex.ReferenceServiceBaseUrlId = IPrimaryServiceRootCache.GetPrimaryRootUrlFromDatabase().Id;
            }
            else
            {
              ResourceIndex.ReferenceUrl = ICommonRepository.GetAndOrAddService_RootUrlStore(ReferanceUri.UriPrimaryServiceRoot.OriginalString);
            }
            ResourceIndexList.Add(ResourceIndex);
          }
        }
      }
    }

    private void SetResourceIndentityElements<ResourceCurrentType, ResourceIndexType>(ResourceIndexType ResourceIndex, IFhirRequestUri FhirRequestUri)
      where ResourceCurrentType : ResourceCurrentBase<ResourceCurrentType, ResourceIndexType>, new()
      where ResourceIndexType : ResourceIndexBase<ResourceCurrentType, ResourceIndexType>, new()
    {
      ResourceIndex.ReferenceResourceType = FhirRequestUri.ResourseName;
      ResourceIndex.ReferenceVersionId = FhirRequestUri.VersionId;
      ResourceIndex.ReferenceFhirId = FhirRequestUri.ResourceId;
    }
  }
}