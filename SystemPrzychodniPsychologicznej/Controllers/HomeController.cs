﻿using System.Web.Mvc;


namespace SystemPrzychodniPsychologicznej.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {


            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your about page.";


            return View();
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}