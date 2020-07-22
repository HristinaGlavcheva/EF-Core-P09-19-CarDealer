using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AutoMapper;
using CarDealer.Data;
using CarDealer.DTO;
using CarDealer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        private const string ResultDirectoryPath = "../../../Datasets/Results";
        public static void Main(string[] args)
        {
            CarDealerContext context = new CarDealerContext();

            //Problem 09
            //string jsonString = File.ReadAllText("../../../Datasets/suppliers.json");
            //string result = ImportSuppliers(context, jsonString);

            //Probleme 10
            //string jsonString = File.ReadAllText("Datasets/parts.json");
            //string result = ImportParts(context, jsonString);

            //Problem 11
            //string jsonString = File.ReadAllText("Datasets/cars.json");
            //string result = ImportCars(context, jsonString);

            //Problem 12
            //string jsonString = File.ReadAllText("Datasets/customers.json");
            //string result = ImportCustomers(context, jsonString);

            //Problem 13
            //string jsonString = File.ReadAllText("Datasets/sales.json");
            //string result = ImportSales(context, jsonString);

            if (!Directory.Exists(ResultDirectoryPath))
            {
                Directory.CreateDirectory(ResultDirectoryPath);
            }

            //Problem 14
            //string jsonResult = GetOrderedCustomers(context);
            //File.WriteAllText(ResultDirectoryPath + "/ordered-customers.json", jsonResult);

            //Problem 15
            //string jsonResult = GetCarsFromMakeToyota(context);
            //File.WriteAllText(ResultDirectoryPath + "/toyota-cars.json", jsonResult);

            //Problem 16
            //string jsonResult = GetLocalSuppliers(context);
            //File.WriteAllText(ResultDirectoryPath + "/local-suppliers.json", jsonResult);

            //Problem 17
            //string jsonResult = GetCarsWithTheirListOfParts(context);
            //File.WriteAllText(ResultDirectoryPath + "cars-and-parts.json", jsonResult);

            //Problem 18
            //string jsonResult = GetTotalSalesByCustomer(context);
            //File.WriteAllText(ResultDirectoryPath + "/customers-total-sales.json", jsonResult);

            //Problem 19
            string jsonResult = GetSalesWithAppliedDiscount(context);
            File.WriteAllText(ResultDirectoryPath + "/sales-discounts.json", jsonResult);

            //Console.WriteLine(result);
        }

        //Problem 09
        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            List<Supplier> suppliers = JsonConvert.DeserializeObject<List<Supplier>>(inputJson);

            context.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count}.";
        }

        //Problem 10
        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            //int suppliersCount = context.Suppliers.Count();

            //List<Part> parts = JsonConvert.DeserializeObject<List<Part>>(inputJson, settings)
            //    .Where(p => p.SupplierId <= suppliersCount)
            //    .ToList();

            var suppliersIds = context.Suppliers.Select(x => x.Id).ToList();

            var parts = JsonConvert.DeserializeObject<Part[]>(inputJson)
                .Where(x => suppliersIds.Contains(x.SupplierId))
                .ToList();

            context.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count}.";
        }

        //Problem 11
        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            var carsDTO = JsonConvert.DeserializeObject<List<CarDTO>>(inputJson);

            foreach (var carDTO in carsDTO)
            {
                Car newCar = new Car
                {
                    Make = carDTO.Make,
                    Model = carDTO.Model,
                    TravelledDistance = carDTO.TravelledDistance,
                };

                context.Cars.Add(newCar);

                foreach (var partId in carDTO.PartsId.Distinct())
                {
                    PartCar newPartCar = new PartCar
                    {
                        PartId = partId,
                        Car = newCar
                    };

                    context.PartCars.Add(newPartCar);

                    //вариант без Distinct
                    //if (newCar.PartCars.FirstOrDefault(p => p.PartId == partId) == null)
                    //{
                    //    context.PartCars.Add(newPartCar);
                    //}
                }
            }

            context.SaveChanges();

            return $"Successfully imported {carsDTO.Count}.";
        }

        //Problem 12
        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            var customers = JsonConvert.DeserializeObject<List<Customer>>(inputJson);

            context.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}.";
        }

        //Problem 13
        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            var sales = JsonConvert.DeserializeObject<List<Sale>>(inputJson);

            context.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count}.";
        }

        //Problem 14
        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context
                .Customers
                .OrderBy(c => c.BirthDate)
                .ThenBy(c => c.IsYoungDriver)
                .Select(c => new
                {
                    Name = c.Name,
                    BirthDate = c.BirthDate.ToString("dd/MM/yyyy"),
                    IsYoungDriver = c.IsYoungDriver
                })
                .ToList();
            
            string jsonResult = JsonConvert.SerializeObject(customers, Formatting.Indented);
            return jsonResult;
        }

        //Problem 15
        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var toyotaCars = context
                .Cars
                .Where(c => c.Make == "Toyota")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TravelledDistance)
                .Select(c => new
                {
                    Id = c.Id,
                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .ToList();

            string jsonResult = JsonConvert.SerializeObject(toyotaCars, Formatting.Indented);
            return jsonResult;
        }

        //Problem 16
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context
                .Suppliers
                .Where(s => s.IsImporter == false)
                .Select(s => new
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count
                })
                .ToList();

            string jsonResult = JsonConvert.SerializeObject(suppliers, Formatting.Indented);
            return jsonResult;
        }

        //Problem 17
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context
                .Cars
                .Select(c => new 
                {
                    car = new 
                    {
                        Make = c.Make,
                        Model = c.Model,
                        TravelledDistance = c.TravelledDistance
                    },
                    parts = c.PartCars.Select(pc => new 
                    {
                        Name = pc.Part.Name,
                        Price = pc.Part.Price.ToString("F2")
                    }).ToList()
                }).ToList();

            string jsonString = JsonConvert.SerializeObject(cars, Formatting.Indented);
            return jsonString;
        }

        //Problem 18
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context
                .Customers
                .Where(c => c.Sales.Count > 0)
                .Select(c => new
                {
                    fullName = c.Name,
                    boughtCars = c.Sales.Count,
                    spentMoney = /*c.IsYoungDriver == false ?*/ c.Sales.Sum(s => s.Car.PartCars.Sum(pc => pc.Part.Price)) //* (100 - s.Discount) / 100)) //:
                    //c.Sales.Sum(s => s.Car.PartCars.Sum(pc => pc.Part.Price * (100 - s.Discount - 5) / 100))
                })
                .OrderByDescending(c => c.spentMoney)
                .ThenByDescending(c => c.boughtCars)
                .ToList();

            string jsonString = JsonConvert.SerializeObject(customers, Formatting.Indented);
            return jsonString;
        }

        //Problem 19
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context
                .Sales
                .Select(s => new
                {
                    car = new
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance
                    },
                    customerName = s.Customer.Name,
                    Discount = s.Discount.ToString("F2"),
                    price = s.Car.PartCars.Sum(pc => pc.Part.Price).ToString("F2"),
                    priceWithDiscount = (s.Car.PartCars.Sum(pc => pc.Part.Price) - s.Discount / 100 * s.Car.PartCars.Sum(pc => pc.Part.Price)).ToString("F2")
                })
                .Take(10)
                .ToList();

            string jsonString = JsonConvert.SerializeObject(sales, Formatting.Indented);
            return jsonString;
        }
    }
}