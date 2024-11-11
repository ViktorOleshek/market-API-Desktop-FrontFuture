﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abstraction.IRepositories;
using Abstraction.Models;
using AutoMapper;
using Data.Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class ReceiptRepository : AbstractRepository<Receipt, ReceiptModel>, IReceiptRepository
    {
        public ReceiptRepository(TradeMarketDbContext context, IMapper mapper)
            : base(context, mapper)
        {
            ArgumentNullException.ThrowIfNull(context);
        }

        public async Task<IEnumerable<ReceiptModel>> GetAllWithDetailsAsync()
        {
            return await this.Context.Set<Receipt>()
                .Include(e => e.ReceiptDetails)
                    .ThenInclude(e => e.Product)
                        .ThenInclude(e => e.Category)
                .Include(e => e.Customer)
                    .ThenInclude(e => e.Person)
                .ToListAsync();
        }

        public Task<ReceiptModel> GetByIdWithDetailsAsync(int id)
        {
            return this.Context.Set<Receipt>()
                .Include(e => e.ReceiptDetails)
                    .ThenInclude(e => e.Product)
                        .ThenInclude(e => e.Category)
                .Include(e => e.Customer)
                    .ThenInclude(e => e.Person)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
