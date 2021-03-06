﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuadBike.Common.Filters.BikeFilter;
using QuadBike.Common.Filters.OrderFilter;
using QuadBike.DataProvider.Interfaces;
using QuadBike.Logic.Interfaces;
using QuadBike.Model.Context.CommitProvider;
using QuadBike.Model.ViewModel.BikeViewModels;
using QuadBike.Model.ViewModel.OrderViewModels;
using QuadBike.Model.ViewModel.Pagination;

namespace QuadBike.Website.Controllers
{
    [Authorize(Roles = "provider")]
    public class BikeController : Controller
    {
        private readonly IBikeService _bikeService;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;
        private readonly IUserManagerService _userManagerService;
        private readonly ICommitProvider _commitProvider;

        public BikeController(IBikeService bikeService, IUserManagerService userManagerService, ICommitProvider commitProvider, IOrderRepository orderRepository, IOrderService orderService)
        {
            _bikeService = bikeService;
            _userManagerService = userManagerService;
            _commitProvider = commitProvider;
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        public IActionResult Index(string name, int page = 1, SortState sortOrder = SortState.NameAsc)
        {
            var currentUserName = User.Identity.Name;
            var userId = _userManagerService.GetUserByName(currentUserName);

            int pageSize = 10;

            var source = _bikeService.GetBikesOfCurrentProvider(userId.Result.Id);

            if (!String.IsNullOrEmpty(name))
            {
                source = source.Where(p => p.Name.Contains(name));
            }

            switch (sortOrder)
            {
                case SortState.NameDesc:
                    source = source.OrderByDescending(s => s.Name);
                    break;
                case SortState.MaxSpeedAsc:
                    source = source.OrderBy(s => s.MaxSpeed);
                    break;
                case SortState.MaxSpeedDesc:
                    source = source.OrderByDescending(s => s.MaxSpeed);
                    break;
                case SortState.TypeEngineAsc:
                    source = source.OrderBy(s => s.TypeEngine);
                    break;
                case SortState.TypeEngineDesc:
                    source = source.OrderByDescending(s => s.TypeEngine);
                    break;
                case SortState.PowerAsc:
                    source = source.OrderBy(s => s.Power);
                    break;
                case SortState.PowerDesc:
                    source = source.OrderByDescending(s => s.Power);
                    break;
                case SortState.FuelAsc:
                    source = source.OrderBy(s => s.Fuel);
                    break;
                case SortState.FuelDesc:
                    source = source.OrderByDescending(s => s.Fuel);
                    break;
                case SortState.PriceAsc:
                    source = source.OrderBy(s => s.Price);
                    break;
                case SortState.PriceDesc:
                    source = source.OrderByDescending(s => s.Price);
                    break;

                default:
                    source = source.OrderBy(s => s.Name);
                    break;
            }

            var count = source.Count();
            var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);
            BikeViewModel viewModel = new BikeViewModel
            {
                PageViewModel = pageViewModel,
                BikeFilterViewModel = new BikeFilterViewModel(name),
                BikeSortViewModel = new BikeSortViewModel(sortOrder),
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
            return RedirectToAction("Index");
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
                    if (model.BikeImg != null)
                    {
                        byte[] imageData = null;
                        using (var binaryReader = new BinaryReader(model.BikeImg.OpenReadStream()))
                        {
                            imageData = binaryReader.ReadBytes((int)model.BikeImg.Length);
                        }

                        bike.Name = model.Name;
                        bike.MaxSpeed = model.MaxSpeed;
                        bike.TypeEngine = model.TypeEngine;
                        bike.Power = model.Power;
                        bike.Fuel = model.Fuel;
                        bike.Description = model.Description;
                        bike.Price = model.Price;
                        bike.BikeImg = imageData;

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
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult OrderList(string name, int page = 1, SortStateTwo sortOrderTwo = SortStateTwo.NameAsc)
        {
            int pageSize = 10;
            var currentUserName = User.Identity.Name;
            var userId = _userManagerService.GetUserByName(currentUserName);
            var source = _orderService.OrdersForCurrentProvider(userId.Result.Id);

            if (!String.IsNullOrEmpty(name))
            {
                source = source.Where(p => p.Name.Contains(name));
            }

            // сортировка
            switch (sortOrderTwo)
            {
                case SortStateTwo.NameDesc:
                    source = source.OrderByDescending(s => s.Name);
                    break;
                case SortStateTwo.DateAsc:
                    source = source.OrderBy(s => s.OrderPlaced);
                    break;
                case SortStateTwo.DateDesc:
                    source = source.OrderByDescending(s => s.OrderPlaced);
                    break;
                default:
                    source = source.OrderBy(s => s.Name);
                    break;
            }


            var count = source.Count();
            var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);
            OrderViewModel viewModel = new OrderViewModel()
            {
                PageViewModel = pageViewModel,
                OrderFilterViewModel = new OrderFilterViewModel(name),
                OrderSortViewModel = new OrderSortViewModel(sortOrderTwo),
                Orders = items
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult InfoOrderList(int id)
        {
            var currentUserName = User.Identity.Name;
            var userId = _userManagerService.GetUserByName(currentUserName);
            var res = _orderService.OrderDetailsOfOrderById(id, userId.Result.Id);
            return View(res);
        }

        [HttpGet]
        public IActionResult ContactProvider(string id)
        {
            var res = _userManagerService.GetUserById(id);
            return View(res);
        }

        [HttpPost]
        public IActionResult DeleteOrder(int? orderId)
        {
            if (orderId != null)
            {
                _orderService.DeleteOrderById(orderId);
                _commitProvider.Save();
                return RedirectToAction("OrderList");
            }
            return NotFound();
        }
    }
}