﻿using Pyro.Common.CompositionRoot;
using Pyro.Common.Interfaces.Service;

namespace Pyro.Backburner.CompositionRoot2
{
  public class FhirResourceOperationServiceFactory : IFhirResourceOperationServiceFactory
  {
    private readonly SimpleInjector.Container Container;

    public FhirResourceOperationServiceFactory(SimpleInjector.Container Container)
    {
      this.Container = Container;
    }

    public IFhirResourceOperationService CreateFhirResourceOperationService()
    {
      return Container.GetInstance<IFhirResourceOperationService>();
    }
  }
}