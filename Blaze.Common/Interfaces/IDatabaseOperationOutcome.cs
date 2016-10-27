﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Common.BusinessEntities.Dto;
using System.Net;

namespace Blaze.Common.Interfaces
{
  public interface IDatabaseOperationOutcome
  {    
    //int NumberOfRecordsPerPage { get; set; }
    int PageRequested { get; set; } 
    int PagesTotal { get; set; }   
    int ReturnedResourceCount { get; set; }
    List<DtoResource> ReturnedResourceList { get; set; }    
    bool SingleResourceRead { get; set; }
    DtoResource ReturnedResource { get; set; }
  }
}
