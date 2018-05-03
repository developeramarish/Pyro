﻿using Pyro.Common.CompositionRoot;
using Pyro.Common.Interfaces.Service;

namespace Pyro.Common.CompositionRoot.Concrete
{
  public class FhirBaseOperationServiceFactory : IFhirBaseOperationServiceFactory
  {
    private readonly SimpleInjector.Container Container;

    public FhirBaseOperationServiceFactory(SimpleInjector.Container Container)
    {
      this.Container = Container;
    }

    public IFhirBaseOperationService CreateFhirBaseOperationService()
    {
      return Container.GetInstance<IFhirBaseOperationService>();
    }
  }
}