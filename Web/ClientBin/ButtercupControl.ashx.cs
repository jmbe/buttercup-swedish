using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Buttercup.Web.ClientBin
{
  public class ButtercupControl : IHttpHandler
  {

    public void ProcessRequest(HttpContext context)
    {
      // prevent the Silverlight control from being cached during development
      context.Response.Expires = -1;
      context.Response.BufferOutput = false;

      context.Response.ContentType = "application/x-silverlight-app";
      context.Response.WriteFile(context.Server.MapPath("ButtercupControl.xap"));
    }

    public bool IsReusable
    {
      get { return false; }
    }
  }
}