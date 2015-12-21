﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Model
{
  public class Prefix
  {
    public int Id { get; set; }
    public string Value { get; set; }
    public int HumanNameId { get; set; }      
  }

  public static partial class DbInfo
  {
    public static readonly string TableNameIs = "Prefix";
    public static readonly string Id = "Id";
    public static readonly string Value = "Value";
    public static readonly string HumanNameId = "HumanNameId";
  }
}
