using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Learnly.Domain.Entities;

namespace Learnly.Application.Interfaces
{
    public interface ILoginAplicacao
    {
        bool ValidarLogin(Usuario usuario, string senha);
        public string GenerateToken(int id, string email, string nome);
    }
}