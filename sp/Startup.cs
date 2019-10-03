using IdentityServer4;
using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rsk.AspNetCore.Authentication.Saml2p;
using System;
using System.Security.Cryptography.X509Certificates;

namespace sp {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();
            services.AddControllersWithViews();

            services.Configure<IISOptions>(options => {
                options.AutomaticAuthentication = false;
                options.AuthenticationDisplayName = "Windows";
            });

            var builder = services.AddIdentityServer(options => {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddTestUsers(TestUsers.Users)
                .AddSigningCredential(new X509Certificate2("testclient.pfx", "test"));

            builder.AddInMemoryIdentityResources(Config.GetIdentityResources());
            builder.AddInMemoryApiResources(Config.GetApis());
            builder.AddInMemoryClients(Config.GetClients())
                .AddSamlPlugin(options => {
                    options.Licensee = "";
                    options.LicenseKey = "";
                    options.WantAuthenticationRequestsSigned = false;
                })
                .AddInMemoryServiceProviders(Config.GetServiceProviders());

            services.AddAuthentication()
                .AddSaml2p("saml2p", options => {
                    options.Licensee = "DEMO";
                    options.LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsIlJlbmV3YWxTZW50VGltZSI6IjAwMDEtMDEtMDFUMDA6MDA6MDAiLCJhdXRoIjoiREVNTyIsImV4cCI6IjIwMTktMTEtMDFUMTI6NDY6MjMuNjc3ODM0MiswMDowMCIsImlhdCI6IjIwMTktMTAtMDJUMTE6NDY6MjMiLCJvcmciOiJERU1PIiwiYXVkIjoyfQ==.ncOdBt4F7CJ/gdk+K9uwrh3z2xZliSYVSMNrPqldiaXRHS47aGs6FgP/V6oD/JYRT98APZe7zGHQgVGViKuqzvmyJrjdaeT4FQLnNxeWX67fHqEDS1WvDNrTkMKDIIfbVZfF/6R4gRNNLHACQME+IoPW8daPOxZh6Z8CDLrClYufM/KwU1Eqneh2gIi3XiU+YF4B7brpCXgDVCLCDuKnL9TvCqJp+SvAr+LNeshzja9XPvV345D2y3Y7qGGvV8CXdMbMTLc55qdovdTgZ6xSnUrzeuSqh2jEG/URJGdfX0j/fVP10X27kdZwnFX0Oe3zvS7RMpTHOSHzyXXcnm2UUTEJxsppwRM88Ng/jrNunq9HJNhlgZLygWXclEdB5v6iMamud/i6UtHq//ad2japvelUiFZleHTX/NRoso7hT4pTcC+nKqI4MfX6tm96e/JLssW2cnBWQAA5FsB61YE6tR4EV0K8VYZCFHxRBK0C5jQrbSjTXQ7vLBPWgceZJw1DhbxKL+TwEC2/xuFrRLsGlTYdPN7+HUE5DqIXRpClCLAVEVvf5c7jo71njmV23zRboc46Uy7jI8CIpiLH4F8NX1YUN/Ws7c7Q333Mu8wxaNNLClUVODLlbUOgyAqN6EOsatZjTT+rSHSwbg8ncVpS8VyImpz3R+zrteFa5NO+SQg=";

                    options.IdentityProviderOptions = new IdpOptions {
                        EntityId = "http://localhost:5000",
                        SigningCertificate = new X509Certificate2("idsrv3test.cer"),
                        SingleSignOnEndpoint = new SamlEndpoint("http://localhost:5000/saml/sso", SamlBindingTypes.HttpRedirect),
                        SingleLogoutEndpoint = new SamlEndpoint("http://localhost:5000/saml/slo", SamlBindingTypes.HttpRedirect),
                    };

                    options.ServiceProviderOptions = new SpOptions {
                        EntityId = "http://localhost:5001/saml",
                        MetadataPath = "/saml/metadata",
                        SignAuthenticationRequests = true,
                        SigningCertificate = new X509Certificate2("testclient.pfx", "test")
                    };

                    options.NameIdClaimType = "sub";
                    options.CallbackPath = "/signin-saml";
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                });

            services.AddAuthentication()
               .AddSaml2p("samlTest", "SAMLTEST IDP", options => {
                   options.Licensee = "DEMO";
                   options.LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsImF1dGgiOiJERU1PIiwiZXhwIjoiMjAxOS0xMC0wOVQwOTowOToyMy42NzIxMzQxKzAxOjAwIiwiaWF0IjoiMjAxOS0wOS0wOVQwODowOToyMyIsIm9yZyI6IkRFTU8iLCJhdWQiOjJ9.HWgKSkEeBopGXy34MSObRKIbgF09eQK3tIoDSN6IguJ3qx/BrGaJNbGf63OI0NAFoETCkVV04SR6opFZcagOL6oK8aVzyr4yfnTUEetFTD0YZsx0e7qBh80tFB+URLY7WZuHqmhHEzqnoLBma67HPQQnEvmKIo5Np7qD3trSwp0Qw+mnrylYo57joj48WpCwCe5tkqQN8gMdU7YBTn+wZWZoPlqb2l3/9Fni9UdZfs+ytgBVCXZbcwzzhtDr2YtpJJBkIyTcJujHsLeBTcSzuYwkPwfycMEPjCk+HCeJ17XUpdwkOAxV0XpfrVttEfsDF3h1MJ4WYPXk2LyylmxY4z0mOadybbeFeMvX/FP0IDuoAQtQHtOWgrb6nNpglTiWsb1orwUf9+nPNrHxlwTwtBAl1vzKsyhGHSC6dAjYsfMaTNcQuJeaEZPHzhycBQlUMxlYpcl4CKYTcc9rITPHTo3Py3JSNt6EPPXp5xVXFE4j422Mzn0187/qgoCpCBMbPvkdeXO9tAx21PRmYNFzpZ0TMLQwFp8J+o+5GRGWzHtrv3ouh8mkWTe97hpEEPJQKsTJa7PmWz+xy/kFFFw+qqMJnRduV4OiU8giizEewHKkOK7L4GjGAVCoEaSQqVqhWVNn6nAQOlzWBrse5VtS6NxjxaeEs0HbHpWJGDeVvMU=";

                   options.IdentityProviderOptions = new IdpOptions {
                       EntityId = "https://samltest.id/saml/idp",
                       SigningCertificate = new X509Certificate2(Convert.FromBase64String("MIIDEjCCAfqgAwIBAgIVAMECQ1tjghafm5OxWDh9hwZfxthWMA0GCSqGSIb3DQEBCwUAMBYxFDASBgNVBAMMC3NhbWx0ZXN0LmlkMB4XDTE4MDgyNDIxMTQwOVoXDTM4MDgyNDIxMTQwOVowFjEUMBIGA1UEAwwLc2FtbHRlc3QuaWQwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC0Z4QX1NFKs71ufbQwoQoW7qkNAJRIANGA4iM0ThYghul3pC+FwrGv37aTxWXfA1UG9njKbbDreiDAZKngCgyjxj0uJ4lArgkr4AOEjj5zXA81uGHARfUBctvQcsZpBIxDOvUUImAl+3NqLgMGF2fktxMG7kX3GEVNc1klbN3dfYsaw5dUrw25DheL9np7G/+28GwHPvLb4aptOiONbCaVvh9UMHEA9F7c0zfF/cL5fOpdVa54wTI0u12CsFKt78h6lEGG5jUs/qX9clZncJM7EFkN3imPPy+0HC8nspXiH/MZW8o2cqWRkrw3MzBZW3Ojk5nQj40V6NUbjb7kfejzAgMBAAGjVzBVMB0GA1UdDgQWBBQT6Y9J3Tw/hOGc8PNV7JEE4k2ZNTA0BgNVHREELTArggtzYW1sdGVzdC5pZIYcaHR0cHM6Ly9zYW1sdGVzdC5pZC9zYW1sL2lkcDANBgkqhkiG9w0BAQsFAAOCAQEASk3guKfTkVhEaIVvxEPNR2w3vWt3fwmwJCccW98XXLWgNbu3YaMb2RSn7Th4p3h+mfyk2don6au7Uyzc1Jd39RNv80TG5iQoxfCgphy1FYmmdaSfO8wvDtHTTNiLArAxOYtzfYbzb5QrNNH/gQEN8RJaEf/g/1GTw9x/103dSMK0RXtl+fRs2nblD1JJKSQ3AdhxK/weP3aUPtLxVVJ9wMOQOfcy02l+hHMb6uAjsPOpOVKqi3M8XmcUZOpx4swtgGdeoSpeRyrtMvRwdcciNBp9UZome44qZAYH1iqrpmmjsfI9pJItsgWu3kXPjhSfj1AJGR1l9JGvJrHki1iHTA==")),
                       SingleSignOnEndpoint = new SamlEndpoint("https://samltest.id/idp/profile/SAML2/Redirect/SSO", SamlBindingTypes.HttpRedirect)
                   };

                   options.ServiceProviderOptions = new SpOptions {
                       EntityId = "http://localhost:5001/saml",
                       MetadataPath = "/saml/metadata",
                       SignAuthenticationRequests = true,
                       SigningCertificate = new X509Certificate2("testclient.pfx", "test")
                   };

                   options.NameIdClaimType = "sub";
                   options.CallbackPath = "/signin-saml";
                   options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
               });

        }

        public void Configure(IApplicationBuilder app) {
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer()
               .UseIdentityServerSamlPlugin();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
        }
    }
}