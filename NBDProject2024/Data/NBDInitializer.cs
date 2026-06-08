using NBDProject2024.Models;

namespace NBD2024.Data
{
    public static class NBDInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using var scope = applicationBuilder.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<NBDProject2024.Data.NBDContext>();
            Seed(context);
        }

        public static void Seed(NBDProject2024.Data.NBDContext context)
        {
            SeedProvincesAndCities(context);
            SeedEmployees(context);
            SeedCoreCatalogs(context);
            SeedClientsAndProjects(context);
            SeedEmployeeSkills(context);
            SeedBids(context);
            SeedWorkOrders(context);
            SeedInventoryAndPurchases(context);
            SeedWorkOrderConsumptions(context);
        }

        private static void SeedProvincesAndCities(NBDProject2024.Data.NBDContext context)
        {
            if (!context.Provinces.Any())
            {
                context.Provinces.AddRange(
                    new Province { ID = "AB", Name = "Alberta" },
                    new Province { ID = "BC", Name = "British Columbia" },
                    new Province { ID = "MB", Name = "Manitoba" },
                    new Province { ID = "NB", Name = "New Brunswick" },
                    new Province { ID = "NS", Name = "Nova Scotia" },
                    new Province { ID = "ON", Name = "Ontario" },
                    new Province { ID = "PE", Name = "Prince Edward Island" },
                    new Province { ID = "QC", Name = "Quebec" },
                    new Province { ID = "SK", Name = "Saskatchewan" },
                    new Province { ID = "YT", Name = "Yukon" }
                );
                context.SaveChanges();
            }

            var citySeeds = new[]
            {
                ("Toronto", "ON"),
                ("St. Catharines", "ON"),
                ("Halifax", "NS"),
                ("Calgary", "AB"),
                ("Vancouver", "BC"),
                ("Winnipeg", "MB"),
                ("Quebec City", "QC"),
                ("Saskatoon", "SK"),
                ("Charlottetown", "PE"),
                ("Fredericton", "NB")
            };

            foreach (var (name, provinceId) in citySeeds)
            {
                if (!context.Cities.Any(c => c.Name == name && c.ProvinceID == provinceId))
                {
                    context.Cities.Add(new City { Name = name, ProvinceID = provinceId });
                }
            }

            context.SaveChanges();
        }

        private static void SeedEmployees(NBDProject2024.Data.NBDContext context)
        {
            var employeeSeeds = new[]
            {
                ("admin@outlook.com", "Admin", "User", "9051110001", Positionemp.Management),
                ("super@outlook.com", "Supervisor", "User", "9051110002", Positionemp.Management),
                ("sales@outlook.com", "Sales", "User", "9051110003", Positionemp.Sales),
                ("designer@outlook.com", "Designer", "User", "9051110004", Positionemp.Design),
                ("root@test.com", "Root", "User", "9051110005", Positionemp.Management),
                ("worker1@nbd.local", "Ethan", "Cole", "9051110006", Positionemp.Worker),
                ("worker2@nbd.local", "Ava", "Grant", "9051110007", Positionemp.Worker),
                ("worker3@nbd.local", "Liam", "Knight", "9051110008", Positionemp.Worker),
                ("worker4@nbd.local", "Sofia", "Banks", "9051110009", Positionemp.Worker),
                ("worker5@nbd.local", "Mason", "Reed", "9051110010", Positionemp.Worker)
            };

            foreach (var (email, firstName, lastName, phone, position) in employeeSeeds)
            {
                var employee = context.Employees.FirstOrDefault(e => e.Email == email);
                if (employee == null)
                {
                    context.Employees.Add(new Employee
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        Phone = phone,
                        Position = position,
                        Active = true,
                        Prescriber = false
                    });
                }
                else
                {
                    employee.FirstName = firstName;
                    employee.LastName = lastName;
                    employee.Phone = phone;
                    employee.Position = position;
                    employee.Active = true;
                }
            }

            context.SaveChanges();
        }

        private static void SeedCoreCatalogs(NBDProject2024.Data.NBDContext context)
        {
            var materialSeeds = new[]
            {
                ("Mulch", "Brown hardwood mulch", 6.50),
                ("Topsoil", "Screened topsoil", 4.20),
                ("Pavers", "Concrete pavers", 3.75),
                ("River Stone", "Decorative river stone", 5.80),
                ("Sod", "Kentucky bluegrass sod", 2.95),
                ("Compost", "Organic compost blend", 3.10),
                ("Cedar Edging", "Cedar wood edging", 8.25),
                ("Garden Fabric", "Weed control fabric", 1.45),
                ("Drain Gravel", "3/4 inch drainage gravel", 4.85),
                ("Limestone Screenings", "Compaction screenings", 3.35)
            };

            foreach (var (name, description, price) in materialSeeds)
            {
                var material = context.Materials.FirstOrDefault(m => m.Name == name);
                if (material == null)
                {
                    context.Materials.Add(new Material { Name = name, Description = description, Price = price });
                }
                else
                {
                    material.Description = description;
                    material.Price = price;
                }
            }

            var labourSeeds = new[]
            {
                ("Site Prep", "Ground preparation", 38.00),
                ("Installation", "Landscape installation", 45.00),
                ("Finishing", "Final detailing", 32.00),
                ("Irrigation Setup", "Irrigation lines and testing", 44.00),
                ("Planting", "Plant and shrub placement", 36.00),
                ("Hardscape Build", "Hardscape assembly", 48.00),
                ("Cleanup", "Final cleanup and waste disposal", 29.00),
                ("Soil Grading", "Site grading and leveling", 34.50),
                ("Lighting Install", "Landscape lighting setup", 41.00),
                ("Inspection", "Site quality inspection", 33.50)
            };

            foreach (var (name, description, price) in labourSeeds)
            {
                var labour = context.Labours.FirstOrDefault(l => l.Name == name);
                if (labour == null)
                {
                    context.Labours.Add(new Labour { Name = name, Description = description, Price = price });
                }
                else
                {
                    labour.Description = description;
                    labour.Price = price;
                }
            }

            var locationSeeds = new[]
            {
                ("Main Warehouse", StockLocationType.Warehouse, "Primary warehouse"),
                ("North Warehouse", StockLocationType.Warehouse, "North zone warehouse"),
                ("South Warehouse", StockLocationType.Warehouse, "South zone warehouse"),
                ("Truck A", StockLocationType.Truck, "Crew truck A"),
                ("Truck B", StockLocationType.Truck, "Crew truck B")
            };

            foreach (var (name, type, notes) in locationSeeds)
            {
                var location = context.StockLocations.FirstOrDefault(l => l.Name == name && l.LocationType == type);
                if (location == null)
                {
                    context.StockLocations.Add(new StockLocation
                    {
                        Name = name,
                        LocationType = type,
                        IsActive = true,
                        Notes = notes
                    });
                }
                else
                {
                    location.IsActive = true;
                    location.Notes = notes;
                }
            }

            context.SaveChanges();
        }

        private static void SeedClientsAndProjects(NBDProject2024.Data.NBDContext context)
        {
            var cityIds = context.Cities.OrderBy(c => c.ID).Select(c => c.ID).ToList();
            if (!cityIds.Any())
            {
                return;
            }

            var clientSeeds = new[]
            {
                ("Mia", "Turner", "Northgate Offices", "9052221001", "northgate@nbd.local", "100 King St", "L3C4N7"),
                ("Noah", "Lee", "Harbor Retail Center", "9052221002", "harbor@nbd.local", "25 Lake Ave", "L3C5P8"),
                ("Emma", "Frost", "Maple Medical", "9052221003", "maple@nbd.local", "12 Pine Rd", "L3C6A1"),
                ("Oliver", "Stone", "Crescent Condos", "9052221004", "crescent@nbd.local", "88 Oak Dr", "L3C6A2"),
                ("Sophia", "West", "Bayside Logistics", "9052221005", "bayside@nbd.local", "77 Harbor Blvd", "L3C6A3"),
                ("Lucas", "Young", "Grandview School", "9052221006", "grandview@nbd.local", "5 School Ln", "L3C6A4"),
                ("Isla", "Ward", "Elmwood Plaza", "9052221007", "elmwood@nbd.local", "43 Elm St", "L3C6A5"),
                ("Henry", "Price", "Silverline Towers", "9052221008", "silverline@nbd.local", "9 Tower Way", "L3C6A6"),
                ("Amelia", "Scott", "Parklane Residences", "9052221009", "parklane@nbd.local", "61 Park Rd", "L3C6A7"),
                ("James", "Miller", "Riverview Campus", "9052221010", "riverview@nbd.local", "200 River St", "L3C6A8")
            };

            var clientIds = new List<int>();
            for (int i = 0; i < clientSeeds.Length; i++)
            {
                var seed = clientSeeds[i];
                var cityId = cityIds[i % cityIds.Count];

                var client = context.Clients.FirstOrDefault(c => c.Email == seed.Item5);
                if (client == null)
                {
                    client = new Client
                    {
                        FirstName = seed.Item1,
                        LastName = seed.Item2,
                        CompanyName = seed.Item3,
                        Phone = seed.Item4,
                        Email = seed.Item5,
                        AddressCountry = "Canada",
                        AddressStreet = seed.Item6,
                        PostalCode = seed.Item7,
                        CityID = cityId
                    };
                    context.Clients.Add(client);
                    context.SaveChanges();
                }
                else
                {
                    client.FirstName = seed.Item1;
                    client.LastName = seed.Item2;
                    client.CompanyName = seed.Item3;
                    client.Phone = seed.Item4;
                    client.AddressCountry = "Canada";
                    client.AddressStreet = seed.Item6;
                    client.PostalCode = seed.Item7;
                    client.CityID = cityId;
                    context.SaveChanges();
                }

                clientIds.Add(client.ID);
            }

            var projectSeeds = new[]
            {
                "Northgate Entrance Refresh",
                "Harbor Patio Revamp",
                "Maple Parking Border",
                "Crescent Courtyard Upgrade",
                "Bayside Loading Zone",
                "Grandview Field Access",
                "Elmwood Walkway Renewal",
                "Silverline Terrace Build",
                "Parklane Courtyard Modernization",
                "Riverview Main Quad"
            };

            for (int i = 0; i < projectSeeds.Length; i++)
            {
                var projectName = projectSeeds[i];
                var project = context.Projects.FirstOrDefault(p => p.ProjectName == projectName);
                var startDate = DateTime.Today.AddDays(-(20 - i));
                var endDate = startDate.AddDays(20 + (i % 4));
                var cityId = cityIds[i % cityIds.Count];

                if (project == null)
                {
                    context.Projects.Add(new Project
                    {
                        ProjectName = projectName,
                        StartTime = startDate,
                        EndTime = endDate,
                        ProjectSite = $"Site {(i + 1):00}",
                        SetupNotes = "Extended deterministic seed project",
                        ClientID = clientIds[i],
                        CityID = cityId
                    });
                }
                else
                {
                    project.StartTime = startDate;
                    project.EndTime = endDate;
                    project.ProjectSite = $"Site {(i + 1):00}";
                    project.SetupNotes = "Extended deterministic seed project";
                    project.ClientID = clientIds[i];
                    project.CityID = cityId;
                }
            }

            context.SaveChanges();
        }

        private static void SeedEmployeeSkills(NBDProject2024.Data.NBDContext context)
        {
            var employees = context.Employees.OrderBy(e => e.ID).Take(10).ToList();
            if (!employees.Any())
            {
                return;
            }

            var skills = Enum.GetValues<EmployeeSkillType>();
            for (int i = 0; i < employees.Count; i++)
            {
                var employee = employees[i];
                var skill = skills[i % skills.Length];

                if (!context.EmployeeSkills.Any(es => es.EmployeeID == employee.ID && es.Skill == skill))
                {
                    context.EmployeeSkills.Add(new EmployeeSkill
                    {
                        EmployeeID = employee.ID,
                        Skill = skill,
                        Level = 3 + (i % 3)
                    });
                }
            }

            context.SaveChanges();
        }

        private static void SeedBids(NBDProject2024.Data.NBDContext context)
        {
            var projects = context.Projects.OrderBy(p => p.ID).Take(10).ToList();
            var materials = context.Materials.OrderBy(m => m.ID).Take(10).ToList();
            var labours = context.Labours.OrderBy(l => l.ID).Take(10).ToList();

            if (projects.Count < 10 || materials.Count < 10 || labours.Count < 10)
            {
                return;
            }

            for (int i = 0; i < projects.Count; i++)
            {
                var project = projects[i];
                var bidDate = DateTime.Today.AddDays(-(i + 1));

                var bid = context.Bids.FirstOrDefault(b => b.ProjectID == project.ID);
                if (bid == null)
                {
                    bid = new Bid { ProjectID = project.ID, BidDate = bidDate };
                    context.Bids.Add(bid);
                    context.SaveChanges();
                }
                else
                {
                    bid.BidDate = bidDate;
                    context.SaveChanges();
                }

                EnsureBidMaterial(context, bid.ID, materials[i % materials.Count].ID, 20 + i);
                EnsureBidMaterial(context, bid.ID, materials[(i + 3) % materials.Count].ID, 10 + (i * 2));
                EnsureBidLabour(context, bid.ID, labours[i % labours.Count].ID, 6 + (i % 5));
                EnsureBidLabour(context, bid.ID, labours[(i + 4) % labours.Count].ID, 4 + (i % 4));
            }

            context.SaveChanges();
        }

        private static void EnsureBidMaterial(NBDProject2024.Data.NBDContext context, int bidId, int materialId, int qty)
        {
            var row = context.BidMaterials.FirstOrDefault(x => x.BidID == bidId && x.MaterialID == materialId);
            if (row == null)
            {
                context.BidMaterials.Add(new BidMaterial { BidID = bidId, MaterialID = materialId, MaterialQuantity = qty });
            }
            else
            {
                row.MaterialQuantity = qty;
            }
        }

        private static void EnsureBidLabour(NBDProject2024.Data.NBDContext context, int bidId, int labourId, double hours)
        {
            var row = context.BidLabours.FirstOrDefault(x => x.BidID == bidId && x.LabourID == labourId);
            if (row == null)
            {
                context.BidLabours.Add(new BidLabour { BidID = bidId, LabourID = labourId, HoursQuantity = hours });
            }
            else
            {
                row.HoursQuantity = hours;
            }
        }

        private static void SeedWorkOrders(NBDProject2024.Data.NBDContext context)
        {
            var projects = context.Projects.OrderBy(p => p.ID).Take(10).ToList();
            var employees = context.Employees.OrderBy(e => e.ID).Take(10).ToList();
            var statuses = new[]
            {
                WorkOrderStatus.Pending,
                WorkOrderStatus.Scheduled,
                WorkOrderStatus.InProgress,
                WorkOrderStatus.Completed,
                WorkOrderStatus.Cancelled,
                WorkOrderStatus.Pending,
                WorkOrderStatus.Scheduled,
                WorkOrderStatus.InProgress,
                WorkOrderStatus.Completed,
                WorkOrderStatus.Cancelled
            };
            var skills = Enum.GetValues<EmployeeSkillType>();

            if (projects.Count < 10 || employees.Count < 10)
            {
                return;
            }

            for (int i = 0; i < 10; i++)
            {
                var project = projects[i];
                var status = statuses[i];
                var scheduledDate = DateTime.Today.AddDays(i - 4);
                var completedOn = status == WorkOrderStatus.Completed ? scheduledDate.AddDays(1) : (DateTime?)null;
                var title = $"WO-{project.ID:000}-{i + 1:00}";

                var workOrder = context.WorkOrders.FirstOrDefault(w => w.Title == title);
                if (workOrder == null)
                {
                    workOrder = new WorkOrder
                    {
                        Title = title,
                        ProjectID = project.ID,
                        ScheduledDate = scheduledDate,
                        Status = status,
                        AssignedCrew = $"Crew {(i % 3) + 1}",
                        CompletedOn = completedOn,
                        Notes = "Extended deterministic seeded work order"
                    };
                    context.WorkOrders.Add(workOrder);
                    context.SaveChanges();
                }
                else
                {
                    workOrder.ProjectID = project.ID;
                    workOrder.ScheduledDate = scheduledDate;
                    workOrder.Status = status;
                    workOrder.AssignedCrew = $"Crew {(i % 3) + 1}";
                    workOrder.CompletedOn = completedOn;
                    workOrder.Notes = "Extended deterministic seeded work order";
                    context.SaveChanges();
                }

                var employee = employees[i];
                var assignment = context.WorkOrderCrewAssignments.FirstOrDefault(a => a.WorkOrderID == workOrder.ID && a.EmployeeID == employee.ID);
                var estimatedHours = 4 + (i % 5);
                var actualHours = status == WorkOrderStatus.Completed ? estimatedHours : (status == WorkOrderStatus.InProgress ? Math.Max(1, estimatedHours - 2) : 0);

                if (assignment == null)
                {
                    context.WorkOrderCrewAssignments.Add(new WorkOrderCrewAssignment
                    {
                        WorkOrderID = workOrder.ID,
                        EmployeeID = employee.ID,
                        AssignedSkill = skills[i % skills.Length],
                        EstimatedHours = estimatedHours,
                        ActualHours = actualHours
                    });
                }
                else
                {
                    assignment.AssignedSkill = skills[i % skills.Length];
                    assignment.EstimatedHours = estimatedHours;
                    assignment.ActualHours = actualHours;
                }
            }

            context.SaveChanges();
        }

        private static void SeedInventoryAndPurchases(NBDProject2024.Data.NBDContext context)
        {
            var materials = context.Materials.OrderBy(m => m.ID).Take(10).ToList();
            var warehouse = context.StockLocations.FirstOrDefault(l => l.Name == "Main Warehouse" && l.LocationType == StockLocationType.Warehouse);
            if (materials.Count < 10 || warehouse == null)
            {
                return;
            }

            foreach (var material in materials)
            {
                var stock = context.MaterialStocks.FirstOrDefault(s => s.MaterialID == material.ID && s.StockLocationID == warehouse.ID);
                if (stock == null)
                {
                    stock = new MaterialStock
                    {
                        MaterialID = material.ID,
                        StockLocationID = warehouse.ID,
                        QuantityOnHand = 60,
                        MinQuantity = 15,
                        LastUnitCost = material.Price
                    };
                    context.MaterialStocks.Add(stock);
                    context.SaveChanges();
                }

                var openingRef = $"OPEN-{material.ID}-{warehouse.ID}";
                if (!context.InventoryMovements.Any(m => m.ReferenceCode == openingRef))
                {
                    context.InventoryMovements.Add(new InventoryMovement
                    {
                        MovementDate = DateTime.Today.AddDays(-30),
                        MovementType = InventoryMovementType.OpeningBalance,
                        MaterialID = material.ID,
                        StockLocationID = warehouse.ID,
                        QuantityDelta = stock.QuantityOnHand,
                        QuantityBefore = 0,
                        QuantityAfter = stock.QuantityOnHand,
                        UnitCost = material.Price,
                        ReferenceCode = openingRef,
                        Notes = "Seeded opening balance"
                    });
                }
            }

            context.SaveChanges();

            var statuses = new[]
            {
                PurchaseRequestStatus.Draft,
                PurchaseRequestStatus.Submitted,
                PurchaseRequestStatus.PartiallyReceived,
                PurchaseRequestStatus.Received,
                PurchaseRequestStatus.Cancelled,
                PurchaseRequestStatus.Draft,
                PurchaseRequestStatus.Submitted,
                PurchaseRequestStatus.PartiallyReceived,
                PurchaseRequestStatus.Received,
                PurchaseRequestStatus.Cancelled
            };

            for (int i = 0; i < 10; i++)
            {
                var supplier = $"Seeder Supplier {(i + 1):00}";
                var material = materials[i % materials.Count];
                var request = context.PurchaseRequests.FirstOrDefault(p => p.SupplierName == supplier);

                if (request == null)
                {
                    request = new PurchaseRequest
                    {
                        SupplierName = supplier,
                        RequestDate = DateTime.Today.AddDays(-(10 - i)),
                        Status = statuses[i],
                        ApprovedBy = statuses[i] == PurchaseRequestStatus.Draft ? null : "admin@outlook.com",
                        ApprovedOn = statuses[i] == PurchaseRequestStatus.Draft ? null : DateTime.UtcNow.AddDays(-(8 - i)),
                        Notes = "Extended deterministic seeded purchase request"
                    };
                    context.PurchaseRequests.Add(request);
                    context.SaveChanges();
                }
                else
                {
                    request.RequestDate = DateTime.Today.AddDays(-(10 - i));
                    request.Status = statuses[i];
                    request.ApprovedBy = statuses[i] == PurchaseRequestStatus.Draft ? null : "admin@outlook.com";
                    request.ApprovedOn = statuses[i] == PurchaseRequestStatus.Draft ? null : DateTime.UtcNow.AddDays(-(8 - i));
                    request.Notes = "Extended deterministic seeded purchase request";
                    context.SaveChanges();
                }

                var requestedQty = 15 + i;
                var line = context.PurchaseRequestLines.FirstOrDefault(l => l.PurchaseRequestID == request.ID && l.MaterialID == material.ID);
                if (line == null)
                {
                    context.PurchaseRequestLines.Add(new PurchaseRequestLine
                    {
                        PurchaseRequestID = request.ID,
                        MaterialID = material.ID,
                        RequestedQty = requestedQty,
                        EstimatedUnitCost = material.Price
                    });
                    context.SaveChanges();
                }
                else
                {
                    line.RequestedQty = requestedQty;
                    line.EstimatedUnitCost = material.Price;
                    context.SaveChanges();
                }

                if (statuses[i] == PurchaseRequestStatus.PartiallyReceived || statuses[i] == PurchaseRequestStatus.Received)
                {
                    var receipt = context.PurchaseReceiptLines.FirstOrDefault(r => r.PurchaseRequestID == request.ID && r.MaterialID == material.ID);
                    if (receipt == null)
                    {
                        var receivedQty = statuses[i] == PurchaseRequestStatus.Received ? requestedQty : Math.Round(requestedQty * 0.5, 2);
                        var stock = context.MaterialStocks.First(s => s.MaterialID == material.ID && s.StockLocationID == warehouse.ID);
                        var before = stock.QuantityOnHand;
                        var after = before + receivedQty;
                        stock.QuantityOnHand = after;
                        stock.LastUnitCost = material.Price;

                        context.PurchaseReceiptLines.Add(new PurchaseReceiptLine
                        {
                            PurchaseRequestID = request.ID,
                            MaterialID = material.ID,
                            StockLocationID = warehouse.ID,
                            ReceivedDate = DateTime.Today.AddDays(-(5 - i)),
                            ReceivedQty = receivedQty,
                            ActualUnitCost = material.Price,
                            SupplierInvoice = $"SEED-{request.ID:0000}"
                        });

                        context.InventoryMovements.Add(new InventoryMovement
                        {
                            MovementDate = DateTime.Today.AddDays(-(5 - i)),
                            MovementType = InventoryMovementType.PurchaseReceipt,
                            MaterialID = material.ID,
                            StockLocationID = warehouse.ID,
                            QuantityDelta = receivedQty,
                            QuantityBefore = before,
                            QuantityAfter = after,
                            UnitCost = material.Price,
                            ReferenceCode = $"PR-{request.ID}",
                            Notes = "Seeded purchase receipt"
                        });

                        context.SaveChanges();
                    }
                }
            }
        }

        private static void SeedWorkOrderConsumptions(NBDProject2024.Data.NBDContext context)
        {
            var warehouse = context.StockLocations.FirstOrDefault(l => l.Name == "Main Warehouse" && l.LocationType == StockLocationType.Warehouse);
            if (warehouse == null)
            {
                return;
            }

            var workOrders = context.WorkOrders
                .Where(w => w.Status == WorkOrderStatus.InProgress || w.Status == WorkOrderStatus.Completed)
                .OrderBy(w => w.ID)
                .Take(10)
                .ToList();
            var materials = context.Materials.OrderBy(m => m.ID).Take(10).ToList();

            if (!workOrders.Any() || !materials.Any())
            {
                return;
            }

            for (int i = 0; i < workOrders.Count; i++)
            {
                var workOrder = workOrders[i];
                var material = materials[i % materials.Count];
                var consumedOn = workOrder.ScheduledDate.AddDays(workOrder.Status == WorkOrderStatus.Completed ? 1 : 0);
                var quantityUsed = 2 + (i % 4);

                var existing = context.WorkOrderMaterialConsumptions.FirstOrDefault(c =>
                    c.WorkOrderID == workOrder.ID &&
                    c.MaterialID == material.ID &&
                    c.StockLocationID == warehouse.ID &&
                    c.ConsumedOn.Date == consumedOn.Date);

                if (existing != null)
                {
                    continue;
                }

                var stock = context.MaterialStocks.FirstOrDefault(s => s.MaterialID == material.ID && s.StockLocationID == warehouse.ID);
                if (stock == null)
                {
                    continue;
                }

                var before = stock.QuantityOnHand;
                var after = Math.Max(0, before - quantityUsed);
                stock.QuantityOnHand = after;

                context.WorkOrderMaterialConsumptions.Add(new WorkOrderMaterialConsumption
                {
                    WorkOrderID = workOrder.ID,
                    MaterialID = material.ID,
                    StockLocationID = warehouse.ID,
                    ConsumedOn = consumedOn,
                    QuantityUsed = quantityUsed,
                    UnitCostAtUse = material.Price,
                    Notes = "Seeded work order material consumption"
                });

                context.InventoryMovements.Add(new InventoryMovement
                {
                    MovementDate = consumedOn,
                    MovementType = InventoryMovementType.Consumption,
                    MaterialID = material.ID,
                    StockLocationID = warehouse.ID,
                    QuantityDelta = -quantityUsed,
                    QuantityBefore = before,
                    QuantityAfter = after,
                    UnitCost = material.Price,
                    ReferenceCode = $"WO-{workOrder.ID}",
                    Notes = "Seeded consumption from work order"
                });
            }

            context.SaveChanges();
        }
    }
}
