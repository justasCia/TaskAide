using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Repositories;

namespace TaskAide.API.Services.Categories
{
    public class CategoryDbSeeder
    {
        private readonly ICategoryRepository _categoryRepository;

        private readonly List<Category> _categories = new List<Category>()
        {
            new Category() { Name = "Valymas", Services = new List<Service>()
                {
                    new Service() { Name = "Namų valymas" },
                    new Service() { Name = "Ofiso valymas" },
                    new Service() { Name = "Valymas po remonto darbų" },
                    new Service() { Name = "Langų valymas" },
                    new Service() { Name = "Minkštų baldų valymas" },
                    new Service() { Name = "Kilimų valymas" }
                }
            },
            new Category() { Name = "Elektros darbai", Services = new List<Service>()
                {
                    new Service() { Name = "Elektros instaliacijos darbai" },
                    new Service() { Name = "Elektros remonto darbai" }
                }
            },
            new Category() { Name = "Santechnikos darbai", Services = new List<Service>()
                {
                    new Service() { Name = "Santechnikos montavimas" },
                    new Service() { Name = "Santecnikos remontas" },
                }
            },
            new Category() { Name = "Statybos darbai", Services = new List<Service>()
                {
                    new Service() { Name = "Apdailos darbai" },
                    new Service() { Name = "Griovimo darbai" },
                    new Service() { Name = "Sienų tinkavimas" },
                    new Service() { Name = "Kaminų valymas" }
                }
            },
            new Category() { Name = "Buitinės technikos pajungimas", Services = new List<Service>()
                {
                    new Service() { Name = "Buitinės technikos įrengimas" },
                    new Service() { Name = "Buitinės technikos remontas" }
                }
            },
            new Category() { Name = "Vidaus apdailos darbai", Services = new List<Service>()
                {
                    new Service() { Name = "Durų montavimas" },
                    new Service() { Name = "Langų montavimas" },
                    new Service() { Name = "Plytelių klojimas" },
                    new Service() { Name = "Tapetavimas" },
                    new Service() { Name = "Sienų ir lubų apdaila" },
                    new Service() { Name = "Sienų dažymas" },
                    new Service() { Name = "Staliaus darbai" },
                    new Service() { Name = "Plastikinių langų reguliavimas" },
                    new Service() { Name = "Įtempiamų lubų montavimas" },
                    new Service() { Name = "Gipso kartono plokščių montavimas" },
                    new Service() { Name = "Apšiltinimas" },
                    new Service() { Name = "Garso izoliavimas" },
                    new Service() { Name = "Karnizų montavimas" },
                    new Service() { Name = "Užuolaidų keitimas" }
                }
            },
            new Category() { Name = "Baldų surinkimas", Services = new List<Service>()
                {
                    new Service() { Name = "Baldų surinkimas" },
                    new Service() { Name = "Virtuvės baldų surinkimas" }
                }
            },
            new Category() { Name = "Pervežimas", Services = new List<Service>()
                {
                    new Service() { Name = "Baldų pervežimas" },
                    new Service() { Name = "Statybinių medžiagų išvežimas" },
                    new Service() { Name = "Sunkių daiktų pervežimas" },
                    new Service() { Name = "Daiktų supakavimas ir išpakavimas" }
                }
            },
            new Category() { Name = "Aplinkos tvarkymas ir priežiūra", Services = new List<Service>()
                {
                    new Service() { Name = "Žolės pjovimas" },
                    new Service() { Name = "Sniego valymas" },
                    new Service() { Name = "Aplinkos tvarkymas" },
                    new Service() { Name = "Grindinio klojimas" },
                    new Service() { Name = "Grindinio plovimas" },
                    new Service() { Name = "Kapų tvarkymas" }
                }
            },
            new Category() { Name = "Kondicionavimo sistemų bei vėdinimo sistemų darbai", Services = new List<Service>()
                {
                    new Service() { Name = "Kondicionierių priežiūra" },
                    new Service() { Name = "Kondicionierių pildymas" },
                    new Service() { Name = "Kondicionierių remontas" },
                    new Service() { Name = "Kondicionierių meistro konsultacija" },
                    new Service() { Name = "Vėdinimo sistemų remontas" },
                    new Service() { Name = "Vėdinimo sistemų programavimas" },
                    new Service() { Name = "Vėdinimo sistemos apžiūra" },
                    new Service() { Name = "Vėdinimo sistemos meistro konsultacija" }
                }
            },
            new Category() { Name = "Apsaugos darbai", Services = new List<Service>()
                {
                    new Service() { Name = "Vartų automatikos montavimas" },
                    new Service() { Name = "Vartų automatikos remontas" },
                    new Service() { Name = "Apsaugos sistemų įrengimas" },
                    new Service() { Name = "Apsaugos sistemų remontas" },
                    new Service() { Name = "Vaizdo stebėjimo sistemų pajungimas" },
                    new Service() { Name = "Vaizdo stebėjimo sistemų remontas" },
                    new Service() { Name = "Priešgaistrinių daviklių instaliacija" },
                    new Service() { Name = "Priešgaistrinių daviklių remontas" }
                }
            },
            new Category() { Name = "Dujų ūkio darbai", Services = new List<Service>()
                {
                    new Service() { Name = "Dujų baliono keitimas" },
                    new Service() { Name = "Dujų įvedimas" }
                }
            }
        };

        public CategoryDbSeeder(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task SeedAsync()
        {
            await AddCategories();
        }

        private async Task AddCategories()
        {
            foreach (var category in _categories)
            {
                var categoryExists = await _categoryRepository.GetAsync(c => c.Name == category.Name);

                if (categoryExists == null)
                {
                    await _categoryRepository.AddAsync(category);
                }
            }
        }
    }
}
