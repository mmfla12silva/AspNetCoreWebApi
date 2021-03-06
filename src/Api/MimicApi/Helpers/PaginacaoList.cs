﻿using MimicApi.V1.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicApi.Helpers
{
    public class PaginacaoList<T> 
    {
        public List<T> Results { get; set; } = new List<T>();
        public Paginacao Paginacao { get; set; }
        public List<LinkDTO> Links { get; set; }
    }
}
