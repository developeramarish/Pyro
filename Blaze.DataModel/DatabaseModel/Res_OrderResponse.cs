﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.DataModel.DatabaseModel.Base;

//This source file has been auto generated.

namespace Blaze.DataModel.DatabaseModel
{

  public class Res_OrderResponse : ResourceIndexBase
  {
    public int Res_OrderResponseID {get; set;}
    public string code_Code {get; set;}
    public string code_System {get; set;}
    public DateTimeOffset? date_DateTimeOffset {get; set;}
    public string request_VersionId {get; set;}
    public string request_FhirId {get; set;}
    public string request_Type {get; set;}
    public virtual ServiceRootURL_Store request_Url { get; set; }
    public int? request_ServiceRootURL_StoreID { get; set; }
    public string who_VersionId {get; set;}
    public string who_FhirId {get; set;}
    public string who_Type {get; set;}
    public virtual ServiceRootURL_Store who_Url { get; set; }
    public int? who_ServiceRootURL_StoreID { get; set; }
    public ICollection<Res_OrderResponse_History> Res_OrderResponse_History_List { get; set; }
    public ICollection<Res_OrderResponse_Index_fulfillment> fulfillment_List { get; set; }
    public ICollection<Res_OrderResponse_Index_identifier> identifier_List { get; set; }
    public ICollection<Res_OrderResponse_Index_profile> profile_List { get; set; }
    public ICollection<Res_OrderResponse_Index_security> security_List { get; set; }
    public ICollection<Res_OrderResponse_Index_tag> tag_List { get; set; }
   
    public Res_OrderResponse()
    {
      this.fulfillment_List = new HashSet<Res_OrderResponse_Index_fulfillment>();
      this.identifier_List = new HashSet<Res_OrderResponse_Index_identifier>();
      this.profile_List = new HashSet<Res_OrderResponse_Index_profile>();
      this.security_List = new HashSet<Res_OrderResponse_Index_security>();
      this.tag_List = new HashSet<Res_OrderResponse_Index_tag>();
      this.Res_OrderResponse_History_List = new HashSet<Res_OrderResponse_History>();
    }
  }
}
