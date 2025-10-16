using MedicationService.Application.DTOs.Prescriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.Interfaces
{
    public interface IPrescriptionService
    {
        Task<PrescriptionResponseDto?> GetByIdAsync(Guid id);

        // Get all prescriptions
        Task<IEnumerable<PrescriptionResponseDto>> GetAllAsync();

        // Get prescriptions by medication ID
        Task<IEnumerable<PrescriptionResponseDto>> GetByMedicationIdAsync(Guid medicationId);

        // Get prescriptions that are expiring before a certain date
        Task<IEnumerable<PrescriptionResponseDto>> GetExpiringPrescriptionsAsync(DateTime beforeDate);

        // Get prescriptions with low remaining refills
        Task<IEnumerable<PrescriptionResponseDto>> GetLowRefillPrescriptionsAsync(int maxRefills);

        // Get prescription by prescription number
        Task<PrescriptionResponseDto?> GetByPrescriptionNumberAsync(string prescriptionNumber);

        // Create a new prescription
        Task<PrescriptionResponseDto> CreateAsync(PrescriptionCreateDto createDto);

        // Update an existing prescription
        Task<PrescriptionResponseDto?> UpdateAsync(Guid id, PrescriptionUpdateDto updateDto);

        // Delete a prescription
        Task<bool> DeleteAsync(Guid id);

        // Check if a prescription exists
        Task<bool> ExistsAsync(Guid id);
    }
}
