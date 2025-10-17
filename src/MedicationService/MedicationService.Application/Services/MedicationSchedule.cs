using AutoMapper;
using MedicationService.Application.Interfaces;
using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.Services
{
    public class MedicationSchedule : IMedicationScheduleRepository
    {
        private readonly IMedicationScheduleRepository _repository;
        private readonly IMapper _mapper;
        public MedicationSchedule(IMedicationScheduleRepository repository,IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Domain.Entities.MedicationSchedule> AddAsync(Domain.Entities.MedicationSchedule entity)
        {
            return await _repository.AddAsync(entity);
        }

        public Task<int> CountAsync()
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Domain.Entities.MedicationSchedule entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Domain.Entities.MedicationSchedule>> FindAsync(Expression<Func<Domain.Entities.MedicationSchedule, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Domain.Entities.MedicationSchedule>> GetActiveSchedulesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Domain.Entities.MedicationSchedule>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Domain.Entities.MedicationSchedule?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Domain.Entities.MedicationSchedule>> GetByMedicationIdAsync(Guid medicationId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Domain.Entities.MedicationSchedule>> GetSchedulesByFrequencyAsync(FrequencyType frequency)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Domain.Entities.MedicationSchedule>> GetUpcomingSchedulesAsync(DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Domain.Entities.MedicationSchedule entity)
        {
            throw new NotImplementedException();
        }
    }
}
