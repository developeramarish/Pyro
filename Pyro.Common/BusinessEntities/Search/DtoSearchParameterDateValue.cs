﻿using System;
using System.Collections.Generic;
using System.Linq;
using Blaze.Common.Enum;

namespace Blaze.Common.BusinessEntities.Search
{
  public class DtoSearchParameterDateValue : DtoSearchParameterValueWithPrefix
  {
    public int Value { get; set; }
  }
}