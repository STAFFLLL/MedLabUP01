﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MedLab
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MedLabEntities : DbContext
    {
        public MedLabEntities()
            : base("name=MedLabEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AccountForInsuranceCompany> AccountForInsuranceCompanies { get; set; }
        public virtual DbSet<Analyzer> Analyzers { get; set; }
        public virtual DbSet<AnalyzerData> AnalyzerDatas { get; set; }
        public virtual DbSet<History> Histories { get; set; }
        public virtual DbSet<InsuranceCompany> InsuranceCompanies { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderAndService> OrderAndServices { get; set; }
        public virtual DbSet<PolicyType> PolicyTypes { get; set; }
        public virtual DbSet<ReasonsForExit> ReasonsForExits { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<StatusAccount> StatusAccounts { get; set; }
        public virtual DbSet<StatusOrder> StatusOrders { get; set; }
        public virtual DbSet<StatusServicesInOrder> StatusServicesInOrders { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserType> UserTypes { get; set; }
    }
}
