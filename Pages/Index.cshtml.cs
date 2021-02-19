using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace myldap.Pages {
  public class IndexModel : PageModel {
    public string Message { get; private set; } = "PageModel in C#";
    public string Person { get; private set; } = "";
    public string DirectoryEntry { get; private set; } = "";
    public string DirectoryPath { get; private set; } = "";

    public async Task OnGet () {
      Console.WriteLine ("Start page...");

      var ldap = "your-ldap";
      var user = "your-username";
      var pass = "your-passwords";
      var entry = new DirectoryEntry (ldap, user, pass);
      var directorySearcher = new DirectorySearcher (entry);

      var searchFilter = "your-criteria";
      directorySearcher.Filter = string.Format (searchFilter, user);

      var result = directorySearcher.FindOne ();
      var directoryEntry = result.GetDirectoryEntry ();

      DirectoryEntry = directoryEntry.Name;
      DirectoryPath = directoryEntry.Path;

      var User = new UserExample {
        EMAIL = result.Properties["mail"][0].ToString (),
      };

      var userClaims = new List<Claim> {
        new Claim ("EMAIL", User.EMAIL),
      };

      var principal = new ClaimsPrincipal (new ClaimsIdentity (userClaims, CookieAuthenticationDefaults.AuthenticationScheme));
      var authProperties = new AuthenticationProperties {
        IsPersistent = true,
        ExpiresUtc = DateTime.UtcNow.AddMinutes (2),
      };

      await HttpContext.SignInAsync (CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
      directorySearcher.Dispose ();

      entry.Close ();
      entry.Dispose ();

      Person += JsonConvert.SerializeObject (User);
      Message += $" Server time is { DateTime.Now }";

    }
  }
}