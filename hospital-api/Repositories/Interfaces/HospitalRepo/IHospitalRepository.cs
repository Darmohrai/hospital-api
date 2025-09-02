using hospital_api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Repositories.Interfaces.HospitalRepo;

public interface IHospitalRepository : IRepository<Hospital>
{
}
