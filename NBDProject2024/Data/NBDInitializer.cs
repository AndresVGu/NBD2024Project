using Humanizer;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.Data;
using NBDProject2024.Models;
using System.Diagnostics;

namespace NBD2024.Data
{
    public static class NBDInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            NBDContext context = applicationBuilder.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<NBDContext>();

            try
            {
                //Delete and recreate the Database with every restart
                //context.Database.EnsureDeleted();
                //context.Database.EnsureCreated();
                //context.Database.Migrate();

                //To randomly generate data
                Random random = new Random();

                #region Generate random Records

                static string GeneratePhoneNumber(Random random)
                {
                    //random area code
                    int areaCode = random.Next(100, 1000);
                    //random exchange code 
                    int exchangeCode = random.Next(100, 1000);
                    //random susbscriber number
                    int subscriberNumber = random.Next(100, 1000);
                    string phonerd = string.Format("{0:000}{1:000}{2:0000}", areaCode, exchangeCode, subscriberNumber);
                    return phonerd;
                }



                //Random Address:
                static string GenerateAddress(Random random)
                {
                    // Números de calle aleatorios (1-9999)
                    int streetNumber = random.Next(1, 10000);

                    // Nombres de calle aleatorios
                    string[] streetNames = { "Main St", "First Ave", "Elm St", "Maple Ave", "Oak St" };
                    string streetName = streetNames[random.Next(streetNames.Length)];

                    // Formatear la dirección
                    string address = $"{streetNumber} {streetName}";

                    return address;
                }
                //Random Postal Code:
                static string GeneratePostalCode(Random random)
                {
                    // Generar tres letras aleatorias
                    char[] letters = new char[3];
                    for (int i = 0; i < 3; i++)
                    {
                        letters[i] = (char)random.Next('A', 'Z' + 1);
                    }

                    // Generar tres dígitos aleatorios
                    int[] digits = new int[3];
                    for (int i = 0; i < 3; i++)
                    {
                        digits[i] = random.Next(0, 10);
                    }

                    // Formatear el código postal
                    string postalCode = string.Format("{0}{1}{2} {3}{4}{5}",
                                                      letters[0], digits[0],
                                                      letters[1], digits[1],
                                                      letters[2], digits[2]);

                    return postalCode;
                }

                #endregion

                //Provinces, Cities
                if (!context.Provinces.Any())
                {
                    var provinces = new List<Province>
                    {
                        new Province { ID = "ON", Name = "Ontario"},
                        new Province { ID = "PE", Name = "Prince Edward Island"},
                        new Province { ID = "NB", Name = "New Brunswick"},
                        new Province { ID = "BC", Name = "British Columbia"},
                        new Province { ID = "NL", Name = "Newfoundland and Labrador"},
                        new Province { ID = "SK", Name = "Saskatchewan"},
                        new Province { ID = "NS", Name = "Nova Scotia"},
                        new Province { ID = "MB", Name = "Manitoba"},
                        new Province { ID = "QC", Name = "Quebec"},
                        new Province { ID = "YT", Name = "Yukon"},
                        new Province { ID = "NU", Name = "Nunavut"},
                        new Province { ID = "NT", Name = "Northwest Territories"},
                        new Province { ID = "AB", Name = "Alberta"}
                    };
                    context.Provinces.AddRange(provinces);
                    context.SaveChanges();
                }
                if (!context.Cities.Any())
                {
                    var cities = new List<City>
                    {
                        new City { Name = "Toronto", ProvinceID="ON"},
                        new City { Name = "Halifax", ProvinceID="NS" },
                        new City { Name = "Calgary", ProvinceID="AB" },
                        new City { Name = "Winnipeg", ProvinceID="MB", },
                        new City { Name = "Stratford", ProvinceID="ON" },
                        new City { Name = "St. Catharines", ProvinceID="ON" },
                        new City { Name = "Welland", ProvinceID="ON" },
                        new City { Name = "Stratford", ProvinceID="PE" },
                        new City { Name = "Ancaster", ProvinceID="ON" },
                        new City { Name = "Vancouver", ProvinceID="BC" },
                    };
                    context.Cities.AddRange(cities);
                    context.SaveChanges();
                }

                //Look for any client. Since we can't have Projects without Clients.
                if (!context.Clients.Any())
                {
                    context.Clients.AddRange(
                        new Client
                        {
                            FirstName = "Amy",
                            LastName = "Benson",
                            CompanyName = "London Sq. Mall",
                            Phone = "4088945603",
                            Email = "BensonAmy1@outlook.com",
                            AddressCountry = "Canada",
                            AddressStreet = "Scott Valley",
                            PostalCode = "LC34N7"
                        },
                         new Client
                         {
                             FirstName = "Bob",
                             MiddleName = "Jones",
                             LastName = "Kellet",
                             CompanyName = "Quantum Innovations Co.",
                             Phone = GeneratePhoneNumber(random),
                             Email = "BobJKellet@outlook.com",
                             AddressCountry = "Canada",
                             AddressStreet = "21 example Rd.",
                             PostalCode = "LC34N7"
                         },
                         new Client
                         {
                             FirstName = "Jim",

                             LastName = "Cruz",
                             CompanyName = "Fusion Enterprices International",
                             Phone = GeneratePhoneNumber(random),
                             AddressCountry = "Canada",
                             Email = "JimCruz1@outlook.com",
                             AddressStreet = "21 example Rd.",

                             PostalCode = "LC34N7"
                         },
                         new Client
                         {
                             FirstName = "Joe",

                             LastName = "Smith",
                             CompanyName = "Stellar Solutions Co.",
                             Phone = GeneratePhoneNumber(random),
                             AddressCountry = "Canada",
                             Email = "SmithJoe1@outlook.com",
                             AddressStreet = "21 example Rd.",
                             PostalCode = "LC34N7"
                         },
                         new Client
                         {
                             FirstName = "Jack",

                             LastName = "Jones",
                             CompanyName = "Nexus Dynamics Inc.",
                             Phone = GeneratePhoneNumber(random),
                             AddressCountry = "Canada",
                             Email = "JackJHjones@outlook.com",
                             AddressStreet = GenerateAddress(random),
                             PostalCode = GeneratePostalCode(random)
                         }
                        );
                    context.SaveChanges();
                }


                // Seed Projects if there aren't any.
                if (!context.Projects.Any())
                {
                    context.Projects.AddRange(
                    new Project
                    {
                        ProjectName = "Project 1",

                        StartTime = DateTime.Parse("2023-06-15"),
                        EndTime = DateTime.Parse("2023-07-08"),
                        ProjectSite = "Back deck 1",
                        SetupNotes = "Notes here",


                        ClientID = context.Clients.FirstOrDefault(c => c.FirstName == "Amy" && c.LastName == "Benson").ID

                    },
                     new Project
                     {
                         ProjectName = "Project 2",

                         StartTime = DateTime.Parse("2023-06-15"),
                         EndTime = DateTime.Parse("2023-07-08"),
                         ProjectSite = "Back deck 1",

                         SetupNotes = "Notes here",

                         ClientID = context.Clients.FirstOrDefault(c => c.ID == 2).ID

                     },
                      new Project
                      {
                          ProjectName = "Project 3",

                          StartTime = DateTime.Parse("2023-06-15"),
                          EndTime = DateTime.Parse("2023-07-08"),
                          ProjectSite = "Back deck 1",

                          SetupNotes = "Notes here",

                          ClientID = context.Clients.FirstOrDefault(c => c.ID == 1).ID

                      },
                       new Project
                       {
                           ProjectName = "Project 4",

                           StartTime = DateTime.Parse("2023-06-15"),
                           EndTime = DateTime.Parse("2023-07-08"),
                           ProjectSite = "Back deck 4",

                           SetupNotes = "Notes here",

                           ClientID = context.Clients.FirstOrDefault(c => c.ID == 2).ID

                       },
                        new Project
                        {
                            ProjectName = "Project 5",

                            StartTime = DateTime.Parse("2023-06-15"),
                            EndTime = DateTime.Parse("2023-07-08"),
                            ProjectSite = "Back deck 5",

                            SetupNotes = "Notes here",

                            ClientID = context.Clients.FirstOrDefault(c => c.ID == 3).ID

                        }
                  );
                    context.SaveChanges();
                }

                #region random Clients

                //Because we can, lets add a bunch more Customers and Functions


                //Add more clients
                if (context.Clients.Count() < 6)
                {
                    string[] companyNames = new string[] { "Consolidated Credit Union", "PEI  Womens Institute", "Atlantic Psychiatric Conference", "TLC Laser Eye Centers", "Cooperators Insurance", "Department of Education", "PEI Teacher's Federation", "Credit Union Managers", "International Union of Operating Engineers", "Department of Transportation & Public Works", "Department of Fisheries & Oceans", "Summerside Tax Center", "Summerside Community Church", "Health & Social Services", "Department of Education", "Bridge Tournament Tour", "Malpeque bay Credit Union", "Atlantic provinces Chamber of  Commerce", "Child & Family Services", "Canadian mental Health assoc.", "PEI Institute if Agrologists", "Acadian Fishermans Coop", "Toronto Dominion Bank", "Highfield Construction", "Dept of Fisheries & Oceans", "Canadian Motor Veichle Arbitration Plan", "PEI Real Estate Assoc.", "Heart & Stroke Foundation", "East Prince Youth Development Centre", "PEI Assoc of Exhibitions", "PEI Potato Processing Council", "Union of Public sector Employees", "Cavendish Agri services", "PEI Pharmaceutical Assoc", "Zellers District Office", "UPEI  Faculty of Education", "College of Family Physicians-PEI chapter", "Dept of Education and Early Childhood Development", "Aerospace Industries Assoc of Canada", "Dept of Transportation", "Mark's Work Warehouse", "Cavendish Agri-Services", "Baie Acadienne Dev. Corp", "Primerica Financial Services", "Holland College-Adult Education", "Downtown  Summerside Inc.", "PEI Hairdressers Assoc", "Occupational Health & Safety Division-WCB", "BTC-Building Tennis Communities", "PEI Institute of Agrologists", "Agriculture Insurance Corp", "Island Health Training Center", "PEI Federation of Agriculture", "Cavendish Agri Services", "Public  Service Alliance", "Loyalist Lakeview Resort", "Consolidated Credit Union", "Cusource-Credit Un.Cen.of N.S.", "Callbecks  Home Hardware", "Summerside Tax Center", "Summerside Tax center", "Genworth Financial Canada", "East Prince Health board", "Agricultural Insurance Corp", "Family Resource Center", "The National Chapter of Canada IODE", "Summerside Tax Center", "Pro Trans Personal Services", "Girls & Women in Sports", "Dept of Agriculture, Fisheries &Aquaculture", "PEI Automobile Dealers Assoc", "Spreadsheet Solutions", "PC Association of PEI-22ND DISTRICT", "Can Society of Safety Engineering", "Canadian Assoc of Property & Home Inspectors", "Egg Producers Board of PEI", "Key, McKnight & Maynard", "Agriculture & Agri Food Canada", "Atlantic Canada Oppurtunities", "Premier World Tours Canadian Maritimes Pioneer" };
                    string[] firstNames = new string[] { "Woodstock", "Violet", "Charlie", "Lucy", "Linus", "Franklin", "Marcie", "Schroeder" };
                    string[] lastNames = new string[] { "Hightower", "Broomspun", "Jones", "Bloggs", "Brown", "Smith", "Daniel" };
                    int companyNameCount = companyNames.Length;
                    string[] postalLetters = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", };

                    foreach (string l in lastNames)
                    {
                        foreach (string f in firstNames)
                        {

                            Client c = new Client()
                            {
                                FirstName = f,
                                MiddleName = l[1].ToString().ToUpper(),
                                LastName = l,
                                CompanyName = companyNames[random.Next(0, companyNameCount)],
                                Phone = random.Next(2, 10).ToString() + random.Next(213214131, 989898989).ToString(),
                                Email = $"{l}"+$"{f}"+ $"{random.Next(1,99)}" + "@outlook.com",
                                AddressCountry = "Canada",
                                AddressStreet = GenerateAddress(random),
                                PostalCode = GeneratePostalCode(random)
                            };
                            context.Clients.Add(c);
                        }
                    }
                    context.SaveChanges();

                }
                //create a collection of primary keys
                int[] clientIDs = context.Clients.Select(c => c.ID).ToArray();

                int clientIDCount = clientIDs.Length;

                //Create 5 notes from Bacon ipsum
                string[] baconNotes = new string[] { "Bacon ipsum dolor amet meatball corned beef kevin, alcatra kielbasa biltong drumstick strip steak spare ribs swine. Pastrami shank swine leberkas bresaola, prosciutto frankfurter porchetta ham hock short ribs short loin andouille alcatra. Andouille shank meatball pig venison shankle ground round sausage kielbasa. Chicken pig meatloaf fatback leberkas venison tri-tip burgdoggen tail chuck sausage kevin shank biltong brisket.", "Sirloin shank t-bone capicola strip steak salami, hamburger kielbasa burgdoggen jerky swine andouille rump picanha. Sirloin porchetta ribeye fatback, meatball leberkas swine pancetta beef shoulder pastrami capicola salami chicken. Bacon cow corned beef pastrami venison biltong frankfurter short ribs chicken beef. Burgdoggen shank pig, ground round brisket tail beef ribs turkey spare ribs tenderloin shankle ham rump. Doner alcatra pork chop leberkas spare ribs hamburger t-bone. Boudin filet mignon bacon andouille, shankle pork t-bone landjaeger. Rump pork loin bresaola prosciutto pancetta venison, cow flank sirloin sausage.", "Porchetta pork belly swine filet mignon jowl turducken salami boudin pastrami jerky spare ribs short ribs sausage andouille. Turducken flank ribeye boudin corned beef burgdoggen. Prosciutto pancetta sirloin rump shankle ball tip filet mignon corned beef frankfurter biltong drumstick chicken swine bacon shank. Buffalo kevin andouille porchetta short ribs cow, ham hock pork belly drumstick pastrami capicola picanha venison.", "Picanha andouille salami, porchetta beef ribs t-bone drumstick. Frankfurter tail landjaeger, shank kevin pig drumstick beef bresaola cow. Corned beef pork belly tri-tip, ham drumstick hamburger swine spare ribs short loin cupim flank tongue beef filet mignon cow. Ham hock chicken turducken doner brisket. Strip steak cow beef, kielbasa leberkas swine tongue bacon burgdoggen beef ribs pork chop tenderloin.", "Kielbasa porchetta shoulder boudin, pork strip steak brisket prosciutto t-bone tail. Doner pork loin pork ribeye, drumstick brisket biltong boudin burgdoggen t-bone frankfurter. Flank burgdoggen doner, boudin porchetta andouille landjaeger ham hock capicola pork chop bacon. Landjaeger turducken ribeye leberkas pork loin corned beef. Corned beef turducken landjaeger pig bresaola t-bone bacon andouille meatball beef ribs doner. T-bone fatback cupim chuck beef ribs shank tail strip steak bacon." };

                #endregion

                #region Add Random Projects
                if (context.Projects.Count() < 6)
                {
                    string[] projectsNames = new string[] { "Garden Renovation", "Backyard Oasis Design", "Front Yard Transformation", "Water Feature Installation", "Xeriscaping Project", "Outdoor Living Space Enhancement", "Pathway and Walkway Redesign", "Tree Planting and Maintenance", "Native Plant Restoration", "Terrace Garden Installation", "Patio Extension and Design", "Drought-Tolerant Landscape Overhaul", "Zen Garden Creation", "Perennial Flowerbed Installation", "Rock Garden Construction", "Fence and Gate Enhancement", "Arbor and Pergola Installation", "Sustainable Lawn Care Implementation", "Vegetable and Herb Garden Setup", "Outdoor Lighting Upgrade" };

                    string[] projectSites = new string[] { "Residential Property", "Commercial Office Park", "Public Park", "Shopping Mall", "School Campus", "Hospital Grounds", "Hotel Courtyard", "Restaurant Outdoor Dining Area", "Golf Course", "Apartment Complex", "Industrial Complex", "Government Building", "Sports Stadium", "Botanical Garden", "Historical Site", "Community Center", "Campground", "Theme Park", "Highway Rest Stop", "Airport Terminal" };
                    DateTime startd = DateTime.Today;
                    DateTime firstdate = DateTime.Parse("2000-01-01");
                    int projectsCount = projectsNames.Length;
                    int counter = 1;

                    foreach (string ln in projectSites)
                    {
                        HashSet<string> selectedNames = new HashSet<string>();

                        while (selectedNames.Count() < 5)
                        {
                            selectedNames.Add(projectsNames[random.Next(projectsCount)]);
                        }

                        foreach (string fn in projectsNames)
                        {
                            Project project = new Project()
                            {
                                ProjectName = fn,
                                StartTime = DateTime.Today.AddDays(-random.Next(1, 365)),
                                EndTime = DateTime.Today.AddDays(random.Next(1, 365)),
                                ProjectSite = ln,

                                SetupNotes = baconNotes[random.Next(5)],
                                ClientID = clientIDs[random.Next(clientIDCount)]


                            };
                            counter++;
                            context.Projects.Add(project);


                        }

                    }
                    context.SaveChanges();


                    string cmd = "DELETE FROM Clients WHERE NOT EXISTS(SELECT 1 FROM Projects WHERE Clients.Id = Projects.ClientID)";
                    context.Database.ExecuteSqlRaw(cmd);
                }


                #endregion

                //Seed data for Materials
                //Start with Inventory
                string[] iventoryItems = new string[] {
                 "Mulch",
    "Gravel",
    "Pavers",
    "Topsoil",
    "Plants",
    "Rocks",
    "Edging",
    "Decorative Stones",
    "Sand",
    "Bark",
    "Sod",
    "Timber",
    "Fertilizer",
    "Seeds",
    "Compost",
    "Mulch Mats",
    "Landscape Fabric",
    "Water Features",
    "Garden Tools",
    "Irrigation Systems"
                };
                if (!context.Materials.Any())
                {
                    double min = 5.0;

                    double maxx = 100.0;
                    foreach (string i in iventoryItems)
                    {
                        Material inventory = new Material()
                        {
                            Name = i,
                            Description = baconNotes[random.Next(5)],
                            Price = random.NextDouble() * (maxx - min) + min

                        };
                        context.Materials.Add(inventory);
                    }
                    context.SaveChanges();
                }

                int[] InventoryIDs = context.Materials.Select(i => i.ID).ToArray();
                int InventoryIDCount = InventoryIDs.Length;

                int[] projectIDs = context.Projects.Select(p => p.ID).ToArray();
                int projectIDCount = projectIDs.Length;

                clientIDs = context.Clients.Select(c => c.ID).ToArray();
                clientIDCount = clientIDs.Length;
                /*Materials
                if (!context.Materials.Any())
                {
                    foreach(int i in projectIDs)
                    {

                    int k = 0;
                        double min = 1.0;
                        double max = 15.0;
                        int howMany = random.Next(1, InventoryIDCount);
                    for(int j =1; j <= howMany; j++)
                    {
                        k = (k >= InventoryIDCount) ? 0 : k;
                        Material m = new Material()
                        {
                            
                            InventoryID = InventoryIDs[k],
                            Quantity = random.Next(1,100),
                            Description = baconNotes[random.Next(5)],
                            PerYardCharge = random.NextDouble() * (max - min) + min


                        };
                        context.Materials.Add(m);
                    }
                    context.SaveChanges();
                    }
                }
                */

                //Seed Labour Types
                string[] laboursTypes = new string[]
                {
                    "Lawn Mowing",
    "Weeding",
    "Hedge Trimming",
    "Mulching",
    "Planting",
    "Pruning",
    "Sodding",
    "Aerating",
    "Fertilizing",
    "Watering",
    "Leaf Removal",
    "Edging",
    "Seeding",
    "Irrigation Installation",
    "Tree Care",
    "Landscape Design",
    "Pest Control",
    "Outdoor Lighting Installation"
                };
                //Labour
                int[] projectIDss = context.Projects.Select(p => p.ID).ToArray();
                int projectIDCountt = projectIDss.Length;
                int[] typesIDs = context.Labours.Select(t => t.ID).ToArray();
                int typesIDCount = typesIDs.Length;
                int[] materialIDs = context.Materials.Select(m => m.ID).ToArray();
                string[] lDescriptions = new string[]
                {
                    "Cutting grass to maintain an even height.",
    "Removing unwanted plants from gardens and other areas.",
    "Pruning and shaping hedges to desired form.",
    "Applying a layer of material to the soil surface to conserve moisture and suppress weeds.",
    "Installing new plants, flowers, shrubs, or trees.",
    "Trimming back overgrown branches to improve plant health and appearance.",
    "Laying down pre-grown grass patches to establish a new lawn.",
    "Loosening compacted soil to improve air and water circulation.",
    "Applying nutrients to soil to promote plant growth and health.",
    "Providing adequate moisture to plants, especially during dry periods.",
    "Clearing fallen leaves from lawns, walkways, and other areas.",
    "Creating clean borders between different landscape elements.",
    "Planting grass seed to establish a new lawn or fill in patches.",
    "Installing systems to efficiently water landscapes.",
    "Pruning, trimming, and maintaining trees for health and appearance.",
    "Planning and designing outdoor spaces for aesthetic appeal and functionality.",
    "Managing pests that can damage plants and landscapes.",
    "Installing lights to enhance visibility and safety in outdoor spaces."

                };

                if (!context.Labours.Any())
                {
                    foreach (string t in laboursTypes)
                    {
                        Labour type = new Labour
                        {
                            Name = t,
                            Description = lDescriptions[random.Next(18)],
                            Price = random.NextDouble() * (70.0 - 16.0) + 16.0
                        };
                        context.Labours.Add(type);
                    }
                    context.SaveChanges();
                }

                /* if (!context.Labours.Any())
                 {
                     int k = 0;
                     double min = 35.0;
                     double max = 40.0;

                     foreach (int i in projectIDss)
                     {
                         int howMany = random.Next(1, typesIDCount);
                         for(int j =1; j <= howMany; j++)
                         {
                             k = (k >= typesIDCount) ? 0 : k;
                             Labour l = new Labour()
                             {
                                 Name = 
                                 LabourTypeID = typesIDs[random.Next(typesIDCount)],
                                 LabourHours = random.NextDouble() * (max - min) + min,
                                 LabourDescription = lDescriptions[random.Next(18)],
                                 LabourUnitPrice = random.NextDouble() * (20.0 - 16.0) + 16.0
                             };
                             k++;
                             context.Labours.Add(l);
                         }
                         context.SaveChanges();
                     }
                 }*/

                if (!context.Bids.Any())
                {
                    int k = 0;
                    foreach (int i in projectIDss)
                    {
                        Bid b = new Bid()
                        {
                            BidDate = DateTime.Today.AddDays(-random.Next(1, 365)),


                            ProjectID = projectIDss[random.Next(projectIDCountt)]

                        };
                        k++;
                        context.Bids.Add(b);
                    }
                    context.SaveChanges();
                }

                //ExtraTABLE
                int[] materialsIDs = context.Materials.Select(s => s.ID).ToArray();
                int[] laboursIDs = context.Labours.Select(s => s.ID).ToArray();
                int[] bidIDs = context.Bids.Select(s => s.ID).ToArray();
                int materialCountID = materialsIDs.Length;
                int labourCountID = laboursIDs.Length;

                if (!context.BidLabours.Any())
                {
                    int k = 0;
                    foreach (int i in bidIDs)
                    {
                        int howMany = random.Next(1, 10);
                        for (int j = 0; j <= howMany; j++)
                        {
                            k = (k >= labourCountID) ? 0 : k;
                            BidLabour bl = new BidLabour()
                            {
                                BidID = i,
                                HoursQuantity = random.NextDouble() * (40.0 - 20.0) + 20.0,
                                LabourID = laboursIDs[j]
                            };
                            k++;
                            context.BidLabours.Add(bl);
                        }
                        context.SaveChanges();
                    }
                }

                if (!context.BidMaterials.Any())
                {
                    int k = 0;
                    foreach (int i in bidIDs)
                    {
                        int howMany = random.Next(1, 10);
                        for (int j = 0; j <= howMany; j++)
                        {
                            k = (k >= materialCountID) ? 0 : k;
                            BidMaterial bm = new BidMaterial()
                            {
                                BidID = i,
                                MaterialQuantity = random.Next(1, 50),
                                MaterialID = materialsIDs[j]
                            };
                            k++;
                            context.BidMaterials.Add(bm);
                        }
                        context.SaveChanges();
                    }
                }

             /*   if(!context.Employees.Any())
                {
                    context.Employees.AddRange(
                         new Employee
                         {
                             FirstName = "John",
                             LastName = "Doe",
                             Email = "admin@outlook.com"
                             
                         },
                          new Employee
                          {
                              FirstName = "Gregory",
                              LastName = "House",
                              Email = "designer@outlook.com"
                          },
                           new Employee
                           {
                               FirstName = "Jane",
                               LastName = "Doe",
                               Email = "super@outlook.com"
                           },
                           new Employee
                           {
                               FirstName = "Fred",
                               LastName = "Flintstone",
                               Email = "sales@outlook.com"
                           },
                      new Employee
                      {
                          FirstName = "Betty",
                          LastName = "Rubble",
                          Email = "user@outlook.com"
                      });
                    context.SaveChanges(); 
                }*/


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
            }
            Debug.WriteLine("Seed process completed.");
        }
    }
}
