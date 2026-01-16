using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace users_service.application
{
    public interface IActivityService
    {
        Task<bool> RegisterActivityAsync(string email, string action, string category);
    }
}
