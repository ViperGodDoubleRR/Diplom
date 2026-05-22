using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RegService.Domain.IRepository
{
    public interface IRegRepository
    {
      Task<bool> CreateVerificationCode(string Email,string code,CancellationToken cancellation);
      Task<bool> CheckCode(string email, string code, CancellationToken cancellationToken);
      Task<bool> RegisterUser(Guid id ,string email, string login, CancellationToken cancellationToken);
    }
}
