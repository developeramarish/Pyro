﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.DataModel.DatabaseModel.Base;

//This source file has been auto generated.

namespace Blaze.DataModel.DatabaseModel
{

  public class Res_NutritionOrder_Index_supplement : TokenIndex
  {
    public int Res_NutritionOrder_Index_supplementID {get; set;}
    public virtual Res_NutritionOrder Res_NutritionOrder { get; set; }
   
    public Res_NutritionOrder_Index_supplement()
    {
    }
  }
}
