﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuadBike.Logic.Interfaces;
using QuadBike.Website.Models;

namespace QuadBike.Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProviderService _providerService;
        private readonly IUserManagerService _userManagerService;

        public HomeController(IProviderService providerService, IUserManagerService userManagerService)
        {
            _providerService = providerService;
            _userManagerService = userManagerService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
