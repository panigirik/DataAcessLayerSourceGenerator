using System;
using DAL.SourceGen.Attributes;

namespace DAL.SourceGen.Sample.Entities;

public class User
{
   public Guid Id { get; set; }
   public string Name { get; set; }
}