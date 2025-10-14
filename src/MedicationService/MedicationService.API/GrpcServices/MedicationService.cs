using Grpc.Core;
using MedicationService.Protos;

namespace MedicationService.API.Services
{
    public class MedicationGrpcService : Medication.MedicationBase
    {
        public override Task<MedicationResponse> GetMedicationInfo(MedicationRequest request, ServerCallContext context)
        {
            var med = new MedicationResponse
            {
                Id = request.Id,
                Name = "Ibuprofen",
                Dosage = "200mg"
            };
            return Task.FromResult(med);
        }
    }
}
