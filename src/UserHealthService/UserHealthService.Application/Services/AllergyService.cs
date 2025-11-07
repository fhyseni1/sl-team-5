using UserHealthService.Application.DTOs.Allergies;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Services
{
    public class AllergyService : IAllergyService
    {
        private readonly IAllergyRepository _allergyRepository;

        public AllergyService(IAllergyRepository allergyRepository)
        {
            _allergyRepository = allergyRepository;
        }

        public async Task<AllergyResponseDto?> GetByIdAsync(Guid id)
        {
            var allergy = await _allergyRepository.GetByIdAsync(id);
            return allergy == null ? null : MapToResponseDto(allergy);
        }

        public async Task<IEnumerable<AllergyResponseDto>> GetAllAsync()
        {
            var allergies = await _allergyRepository.GetAllAsync();
            return allergies.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<AllergyResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var allergies = await _allergyRepository.GetByUserIdAsync(userId);
            return allergies.Select(MapToResponseDto);
        }

        public async Task<int> GetAllergyCountAsync(Guid userId)
        {
            return await _allergyRepository.CountByUserIdAsync(userId);
        }

        public async Task<AllergyResponseDto> CreateAsync(AllergyCreateDto createDto)
        {
            var allergy = new Allergy
            {
                Id = Guid.NewGuid(),
                UserId = createDto.UserId,
                AllergenName = createDto.AllergenName,
                Description = createDto.Description,
                Severity = createDto.Severity,
                Symptoms = createDto.Symptoms,
                Treatment = createDto.Treatment,
                DiagnosedDate = createDto.DiagnosedDate,
                DiagnosedBy = createDto.DiagnosedBy,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var created = await _allergyRepository.AddAsync(allergy);
            return MapToResponseDto(created);
        }

        public async Task<AllergyResponseDto?> UpdateAsync(Guid id, AllergyUpdateDto updateDto)
        {
            var existingAllergy = await _allergyRepository.GetByIdAsync(id);
            if (existingAllergy == null) return null;

            existingAllergy.AllergenName = updateDto.AllergenName;
            existingAllergy.Description = updateDto.Description;
            existingAllergy.Severity = updateDto.Severity;
            existingAllergy.Symptoms = updateDto.Symptoms;
            existingAllergy.Treatment = updateDto.Treatment;
            existingAllergy.DiagnosedDate = updateDto.DiagnosedDate;
            existingAllergy.DiagnosedBy = updateDto.DiagnosedBy;
            existingAllergy.IsActive = updateDto.IsActive;
            existingAllergy.UpdatedAt = DateTime.UtcNow;

            var updated = await _allergyRepository.UpdateAsync(existingAllergy);
            return MapToResponseDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _allergyRepository.DeleteAsync(id);
        }

            public async Task<object> CheckAllergyConflictsAsync(Guid userId, string medicationName)
        {
            var userAllergies = await _allergyRepository.GetByUserIdAsync(userId);
            var commonAllergyConflicts = new Dictionary<string, List<string>>
            {
                { "Penicillin", new List<string> { "penicillin", "amoxicillin", "ampicillin", "amoxil", "augmentin" } },
                { "Sulfa", new List<string> { "sulfa", "sulfamethoxazole", "sulfasalazine", "bactrim", "septra" } },
                { "Aspirin", new List<string> { "aspirin", "salicylate", "asa", "ibuprofen", "naproxen" } },
                { "Ibuprofen", new List<string> { "ibuprofen", "advil", "motrin", "nuprin", "naproxen" } },
                { "Codeine", new List<string> { "codeine", "hydrocodone", "oxycodone", "vicodin", "percocet" } },
                { "Cephalosporins", new List<string> { "cephalexin", "ceftriaxone", "cefuroxime", "cefdinir" } },
                { "NSAIDs", new List<string> { "ibuprofen", "naproxen", "diclofenac", "celecoxib", "indomethacin" } }
            };

            var potentialConflicts = new List<object>();
            var medicationNameLower = medicationName.ToLowerInvariant();

            foreach (var allergy in userAllergies)
            {
            
                if (medicationNameLower.Contains(allergy.AllergenName.ToLowerInvariant()) ||
                    allergy.AllergenName.ToLowerInvariant().Contains(medicationNameLower))
                {
                    potentialConflicts.Add(new
                    {
                        Allergy = MapToResponseDto(allergy),
                        ConflictReason = $"Medication name contains allergen '{allergy.AllergenName}'",
                        Severity = "High"
                    });
                    continue;
                }

                if (commonAllergyConflicts.ContainsKey(allergy.AllergenName))
                {
                    var relatedMeds = commonAllergyConflicts[allergy.AllergenName];
                    if (relatedMeds.Any(med => medicationNameLower.Contains(med)))
                    {
                        potentialConflicts.Add(new
                        {
                            Allergy = MapToResponseDto(allergy),
                            ConflictReason = $"Medication is related to known allergen '{allergy.AllergenName}'",
                            Severity = "High"
                        });
                    }
                }
            }

            return new
            {
                HasConflicts = potentialConflicts.Any(),
                Conflicts = potentialConflicts,
                UserId = userId,
                MedicationName = medicationName,
                CheckedAt = DateTime.UtcNow
            };
        }

        private AllergyResponseDto MapToResponseDto(Allergy allergy)
        {
            return new AllergyResponseDto
            {
                Id = allergy.Id,
                UserId = allergy.UserId,
                AllergenName = allergy.AllergenName,
                Description = allergy.Description,
                Severity = allergy.Severity,
                Symptoms = allergy.Symptoms,
                Treatment = allergy.Treatment,
                DiagnosedDate = allergy.DiagnosedDate,
                DiagnosedBy = allergy.DiagnosedBy,
                IsActive = allergy.IsActive,
                CreatedAt = allergy.CreatedAt,
                UpdatedAt = allergy.UpdatedAt
            };
        }
    }
}