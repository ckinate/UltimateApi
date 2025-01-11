using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(RepositoryContext repositoryContext): base(repositoryContext)
        {
        }

        public void CreateCompany(Company company)
        {
            Create(company);
        }

        public async Task< IEnumerable<Company>> GetAllCompaniesAsync(bool trackChanges)
        {
           var getAllCompany = await FindAll(trackChanges).OrderBy(c=>c.Name).ToListAsync();
            return getAllCompany;
        }

        public async Task <Company> GetCompanyAsync(Guid companyId, bool trackChanges)
        {
           var getCompany =  await FindByCondition(c => c.Id.Equals(companyId), trackChanges).SingleOrDefaultAsync();

            if (getCompany is null)
            {
                return  new Company();
            }

            return getCompany;
        }

        public async Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges) => await FindByCondition(x => ids.Contains(x.Id), trackChanges).ToArrayAsync();

        public void DeleteCompany(Company company) => Delete(company);
    }
}
