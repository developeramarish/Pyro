﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pyro.Common.ServiceRoot;
using Pyro.Common.Interfaces.Service;

namespace Pyro.Common.Tools.UriSupport
{
  public class PyroRequestUri : IPyroRequestUri
  {
    private readonly IPrimaryServiceRootCache IPrimaryServiceRootCache;

    public PyroRequestUri(IPrimaryServiceRootCache IPrimaryServiceRootCache, IPyroFhirUri IPyroFhirUri)
    {
      this.IPrimaryServiceRootCache = IPrimaryServiceRootCache;
      this.FhirRequestUri = IPyroFhirUri;
    }

    public IDtoRootUrlStore PrimaryRootUrlStore
    {
      get
      {
        return IPrimaryServiceRootCache.GetPrimaryRootUrlStoreFromDatabase();
      }
      set
      {
        throw new NotImplementedException();
      }
    }
    public IPyroFhirUri FhirRequestUri { get; set; }
  }
}
