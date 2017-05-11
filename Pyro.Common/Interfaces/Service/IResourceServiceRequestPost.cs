﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Pyro.Common.Enum;
using Pyro.Common.Interfaces.UriSupport;
using Pyro.Common.Interfaces.Dto;
using Pyro.Common.Interfaces.Dto.Headers;

namespace Pyro.Common.Interfaces.Service
{
  public interface IResourceServiceRequestPost
  {
    Resource Resource { get; set; }    
    /// <summary>
    /// The id must be a valid GUID and is later validated as one.
    /// </summary>
    string ForceId { get; set; }
    IDtoRequestUri RequestUri { get; set; }
    IDtoSearchParameterGeneric SearchParameterGeneric { get; set; }
    IDtoRequestHeaders RequestHeaders { get; set; }
  }
}
