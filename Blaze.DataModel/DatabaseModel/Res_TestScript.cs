﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.DataModel.DatabaseModel.Base;

//This source file has been auto generated.

namespace Blaze.DataModel.DatabaseModel
{

  public class Res_TestScript : ResourceIndexBase
  {
    public int Res_TestScriptID {get; set;}
    public string description_String {get; set;}
    public string identifier_Code {get; set;}
    public string identifier_System {get; set;}
    public string name_String {get; set;}
    public string url_Uri {get; set;}
    public ICollection<Res_TestScript_History> Res_TestScript_History_List { get; set; }
    public ICollection<Res_TestScript_Index_testscript_capability> testscript_capability_List { get; set; }
    public ICollection<Res_TestScript_Index_testscript_setup_capability> testscript_setup_capability_List { get; set; }
    public ICollection<Res_TestScript_Index_testscript_test_capability> testscript_test_capability_List { get; set; }
    public ICollection<Res_TestScript_Index_profile> profile_List { get; set; }
    public ICollection<Res_TestScript_Index_security> security_List { get; set; }
    public ICollection<Res_TestScript_Index_tag> tag_List { get; set; }
   
    public Res_TestScript()
    {
      this.testscript_capability_List = new HashSet<Res_TestScript_Index_testscript_capability>();
      this.testscript_setup_capability_List = new HashSet<Res_TestScript_Index_testscript_setup_capability>();
      this.testscript_test_capability_List = new HashSet<Res_TestScript_Index_testscript_test_capability>();
      this.profile_List = new HashSet<Res_TestScript_Index_profile>();
      this.security_List = new HashSet<Res_TestScript_Index_security>();
      this.tag_List = new HashSet<Res_TestScript_Index_tag>();
      this.Res_TestScript_History_List = new HashSet<Res_TestScript_History>();
    }
  }
}
