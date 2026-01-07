using System;
using System.Threading.Tasks;
using DAL.SourceGen.Attributes;
using DAL.SourceGen.Sample.Entities;
using ServiceLifetime = DAL.SourceGen.Attributes.ServiceLifetime;

namespace DAL.SourceGen.Sample.Repositories;

[Repository(ServiceLifetime.Scoped)]
public interface IUserRepository: IRepository<User>
{
    
}