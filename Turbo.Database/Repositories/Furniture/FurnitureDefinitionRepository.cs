﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Turbo.Database.Context;
using Turbo.Core.Database.Entities.Furniture;

namespace Turbo.Database.Repositories.Furniture;

public class FurnitureDefinitionRepository(IEmulatorContext _context) : IFurnitureDefinitionRepository
{
    public async Task<FurnitureDefinitionEntity> FindAsync(int id)
    {
        return await _context.FurnitureDefinitions
            .FirstOrDefaultAsync(definition => definition.Id == id);
    }

    public async Task<List<FurnitureDefinitionEntity>> FindAllAsync()
    {
        return await _context.FurnitureDefinitions
            .AsNoTracking()
            .ToListAsync();
    }
}