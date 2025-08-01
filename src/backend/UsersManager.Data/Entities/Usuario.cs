﻿using System;
using System.Collections.Generic;

namespace UsersManager.Data.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty; 
        public DateTime DataCadastro { get; set; } 
        public string Status { get; set; } = string.Empty; 

        public List<Acesso> Acessos { get; internal set; } = new List<Acesso>(); 


        public ICollection<PerfilUsuario> PerfisUsuario { get; set; } = new List<PerfilUsuario>();
    }
}