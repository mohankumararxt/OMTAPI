using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Context
{
    public class DbInitializer : IDbInitializer
    {
        private readonly OMTDataContext _context;

        public DbInitializer(OMTDataContext context)
        {
            _context = context;
        }
        public void Initialize()
        {
            _context.Database.EnsureCreated();
        }

        public void SeedDatabase()
        {

        }
    }
}
