using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;

namespace BillRob.Web
{
	public class PasswordProtectHttpModule : IHttpModule
	{
		void IHttpModule.Dispose()
		{
			
		}

		void IHttpModule.Init(HttpApplication context)
		{
			context.BeginRequest += new EventHandler(context_BeginRequest);
		}

		void context_BeginRequest(object sender, EventArgs e)
		{
			var application = (HttpApplication)sender;

			var context = new HttpContextWrapper(application.Context);
			DoPasswordProtect(context);
		}

		void DoPasswordProtect(HttpContextWrapper context)
		{
			var request = context.Request;
			var response = context.Response;

			//normally this would be some type of abstraction.
			var username = ConfigurationManager.AppSettings["br-username"];
			var password = ConfigurationManager.AppSettings["br-password"];

			if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
			{
				var data = String.Format("{0}:{1}", username, password);
				var correctHeader = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(data));

				string securityString = request.Headers["Authorization"];
				if (securityString == null)
					goto forceRedirect;

				if (securityString != correctHeader)
					goto forceRedirect;

				goto end;

			forceRedirect:
				var host = request.Url.Host.ToLower();
				response.AddHeader("WWW-Authenticate", String.Format(@"Basic realm=""{0}""", host));
				response.StatusCode = 401;
				response.End();
			end:
				;
			}
		}
	}
}
