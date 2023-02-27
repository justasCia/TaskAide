using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Repositories;

namespace TaskAide.API.Services.Categories
{
    public class CategoryDbSeeder
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly List<string> _categories = new List<string>()
        {
            "Valymas",
            "Elektros darbai",
            "Santechnikos darbai",
            "Statybos darbai",
            "Buitinės technikos pajungimas",
            "Vidaus apdailos darbai",
            "Baldų surinkimas",
            "Namų remontas",
            "Pervežimas",
            "Aplinkos tvarkymas ir priežiūra",
            "Kondicionavimo sistemų bei vėdinimo sistemų darbai",
            "Apsaugos darbai",
            "Dujų ūkio darbai"
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
                var categoryExists = await _categoryRepository.GetAsync(c => c.Name == category);

                if (categoryExists == null)
                {
                    await _categoryRepository.AddAsync(new Category() { Name = category });
                }
            }
        }
    }
}
