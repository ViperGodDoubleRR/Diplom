using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Interface
{
    public interface IResRepository
    {
         Task<bool> ResRequestCode(string email,string code,CancellationToken cancellation);
         Task<bool> CheckCode(string email, string code, CancellationToken cancellationToken);
         Task<bool> ResetPassword(string email, string newpassword, CancellationToken cancellationToken);
    }
}
