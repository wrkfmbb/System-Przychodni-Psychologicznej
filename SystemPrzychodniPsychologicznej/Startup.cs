using Microsoft.Owin;
using Owin;
using SystemPrzychodniPsychologicznej.Controllers;
using Hangfire;
using System;

[assembly: OwinStartupAttribute(typeof(SystemPrzychodniPsychologicznej.Startup))]
namespace SystemPrzychodniPsychologicznej
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            GlobalConfiguration.Configuration
              .UseSqlServerStorage("DefaultConnection");

            app.UseHangfireDashboard("/hf");

            EmailController obj = new EmailController();
            RecurringJob.AddOrUpdate(() => obj.SendEmail(), Cron.Daily(11));

            app.UseHangfireServer();
        }
    }
}
