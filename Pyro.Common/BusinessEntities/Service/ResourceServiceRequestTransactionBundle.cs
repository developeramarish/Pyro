﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Pyro.Common.Interfaces.Service;
using Pyro.Common.Interfaces.UriSupport;
using Pyro.Common.Enum;
using Pyro.Common.Interfaces.Dto;
using Pyro.Common.Interfaces.Dto.Headers;

namespace Pyro.Common.BusinessEntities.Service
{
  public class ResourceServiceRequestTransactionBundle : IResourceServiceRequestTransactionBundle
  {
    public IDtoSearchParameterGeneric SearchParameterGeneric { get; set; }
    public IDtoRequestHeaders RequestHeaders { get; set; }
    public Resource Resource { get; set; }
    public IServiceNegotiator ServiceNegotiator { get; set; }
    public IDtoFhirRequestUri DtoFhirRequestUri { get; set; }

    public ResourceServiceRequestTransactionBundle(Resource Resource, IDtoFhirRequestUri DtoFhirRequestUri, IDtoSearchParameterGeneric SearchParameterGeneric, IDtoRequestHeaders RequestHeaders, IServiceNegotiator ServiceNegotiator)
    {
      if (Resource == null)
        throw new NullReferenceException("Resource can not be null.");
      if (DtoFhirRequestUri == null)
        throw new NullReferenceException("DtoFhirRequestUri can not be null.");
      if (SearchParameterGeneric == null)
        throw new NullReferenceException("SearchParameterGeneric can not be null.");
      if (RequestHeaders == null)
        throw new NullReferenceException("RequestHeaders can not be null.");
      if (ServiceNegotiator == null)
        throw new NullReferenceException("ServiceNegotiator can not be null.");

      this.Resource = Resource;
      this.DtoFhirRequestUri = DtoFhirRequestUri;
      this.SearchParameterGeneric = SearchParameterGeneric;
      this.RequestHeaders = RequestHeaders;
      this.ServiceNegotiator = ServiceNegotiator;
    }    
  }
}