using System;
using Dal.SourceGen.Abstractions.Attributes;

namespace DAL.SourceGen.Sample;

[GenerateRepository]
public class User
{
   public Guid Id { get; set; } 
   
   public string Name { get; set; }
}