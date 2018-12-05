using MongoDB.Driver;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sourceportal.SAP.Domain.Models.DB.Organizations;

namespace Sourceportal.SAP.DB.Organizations
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["MongoConnection"].ConnectionString;
        private readonly MongoClient _client;
        private static readonly string MongoDbName = ConfigurationManager.AppSettings["MongoSapDbName"];

        public OrganizationRepository()
        {
            _client = new MongoClient(ConnectionString);
        }

        private IMongoCollection<OrganizationDb> GetOrganizationCollection()
        {
            var database = _client.GetDatabase(MongoDbName);
            return database.GetCollection<OrganizationDb>("Organizations");
        }

        private IMongoCollection<CompanyDb> GetCompanyCollection()
        {
            var database = _client.GetDatabase(MongoDbName);
            return database.GetCollection<CompanyDb>("Companies");
        }

        public List<OrganizationDb> GetAllOrganizations()
        {
            return GetOrganizationCollection().Find(x => true).ToList();
        }

        public List<CompanyDb> GetAllCompanies()
        {
            return GetCompanyCollection().Find(x => true).ToList();
        }

    }
}
