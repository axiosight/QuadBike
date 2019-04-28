﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuadBike.Logic.Interfaces;
using QuadBike.Model.Context.CommitProvider;
using QuadBike.Model.ViewModel.BikeViewModels;
using QuadBike.Model.ViewModel.Pagination;

namespace QuadBike.Website.Controllers
{
    [Authorize(Roles = "provider")]
    public class BikeController : Controller
    {
        private readonly IBikeService _bikeService;
        private readonly IUserManagerService _userManagerService;
        private readonly ICommitProvider _commitProvider;

        public BikeController(IBikeService bikeService, IUserManagerService userManagerService, ICommitProvider commitProvider)
        {
            _bikeService = bikeService;
            _userManagerService = userManagerService;
            _commitProvider = commitProvider;
        }

        public IActionResult Index(int page = 1)
        {
            var currentUserName = User.Identity.Name;
            var userId = _userManagerService.GetUserByName(currentUserName);

            int pageSize = 10;   // количество элементов на странице

            var source = _bikeService.GetBikesOfCurrentProvider(userId.Result.Id);
            var count = source.Count();
            var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);
            BikeViewModel viewModel = new BikeViewModel
            {
                PageViewModel = pageViewModel,
                Bikes = items
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Delete")]
        public IActionResult Delete(int? id)
        {
            if (id != null)
            {
                _bikeService.DeleteById(id);
                _commitProvider.Save();
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult Create(BikeViewModel bike)
        {
            var currentUserName = User.Identity.Name;
            var userId = _userManagerService.GetUserByName(currentUserName);
            
            if (ModelState.IsValid)
            {
                if (bike.BikeImg != null)
                {
                    byte[] imageData = null;
                    using (var binaryReader = new BinaryReader(bike.BikeImg.OpenReadStream()))
                    {
                        imageData = binaryReader.ReadBytes((int)bike.BikeImg.Length);
                    }
                    _bikeService.CreateBike(bike, userId.Result.Id, imageData);
                    return RedirectToAction("Index");
                }
            }
            return View(bike);
        }

        public IActionResult Edit(int id)
        {
            var res = _bikeService.GetBikeById(id);

            if (res == null)
            {
                return NotFound();
            }
            ChangeBikeViewModel model = new ChangeBikeViewModel
            {
                Id = res.Id,
                Name = res.Name,
                MaxSpeed = res.MaxSpeed,
                TypeEngine = res.TypeEngine,
                Power = res.Power,
                Fuel = res.Fuel,
                Description = res.Description,
                Price = res.Price
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(ChangeBikeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var bike = _bikeService.GetBikeById(model.Id);

                if (bike != null)
                {
                    bike.Name = model.Name;
                    bike.MaxSpeed = model.MaxSpeed;
                    bike.TypeEngine = model.TypeEngine;
                    bike.Power = model.Power;
                    bike.Fuel = model.Fuel;
                    bike.Description = model.Description;
                    bike.Price = model.Price;

                    var result = _bikeService.Update(bike);
                    if (result == true)
                    {
                        _commitProvider.Save();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            return View(model);
        }
    }
}