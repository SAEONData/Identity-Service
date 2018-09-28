using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using SAEON.Identity.Service.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.Services
{
    public class IdentityProfileService : IProfileService
    {

        private readonly IUserClaimsPrincipalFactory<SAEONUser> _claimsFactory;
        private readonly UserManager<SAEONUser> _userManager;

        public IdentityProfileService(IUserClaimsPrincipalFactory<SAEONUser> claimsFactory, UserManager<SAEONUser> userManager)
        {
            _claimsFactory = claimsFactory;
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            if (user == null)
            {
                throw new ArgumentException("");
            }

            var principal = await _claimsFactory.CreateAsync(user);
            var claims = principal.Claims.ToList();

            //Add custom claims
            claims.Add(new System.Security.Claims.Claim("UserId", user.Id.ToString()));
            claims.Add(new System.Security.Claims.Claim("FirstName", user.FirstName));
            claims.Add(new System.Security.Claims.Claim("Surname", user.Surname));

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}
