using AutoMapper;
using UserHealthService.Application.DTOs.HealthMetrics;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Mappings
{
    public class HealthMetricProfile : Profile
    {
        public HealthMetricProfile()
        {
            CreateMap<HealthMetric, HealthMetricResponseDto>();
            CreateMap<HealthMetricCreateDto, HealthMetric>();
            CreateMap<HealthMetricUpdateDto, HealthMetric>();
        }
    }
}
