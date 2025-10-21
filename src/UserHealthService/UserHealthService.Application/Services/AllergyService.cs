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

            // Manual mapping for update
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
            
            var potentialConflicts = userAllergies
                .Where(a => medicationName.Contains(a.AllergenName, StringComparison.OrdinalIgnoreCase) ||
                           a.AllergenName.Contains(medicationName, StringComparison.OrdinalIgnoreCase))
                .Select(a => new
                {
                    Allergy = MapToResponseDto(a),
                    ConflictReason = $"Medication '{medicationName}' may contain or relate to allergen '{a.AllergenName}'"
                })
                .ToList();

            return new
            {
                HasConflicts = potentialConflicts.Any(),
                Conflicts = potentialConflicts,
                UserId = userId,
                MedicationName = medicationName
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