using gestionaMelon.Facebook;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Owin;

namespace gestionaMelon
{
    public partial class Startup
    {
        // Para obtener más información para configurar la autenticación, visite http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Permitir que la aplicación use una cookie para almacenar información para el usuario que inicia sesión
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });
            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Quitar los comentarios de las siguientes líneas para habilitar el inicio de sesión con proveedores de inicio de sesión de terceros
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "307435572999520",
            //   appSecret: "031681d194ccede23c936808e99c6c3d");
            





            ///     INGRESAR APP ID Y APP SECRET DE FACEBOOK Y GOOGLE

            var facebookOptions = new FacebookAuthenticationOptions()
            {
                AppId = "",
                AppSecret = "",
                BackchannelHttpHandler = new FacebookBackChannelHandler(),
                UserInformationEndpoint = "https://graph.facebook.com/v2.4/me?fields=id,email"
            };
            facebookOptions.Scope.Add("email");
            app.UseFacebookAuthentication(facebookOptions);

            var googleOAuth2AuthenticationOptions = new GoogleOAuth2AuthenticationOptions
            {
            ClientId = "",
            ClientSecret = "",
            };
            app.UseGoogleAuthentication(googleOAuth2AuthenticationOptions);


            //app.UseGoogleAuthentication(){
            //    clientId:"694936874343-iqbj8p3ti3ha9hgfc1n80i2j225o9a2e.apps.googleusercontent.com",
            //    clientSecret: "kwXPYEfjhXC66KVrNxrRSQZF"
            //};

            //app.UseGoogleAuthentication(new GoogleOauth2AuthenticationOptions()){
            
            //}
        }
    }
}