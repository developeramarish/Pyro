﻿using Pyro.Common.CompositionRoot;
using Pyro.Common.Tools.Headers;

namespace Pyro.Backburner.CompositionRoot2
{
  public class RequestHeaderFactory : IRequestHeaderFactory
  {
    private readonly SimpleInjector.Container Container;

    public RequestHeaderFactory(SimpleInjector.Container Container)
    {
      this.Container = Container;
    }

    public IRequestHeader CreateRequestHeader()
    {
      return Container.GetInstance<IRequestHeader>();
    }
  }
}