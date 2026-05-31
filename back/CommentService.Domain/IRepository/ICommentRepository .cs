using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommentService.Domain.Models;

namespace CommentService.Domain.IRepository
{
    public interface ICommentRepository
    {
        Task AddAsync(Comment comment);
    }
}
