using System;

namespace DAL.SourceGen.Sample.Entities;

public class User
{
   public Guid Id { get; set; }
   public required string Name { get; set; }
}
