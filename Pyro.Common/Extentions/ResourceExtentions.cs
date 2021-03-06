﻿using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;

namespace Pyro.Common.Extentions
{
  static class ResourceExtentions
  {
    public static List<ResourceReference> AllReferences(this IEnumerable<Bundle.EntryComponent> EntryComponentList)
    {
      //Cache ClassMappings's PropertyList as likley to have same resource again in the bundle entrie list
      var ClassPropertyMappingListCache = new Dictionary<int, IEnumerable<Hl7.Fhir.Introspection.PropertyMapping>>();

      var ReferenceResultList = new List<ResourceReference>();
      int Count = 0;
      foreach (var Entry in EntryComponentList)
      {
        if (Entry.Resource != null)
        {
          ReferenceResultList.AddRange(Entry.Resource.AllBaseReferences(ClassPropertyMappingListCache));
        }
        Count++;
      }
      return ReferenceResultList;
    }

    public static List<ResourceReference> AllReferences(this Resource Resource)
    {
      var ClassPropertyMappingListCache = new Dictionary<int, IEnumerable<Hl7.Fhir.Introspection.PropertyMapping>>();
      return Resource.AllBaseReferences(ClassPropertyMappingListCache);
    }

    private static List<ResourceReference> AllBaseReferences(this Base FhirBase, Dictionary<int, IEnumerable<Hl7.Fhir.Introspection.PropertyMapping>> ClassPropertyMappingListCache)
    {
      var ReferenceResultList = new List<ResourceReference>();
      if (FhirBase == null)
        return ReferenceResultList;

      //If DomainResource or Extension then drill through all extentions and collect all referances
      if (FhirBase is DomainResource DomainResource)
      {
        ReferenceResultList.AddRange(DomainResource.Extension.AllExtensionListReferences());
      }
      else if (FhirBase is Extension Extension)
      {
        ReferenceResultList.AddRange(Extension.AllExtensionReferences());
      }

      //Cache ClassMappings's PropertyList as likley to have same resource again in the bundle entries
      IEnumerable<Hl7.Fhir.Introspection.PropertyMapping> PropertyMappingList = ClassPropertyMappingListCache.SingleOrDefault(x => x.Key == FhirBase.TypeName.GetHashCode()).Value;
      if (PropertyMappingList == null)
      {
        var ClassMapping = Hl7.Fhir.Introspection.ClassMapping.Create(FhirBase.GetType());
        PropertyMappingList = ClassMapping.PropertyMappings.Where(t => t.ElementType == typeof(ResourceReference) || t.ElementType.BaseType == typeof(BackboneElement));
        ClassPropertyMappingListCache.Add(FhirBase.TypeName.GetHashCode(), PropertyMappingList);
      }
      int Count = 0;
      foreach (var PropertyItem in PropertyMappingList)
      {

        if (PropertyItem.ElementType.BaseType == typeof(BackboneElement))
        {
          if (PropertyItem.IsCollection)
          {
            try
            {
              var PropertyCollection = PropertyItem.GetValue(FhirBase) as System.Collections.IEnumerable;
              foreach (var CollectionItem in PropertyCollection)
              {
                var BackboneElement = CollectionItem as BackboneElement;
                ReferenceResultList.AddRange(BackboneElement.AllBaseReferences(ClassPropertyMappingListCache));
              }
            }
            catch (System.Exception)
            {
              //Why Does 'PropertyItem.GetValue(FhirBase)' throw an exception?
              //for no we are ignoring the exception as not sure what else to do, could be some 
              //ResourceReferences in transation bunudles are not updated.
            }
          }
          else
          {
            try
            {
              var BackboneElement = PropertyItem.GetValue(FhirBase) as BackboneElement;
              ReferenceResultList.AddRange(BackboneElement.AllBaseReferences(ClassPropertyMappingListCache));
            }
            catch (System.Exception)
            {
              //Why Does 'PropertyItem.GetValue(FhirBase)' throw an exception?
              //for no we are ignoring the exception as not sure what else to do, could be some 
              //ResourceReferences in transation bunudles are not updated.
            }
          }
        }
        else
        {
          try
          {
            if (PropertyItem.GetValue(FhirBase) is ResourceReference rr)
              ReferenceResultList.Add(rr);
          }
          catch (System.Exception)
          {
            //Why Does 'PropertyItem.GetValue(FhirBase)' throw an exception?
            //for no we are ignoring the exception as not sure what else to do, could be some 
            //ResourceReferences in transation bunudles are not updated.
          }
        }
        Count++;
      }
      return ReferenceResultList;
    }

    private static IEnumerable<ResourceReference> AllExtensionReferences(this Extension Extension)
    {
      var ReferenceResultList = new List<ResourceReference>();
      if (Extension.Value != null)
      {
        if (Extension.Value.TypeName == ModelInfo.FhirTypeToFhirTypeName(FHIRAllTypes.Reference))
        {
          if (Extension.Value is ResourceReference Ref)
            ReferenceResultList.Add(Ref);
        }
      }
      foreach (Extension Ext in Extension.Extension)
      {
        ReferenceResultList.AddRange(AllExtensionReferences(Ext));
      }
      return ReferenceResultList;
    }

    private static IEnumerable<ResourceReference> AllExtensionListReferences(this IList<Extension> ExtensionList)
    {
      var ReferenceResultList = new List<ResourceReference>();
      foreach (Extension Ext in ExtensionList)
      {
        ReferenceResultList.AddRange(Ext.AllExtensionReferences());
      }
      return ReferenceResultList;
    }

  }
}
