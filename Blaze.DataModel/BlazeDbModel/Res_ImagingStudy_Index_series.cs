﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//This source file has been auto generated.

namespace Blaze.DataModel.BlazeDbModel
{

  public class Res_ImagingStudy_Index_series
  {
    public int Id {get; set;}
    public string Uri {get; set;}     
    
    //Keyed
    public virtual Res_ImagingStudy Res_ImagingStudy { get; set; }                     
  }
}